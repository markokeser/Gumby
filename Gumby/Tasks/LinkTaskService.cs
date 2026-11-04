#if ANDROID
using Android.Content;
using Gumby.Services;

#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gumby.Tasks
{
    public class LinkTaskService
    {
        public static async Task OpenUrl(string url)
        {
            try
            {
#if ANDROID
                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));

                // Koristi browser iz Settings umesto Brave
                if (BrowserSettingsService.UseCustomBrowser && !string.IsNullOrEmpty(BrowserSettingsService.SelectedBrowserPackage))
                {
                    intent.SetPackage(BrowserSettingsService.SelectedBrowserPackage);
                }

                intent.SetFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
#endif
            }
            catch (Exception ex)
            {
                //          System.Diagnostics.Debug.WriteLine($">>> LinkTaskService Error: {ex.Message}");
            }
        }

        public static async Task OpenUrlWithBrowser(string url, string? browserPackage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                    return;

#if ANDROID
                var context = Android.App.Application.Context;
                var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));

                if (!string.IsNullOrEmpty(browserPackage))
                {
                    intent.SetPackage(browserPackage);
                }

                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
#else
                await Launcher.OpenAsync(url);
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OpenUrl error: {ex.Message}");
                // Fallback na default pregledač
                await Launcher.OpenAsync(url);
            }
        }
    }
}
