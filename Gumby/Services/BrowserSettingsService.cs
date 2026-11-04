using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

#if ANDROID
using Android.Content;
using Android.Content.PM;
#endif

namespace Gumby.Services
{
    public class BrowserSettingsService
    {
        // Preference ključevi
        public static string SelectedBrowserPackage
        {
            get => Preferences.Get("selected_browser_package", "");
            set => Preferences.Set("selected_browser_package", value);
        }

        public static string SelectedBrowserName
        {
            get => Preferences.Get("selected_browser_name", "Default browser");
            set => Preferences.Set("selected_browser_name", value);
        }

        public static bool UseCustomBrowser
        {
            get => Preferences.Get("use_custom_browser", false);
            set => Preferences.Set("use_custom_browser", value);
        }

        /// <summary>
        /// Lista poznatih browsera - ovo je hardkodirano i ne zahteva QUERY_ALL_PACKAGES
        /// </summary>
        public static ObservableCollection<BrowserInfo> GetKnownBrowsers()
        {
            return new ObservableCollection<BrowserInfo>
            {
                new BrowserInfo { PackageName = "com.android.chrome", BrowserName = "Google Chrome" },
                new BrowserInfo { PackageName = "com.brave.browser", BrowserName = "Brave" },
                new BrowserInfo { PackageName = "org.mozilla.firefox", BrowserName = "Firefox" },
                new BrowserInfo { PackageName = "com.microsoft.emmx", BrowserName = "Microsoft Edge" },
                new BrowserInfo { PackageName = "com.opera.browser", BrowserName = "Opera" },
                new BrowserInfo { PackageName = "com.sec.android.app.sbrowser", BrowserName = "Samsung Internet" },
                new BrowserInfo { PackageName = "com.vivaldi.browser", BrowserName = "Vivaldi" },
                new BrowserInfo { PackageName = "com.kiwibrowser.browser", BrowserName = "Kiwi Browser" },
                new BrowserInfo { PackageName = "com.chrome.beta", BrowserName = "Chrome Beta" },
                new BrowserInfo { PackageName = "com.chrome.dev", BrowserName = "Chrome Dev" },
                new BrowserInfo { PackageName = "org.mozilla.fenix", BrowserName = "Firefox Nightly" }
            };
        }

        /// <summary>
        /// Proverava da li je browser instaliran (bez QUERY_ALL_PACKAGES)
        /// </summary>
        public static async Task<bool> IsBrowserInstalled(string packageName)
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var packageManager = context.PackageManager;
                packageManager.GetPackageInfo(packageName, 0);
                return true;
            }
            catch (PackageManager.NameNotFoundException)
            {
                return false;
            }
#endif
            return false;
        }

        /// <summary>
        /// Dobija listu instaliranih browsera iz poznate liste
        /// </summary>
        public static async Task<ObservableCollection<BrowserInfo>> GetInstalledBrowsers()
        {
            var installedBrowsers = new ObservableCollection<BrowserInfo>();
            var allKnownBrowsers = GetKnownBrowsers();

            foreach (var browser in allKnownBrowsers)
            {
                var isInstalled = await IsBrowserInstalled(browser.PackageName);
                if (isInstalled)
                {
                    browser.IsInstalled = true;
                    browser.IsSelected = UseCustomBrowser && SelectedBrowserPackage == browser.PackageName;
                    installedBrowsers.Add(browser);
                }
            }

            return installedBrowsers;
        }

        /// <summary>
        /// Dobija default browser (pokušava da ga detektuje, ali ne garantuje)
        /// </summary>
        public static async Task<BrowserInfo> GetDefaultBrowser()
        {
#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var packageManager = context.PackageManager;

                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://www.example.com"));
                var resolveInfo = packageManager.ResolveActivity(intent, PackageInfoFlags.MatchDefaultOnly);

                if (resolveInfo != null && resolveInfo.ActivityInfo != null)
                {
                    var packageName = resolveInfo.ActivityInfo.PackageName;
                    var appName = resolveInfo.LoadLabel(packageManager)?.ToString() ?? packageName;

                    return new BrowserInfo
                    {
                        PackageName = packageName,
                        BrowserName = appName,
                        IsDefault = true
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetDefaultBrowser error: {ex.Message}");
            }
#endif
            return new BrowserInfo { BrowserName = "Default browser", PackageName = "", IsDefault = true };
        }

        /// <summary>
        /// Čuva izabrani browser
        /// </summary>
        public static void SaveSelectedBrowser(string packageName, string browserName)
        {
            SelectedBrowserPackage = packageName;
            SelectedBrowserName = browserName;
            UseCustomBrowser = true;
        }

        /// <summary>
        /// Resetuje na default browser
        /// </summary>
        public static void ResetToDefault()
        {
            UseCustomBrowser = false;
            SelectedBrowserPackage = "";
            SelectedBrowserName = "Default browser";
        }
    }

    public class BrowserInfo
    {
        public string PackageName { get; set; } = "";
        public string BrowserName { get; set; } = "";
        public bool IsDefault { get; set; }
        public bool IsSelected { get; set; }
        public bool IsInstalled { get; set; }
    }
}