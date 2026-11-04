using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Gumby.Tasks
{
    public class CallTaskService
    {
        public static async Task CallNumber(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    await ShowAlert("Error", "No phone number provided.");
                    return;
                }

                // Request CALL_PHONE permission
                var status = await Permissions.RequestAsync<Permissions.Phone>();

                if (status != PermissionStatus.Granted)
                {
                    await ShowAlert("Permission Denied",
                        "Cannot make a call without phone permission. Please enable it in device settings.");
                    return;
                }

                // Use native Android intent to call directly
#if ANDROID
                var context = Android.App.Application.Context;

                // Get the default dialer package
                var intent = new Android.Content.Intent(Android.Content.Intent.ActionCall);
                intent.SetData(Android.Net.Uri.Parse($"tel:{phoneNumber}"));
                intent.SetFlags(Android.Content.ActivityFlags.NewTask);

           /*     var packageManager = context.PackageManager;
                var resolveInfo = packageManager.ResolveActivity(intent, 0);

                if (resolveInfo != null)
                {
                    // Explicitly set the package to the default dialer (com.android.phone or com.android.server.telecom)
                    var packageName = resolveInfo.ActivityInfo.PackageName;
                    intent.SetPackage(packageName);
                }   */

                context.StartActivity(intent);
#endif
            }
            catch (FeatureNotSupportedException)
            {
                await ShowAlert("Error", "Phone calls are not supported on this device.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Call error: {ex.Message}");
                await ShowAlert("Error", $"Failed to make call: {ex.Message}");
            }
        }

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
    }
}