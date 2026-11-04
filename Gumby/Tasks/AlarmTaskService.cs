using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Content;
using Android.Provider;
#endif

namespace Gumby.Tasks
{
    public class AlarmTaskService
    {
        public static async Task SetAlarm(string timeString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(timeString))
                {
                    await ShowAlert("Error", "No alarm time specified.");
                    return;
                }

                Debug.WriteLine($">>> AlarmTaskService: Setting alarm for {timeString}");

                var parts = timeString.Split(':');
                if (parts.Length != 2)
                {
                    await ShowAlert("Error", $"Invalid time format: {timeString}");
                    return;
                }

                if (!int.TryParse(parts[0], out int hour) || !int.TryParse(parts[1], out int minute))
                {
                    await ShowAlert("Error", $"Invalid time values: {timeString}");
                    return;
                }

#if ANDROID
                try
                {
                    var intent = new Intent(AlarmClock.ActionSetAlarm);
                    intent.PutExtra(AlarmClock.ExtraHour, hour);
                    intent.PutExtra(AlarmClock.ExtraMinutes, minute);
                    intent.PutExtra(AlarmClock.ExtraSkipUi, true);  // ← CREATES ALARM WITHOUT OPENING UI
                    intent.SetFlags(ActivityFlags.NewTask);

                    var context = Android.App.Application.Context;

                    // Check if there's an app that can handle this intent
                    if (intent.ResolveActivity(context.PackageManager) != null)
                    {
                        context.StartActivity(intent);
                        await ShowAlert("Alarm Set", $"Alarm set for {hour:D2}:{minute:D2}");
                    }
                    else
                    {
                        await ShowAlert("Error", "No alarm app found on this device.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Android alarm error: {ex.Message}");
                    await ShowAlert("Error", $"Failed to set alarm: {ex.Message}");
                }
#else
                await ShowAlert("Set Alarm", $"Would set alarm for {hour:D2}:{minute:D2} (Android only)");
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SetAlarm error: {ex.Message}");
                await ShowAlert("Error", $"Failed to set alarm: {ex.Message}");
            }
        }

        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
      //              await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
    }
}