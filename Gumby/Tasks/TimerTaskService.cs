using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Content;
#endif

namespace Gumby.Tasks
{
    public class TimerTaskService
    {
        public static async Task StartTimer(int seconds)
        {
            try
            {
                if (seconds <= 0)
                {
                    await ShowAlert("Error", "Invalid timer duration.");
                    return;
                }

                Debug.WriteLine($">>> TimerTaskService: Starting timer for {seconds} seconds");

#if ANDROID
                try
                {
                    var context = Android.App.Application.Context;

                    // Try to open system clock app with timer
                    var intent = new Intent(Android.Provider.AlarmClock.ActionSetTimer);
                    intent.PutExtra(Android.Provider.AlarmClock.ExtraLength, seconds);
                    intent.PutExtra(Android.Provider.AlarmClock.ExtraSkipUi, true);
                    intent.SetFlags(ActivityFlags.NewTask);

                    // Check if there's an app that can handle this intent
                    if (intent.ResolveActivity(context.PackageManager) != null)
                    {
                        context.StartActivity(intent);

                        // Format time for display
                        var timeSpan = TimeSpan.FromSeconds(seconds);
                        var timeText = FormatTime(timeSpan);

                        await ShowAlert("Timer Started", $"Timer set for {timeText}");
                    }
                    else
                    {
                        // Fallback: open clock app manually
                        var clockIntent = context.PackageManager.GetLaunchIntentForPackage("com.google.android.deskclock");
                        clockIntent ??= context.PackageManager.GetLaunchIntentForPackage("com.android.deskclock");
                        clockIntent ??= context.PackageManager.GetLaunchIntentForPackage("com.sec.android.app.clockpackage");

                        if (clockIntent != null)
                        {
                            clockIntent.SetFlags(ActivityFlags.NewTask);
                            context.StartActivity(clockIntent);
                            await ShowAlert("Timer", $"Please set a timer for {FormatTime(TimeSpan.FromSeconds(seconds))}");
                        }
                        else
                        {
                            await ShowAlert("Timer", $"Timer would be set for {FormatTime(TimeSpan.FromSeconds(seconds))} (Android only)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Android timer error: {ex.Message}");
                    await ShowAlert("Error", $"Failed to start timer: {ex.Message}");
                }
#else
                var timeSpan = TimeSpan.FromSeconds(seconds);
                await ShowAlert("Start Timer", $"Would start timer for {FormatTime(timeSpan)} (Android only)");
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StartTimer error: {ex.Message}");
                await ShowAlert("Error", $"Failed to start timer: {ex.Message}");
            }
        }

        private static string FormatTime(TimeSpan time)
        {
            if (time.Hours > 0)
                return $"{time.Hours}h {time.Minutes}m {time.Seconds}s";
            if (time.Minutes > 0)
                return $"{time.Minutes}m {time.Seconds}s";
            return $"{time.Seconds}s";
        }

        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
       //             await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
    }
}