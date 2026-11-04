using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Gumby.Tasks
{
    public class OpenAppTaskService
    {
        public static async Task OpenApp(string packageName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(packageName))
                {
                    await ShowAlert("Error", "No app specified.");
                    return;
                }

                Debug.WriteLine($">>> OpenAppTaskService: Opening {packageName}");

#if ANDROID
                var context = Android.App.Application.Context;
                var packageManager = context.PackageManager;

                // Get launch intent for the package
                var intent = packageManager.GetLaunchIntentForPackage(packageName);

                if (intent != null)
                {
                    intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                    context.StartActivity(intent);
                    await ShowAlert("Opening App", $"Opening {packageName}...");
                }
                else
                {
                    // App not found, offer to open Play Store
                    var result = await ShowAlertChoice(
                        "App Not Found",
                        $"'{packageName}' is not installed.\n\nWould you like to search for it in Play Store?",
                        "Yes", "No");

                    if (result)
                    {
                        await Launcher.OpenAsync($"market://details?id={packageName}");
                    }
                }
#else
                await ShowAlert("Info", "Opening apps is only supported on Android.");
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OpenApp error: {ex.Message}");
                await ShowAlert("Error", $"Failed to open app: {ex.Message}");
            }
        }

        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }

        private static async Task<bool> ShowAlertChoice(string title, string message, string accept, string cancel)
        {
            var result = false;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
       //             result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                }
            });
            return result;
        }
    }
}
