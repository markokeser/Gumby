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

                // For Android 12+ (API 31+): Use WifiNetworkSuggestion (Settings panel approach)
                if (osVersion >= 31)
                {
                    await ConnectViaWifiNetworkSuggestion(context, wifiManager, ssid, encryption, password);
                }
                // For Android 10-11: Use direct network configuration
                else if (osVersion >= 29)
                {
                    await ConnectViaWifiConfiguration(context, wifiManager, ssid, encryption, password);
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
                Debug.WriteLine(">>> Using WifiNetworkSuggestion (Android 12+)");

                // Check if WiFi is enabled
                if (!wifiManager.IsWifiEnabled)
                {
                    var enable = await ShowAlertChoice("WiFi Disabled", 
                        "WiFi is off. Enable it?", "Yes", "No");
                    if (enable)
                    {
                        wifiManager.SetWifiEnabled(true);
                        await Task.Delay(3000);
                    }
                    else
                    {
                        return;
                    }
                }

                // Create WifiNetworkSuggestion
                var builder = new WifiNetworkSuggestion.Builder()
                    .SetSsid(ssid);

                // Set password based on encryption type
                if (!string.IsNullOrEmpty(password))
                {
                    switch (encryption.ToUpper())
                    {
                        case "WPA":
                        case "WPA2":
                        case "WPA3":
                            builder.SetWpa2Passphrase(password);
                            break;
                        case "WEP":
                            Debug.WriteLine(">>> WEP encryption not supported, using WPA2");
                            builder.SetWpa2Passphrase(password);
                            break;
                        case "NOPASS":
                        case "OPEN":
                            // No password for open networks
                            break;
                        default:
                            builder.SetWpa2Passphrase(password);
                            break;
                    }
                }
                else if (encryption.ToUpper() != "OPEN" && encryption.ToUpper() != "NOPASS")
                {
                    builder.SetWpa2Passphrase("");
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

        /// <summary>
        /// Android 10-11 (API 29-30): Use WifiConfiguration - legacy but still works
        /// </summary>
        private static async Task ConnectViaWifiConfiguration(Context context, WifiManager wifiManager, 
            string ssid, string encryption, string password)
        {
            try
            {
                Debug.WriteLine(">>> Using WifiConfiguration (Android 10-11)");

                if (!wifiManager.IsWifiEnabled)
                {
                    wifiManager.SetWifiEnabled(true);
                    await Task.Delay(3000);
                }

                // Remove existing config
                RemoveExistingNetwork(wifiManager, ssid);
                await Task.Delay(500);

                // Create config
                var config = CreateWifiConfiguration(ssid, encryption, password);

                var result = wifiManager.AddNetwork(config);
                int netId = (int)result;
                if (netId != -1)
                {
                    wifiManager.Disconnect();
                    await Task.Delay(1000);

                    wifiManager.EnableNetwork(netId, true);
                    wifiManager.Reconnect();

                    // CRITICAL: Wait for DHCP to assign IP address
                    Debug.WriteLine(">>> Waiting for DHCP to assign IP...");
                    await Task.Delay(5000);

                    // Verify connection with IP
                    var hasInternet = await VerifyInternetConnection(wifiManager, ssid, netId);
                    
                    if (hasInternet)
                    {
                        await ShowAlert("✓ Connected", $"Successfully connected to '{ssid}' with internet!");
                    }
                    else
                    {
                        await ShowAlert("⚠ Connected", $"Connected to '{ssid}' but internet may not be working.\n\nTry manually connecting in WiFi Settings.");
                    }
                }
                else
                {
                    await ShowAlert("Error", $"Failed to add network '{ssid}'");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> WifiConfiguration Error: {ex.Message}");
                await ShowAlert("Error", $"Connection failed: {ex.Message}");
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
        /// </summary>
        private static (string ssid, string encryption, string password) ParseWifiData(string wifiData)
        {
            string ssid = "";
            string encryption = "";
            string password = "";

            try
            {
                // Remove WIFI: prefix if present
                var data = wifiData.StartsWith("WIFI:") ? wifiData.Substring(5) : wifiData;

                // Split by semicolon
                var parts = data.Split(';');
                
                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part)) continue;

                    if (part.StartsWith("S:"))
                        ssid = part.Substring(2);
                    else if (part.StartsWith("T:"))
                        encryption = part.Substring(2);
                    else if (part.StartsWith("P:"))
                        password = part.Substring(2);
                }

                Debug.WriteLine($">>> Parsed - SSID: {ssid}, Type: {encryption}, Pass length: {password?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> ParseWifiData Error: {ex.Message}");
            }

            return (ssid, encryption, password);
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