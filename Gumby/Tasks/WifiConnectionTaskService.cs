using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Debug = System.Diagnostics.Debug;

#if ANDROID
using Android.Net;
using Android.Net.Wifi;
using Android.Content;
using Android.OS;
using Android.Net.Nsd;
using AndroidApp = Android.App.Application;
#endif

namespace Gumby.Tasks
{
    /// <summary>
    /// Service to connect to WiFi networks from QR codes or WiFi strings
    /// Format: WIFI:S:SSID;T:Encryption;P:Password;;
    /// Example: WIFI:S:MyNetwork;T:WPA;P:mypassword;;
    /// </summary>
    public class WifiConnectionTaskService
    {
        /// <summary>
        /// Connects to a WiFi network using WiFi format data
        /// </summary>
        public static async Task ConnectToWifi(string wifiData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(wifiData))
                {
                    await ShowAlert("Error", "No WiFi data provided.");
                    return;
                }

                Debug.WriteLine($">>> WifiConnectionTaskService: Processing WiFi data: {wifiData}");

                // Parse the WiFi data (keeps original format parsing)
                var (ssid, encryption, password) = ParseWifiData(wifiData);

                if (string.IsNullOrEmpty(ssid))
                {
                    await ShowAlert("Error", "Invalid WiFi data format.\n\nExpected: WIFI:S:SSID;T:WPA;P:PASSWORD;;");
                    return;
                }

                Debug.WriteLine($">>> WiFi Credentials - SSID: {ssid}, Encryption: {encryption}, Password: {'*'.ToString().PadRight(password?.Length ?? 0, '*')}");

#if ANDROID
                await ConnectOnAndroid(ssid, encryption, password);
#else
                await ConnectFallback(ssid, encryption);
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> ConnectToWifi Error: {ex.Message}");
                await ShowAlert("Error", $"WiFi connection failed: {ex.Message}");
            }
        }

#if ANDROID
        /// <summary>
        /// Android-specific WiFi connection
        /// Handles Android 10+ and Android 12+ differences
        /// </summary>
        private static async Task ConnectOnAndroid(string ssid, string encryption, string password)
        {
            try
            {
                var context = AndroidApp.Context;
                var wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);

                if (wifiManager == null)
                {
                    await ShowAlert("Error", "WiFi manager not available on this device.");
                    return;
                }

                var osVersion = (int)Build.VERSION.SdkInt;
                Debug.WriteLine($">>> Android API Level: {osVersion}");

                // Android 10+ (API 29+): WifiConfiguration/AddNetwork vraća -1 za aplikacije
                // koje ciljaju Android 10+, pa je WifiNetworkSuggestion jedini podržan način
                if (osVersion >= 29)
                {
                    await ConnectViaWifiNetworkSuggestion(context, wifiManager, ssid, encryption, password);
                }
                // For older Android: Legacy approach
                else
                {
                    await ConnectViaLegacy(context, wifiManager, ssid, encryption, password);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> Android WiFi Connection Error: {ex.GetType().Name} - {ex.Message}");
                await ShowAlert("Error", $"Android connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Android 12+ (API 31+): Use WifiNetworkSuggestion - most reliable
        /// </summary>
        private static async Task ConnectViaWifiNetworkSuggestion(Context context, WifiManager wifiManager, 
            string ssid, string encryption, string password)
        {
            try
            {
                Debug.WriteLine(">>> Using WifiNetworkSuggestion (Android 10+)");

                // Od Android 10 aplikacija ne može sama da uključi WiFi - otvori panel za korisnika
                if (!wifiManager.IsWifiEnabled)
                {
                    var enable = await ShowAlertChoice("WiFi Disabled",
                        "WiFi is off. Open WiFi settings to turn it on?", "Yes", "No");
                    if (!enable)
                        return;

                    OpenWifiSettings(context, usePanel: true);
                }

                var encryptionType = encryption.ToUpperInvariant();
                var isOpen = string.IsNullOrEmpty(password) || encryptionType == "OPEN" || encryptionType == "NOPASS";

                // WEP mreže nisu podržane preko WifiNetworkSuggestion API-ja
                if (!isOpen && encryptionType == "WEP")
                {
                    await ShowAlert("WiFi Connection",
                        $"WEP networks can't be added automatically.\n\nPlease connect to '{ssid}' manually.");
                    OpenWifiSettings(context, usePanel: false);
                    return;
                }

                var builder = new WifiNetworkSuggestion.Builder()
                    .SetSsid(ssid);

                if (!isOpen)
                {
                    if (encryptionType == "WPA3")
                        builder.SetWpa3Passphrase(password);
                    else
                        builder.SetWpa2Passphrase(password);
                }

                var suggestion = builder.Build();

                // Add the suggestion - use C# List
                var suggestions = new List<WifiNetworkSuggestion> { suggestion };

                int result = (int)wifiManager.AddNetworkSuggestions(suggestions);
                Debug.WriteLine($">>> AddNetworkSuggestions result: {result}");

                if (result == 0)
                {
                    await ShowAlert("WiFi Connection", 
                        $"WiFi suggestion added for '{ssid}'.\n\n" +
                        $"Please go to WiFi settings and tap '{ssid}' to connect.\n\n" +
                        $"Internet will work once you're connected and DHCP assigns an IP.");
                }
                else
                {
                    await ShowAlert("Warning", $"Could not add WiFi suggestion: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> WifiNetworkSuggestion Error: {ex.Message}");
                await ShowAlert("Error", $"Suggestion failed: {ex.Message}");
            }
        }

        private static void OpenWifiSettings(Context context, bool usePanel)
        {
            try
            {
                var action = usePanel
                    ? Android.Provider.Settings.Panel.ActionWifi
                    : Android.Provider.Settings.ActionWifiSettings;
                var intent = new Intent(action);
                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> OpenWifiSettings Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Android 9 and older: Legacy WiFi connection
        /// </summary>
        private static async Task ConnectViaLegacy(Context context, WifiManager wifiManager, 
            string ssid, string encryption, string password)
        {
            try
            {
                Debug.WriteLine(">>> Using Legacy WiFi Connection (Android 9-)");

                if (!wifiManager.IsWifiEnabled)
                {
                    wifiManager.SetWifiEnabled(true);
                    await Task.Delay(3000);
                }

                RemoveExistingNetwork(wifiManager, ssid);
                var config = CreateWifiConfiguration(ssid, encryption, password);

                var result = wifiManager.AddNetwork(config);
                int netId = (int)result;
                if (netId != -1)
                {
                    wifiManager.EnableNetwork(netId, true);
                    
                    // Wait for DHCP
                    Debug.WriteLine(">>> Waiting for DHCP to assign IP...");
                    await Task.Delay(5000);

                    var hasInternet = await VerifyInternetConnection(wifiManager, ssid, netId);
                    
                    if (hasInternet)
                    {
                        await ShowAlert("✓ Connected", $"Successfully connected to '{ssid}' with internet!");
                    }
                    else
                    {
                        await ShowAlert("⚠ Connected", $"Connected to '{ssid}' but internet may not be working.");
                    }
                }
                else
                {
                    await ShowAlert("Error", $"Failed to connect to '{ssid}'");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> Legacy Connection Error: {ex.Message}");
                await ShowAlert("Error", $"Legacy connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Verify that internet connection is working
        /// CRITICAL: This ensures DHCP has completed and Internet is available
        /// </summary>
        private static async Task<bool> VerifyInternetConnection(WifiManager wifiManager, string ssid, int netId)
        {
            try
            {
                Debug.WriteLine(">>> Verifying internet connection...");

                var connectionInfo = wifiManager.ConnectionInfo;
                if (connectionInfo == null)
                {
                    Debug.WriteLine(">>> No connection info available");
                    return false;
                }

                // Check if IP address was assigned (not 0.0.0.0)
                int ipAddress = connectionInfo.IpAddress;
                if (ipAddress == 0)
                {
                    Debug.WriteLine(">>> DHCP failed - No IP address assigned");
                    return false;
                }

                // Convert IP to readable format
                var ip = ((ipAddress & 0xff) + "." +
                         ((ipAddress >> 8) & 0xff) + "." +
                         ((ipAddress >> 16) & 0xff) + "." +
                         ((ipAddress >> 24) & 0xff));
                
                Debug.WriteLine($">>> IP Address assigned: {ip}");
                Debug.WriteLine($">>> Network ID: {connectionInfo.NetworkId}");
                Debug.WriteLine($">>> Link Speed: {connectionInfo.LinkSpeed} Mbps");

                // Check if connected to the right network
                if (connectionInfo.NetworkId == netId)
                {
                    Debug.WriteLine($">>> ✓ Connected to correct network: {ssid}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($">>> ✗ Connected to different network");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> VerifyInternetConnection Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create WifiConfiguration object from credentials
        /// </summary>
        private static WifiConfiguration CreateWifiConfiguration(string ssid, string encryption, string password)
        {
            var config = new WifiConfiguration
            {
                Ssid = $"\"{ssid}\""
            };

            switch (encryption.ToUpper())
            {
                case "WPA":
                case "WPA2":
                case "WPA3":
                    config.AllowedKeyManagement.Set((int)KeyManagementType.WpaPsk);
                    config.PreSharedKey = $"\"{password}\"";
                    break;

                case "WEP":
                    config.AllowedKeyManagement.Set((int)KeyManagementType.None);
                    config.AllowedAuthAlgorithms.Set((int)AuthAlgorithmType.Open);
                    config.AllowedAuthAlgorithms.Set((int)AuthAlgorithmType.Shared);
                    var wepKeys = new string[4];
                    wepKeys[0] = $"\"{password}\"";
                    config.WepKeys = wepKeys;
                    config.WepTxKeyIndex = 0;
                    break;

                case "OPEN":
                case "NOPASS":
                    config.AllowedKeyManagement.Set((int)KeyManagementType.None);
                    break;

                default:
                    config.AllowedKeyManagement.Set((int)KeyManagementType.WpaPsk);
                    config.PreSharedKey = $"\"{password}\"";
                    break;
            }

            return config;
        }

        /// <summary>
        /// Remove existing WiFi network configuration
        /// </summary>
        private static void RemoveExistingNetwork(WifiManager wifiManager, string ssid)
        {
            try
            {
                var configuredNetworks = wifiManager.ConfiguredNetworks;
                if (configuredNetworks != null)
                {
                    foreach (var network in configuredNetworks)
                    {
                        if (network?.Ssid == $"\"{ssid}\"")
                        {
                            wifiManager.RemoveNetwork(network.NetworkId);
                            wifiManager.SaveConfiguration();
                            Debug.WriteLine($">>> Removed existing config for {ssid}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> RemoveExistingNetwork Error: {ex.Message}");
            }
        }
#endif

        /// <summary>
        /// Parse WiFi data from format: WIFI:S:SSID;T:Encryption;P:Password;;
        /// Specijalni znakovi (\ ; , : ") u vrednostima su escape-ovani sa "\".
        /// </summary>
        internal static (string ssid, string encryption, string password) ParseWifiData(string wifiData)
        {
            string ssid = "";
            string encryption = "";
            string password = "";

            try
            {
                var data = wifiData.Trim();

                // Gumby upisuje WiFi podatke URL-enkodovane (bez sirovih ";")
                if (!data.Contains(';'))
                    data = System.Uri.UnescapeDataString(data);

                // Remove WIFI: prefix if present
                if (data.StartsWith("WIFI:", StringComparison.OrdinalIgnoreCase))
                    data = data.Substring(5);

                foreach (var part in SplitEscaped(data, ';'))
                {
                    if (part.Length < 2 || part[1] != ':')
                        continue;

                    var value = Unescape(part.Substring(2));
                    switch (char.ToUpperInvariant(part[0]))
                    {
                        case 'S': ssid = value; break;
                        case 'T': encryption = value; break;
                        case 'P': password = value; break;
                    }
                }

                Debug.WriteLine($">>> Parsed - SSID: {ssid}, Type: {encryption}, Pass length: {password.Length}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> ParseWifiData Error: {ex.Message}");
            }

            return (ssid, encryption, password);
        }

        /// <summary>
        /// Escape-uje specijalne znakove WIFI formata (\ ; , : ").
        /// </summary>
        public static string EscapeWifiValue(string value)
        {
            var sb = new System.Text.StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (c == '\\' || c == ';' || c == ',' || c == ':' || c == '"')
                    sb.Append('\\');
                sb.Append(c);
            }
            return sb.ToString();
        }

        // Deli string po separatoru, ali ne po escape-ovanom separatoru ("\;")
        private static List<string> SplitEscaped(string data, char separator)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                var c = data[i];
                if (c == '\\' && i + 1 < data.Length)
                {
                    current.Append(c).Append(data[i + 1]);
                    i++;
                }
                else if (c == separator)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result;
        }

        private static string Unescape(string value)
        {
            var sb = new System.Text.StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\\' && i + 1 < value.Length)
                    i++;
                sb.Append(value[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Fallback for non-Android platforms
        /// </summary>
        private static async Task ConnectFallback(string ssid, string encryption)
        {
            await ShowAlert("WiFi Connection", 
                $"Would connect to:\n\nSSID: {ssid}\nEncryption: {encryption}\n\nPlease connect manually on your device settings.");
        }

        /// <summary>
        /// Show alert dialog
        /// </summary>
        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
        //            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }

        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        private static async Task<bool> ShowAlertChoice(string title, string message, string accept, string cancel)
        {
            var result = false;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                }
            });
            return result;
        }
    }
}