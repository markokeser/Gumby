#if ANDROID
using Android.Media;
using Android.App;
using Android.Content;
using Android.OS;
#endif

namespace Gumby.Tasks
{
    public class RingtoneTaskService
    {
        // ========== POSTOJEĆE METODE (NE MENJAJ) ==========

        public static async Task RingtoneOn()
        {
#if ANDROID
            try
            {
                var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    audioManager.RingerMode = RingerMode.Normal;
                    int maxVolume = audioManager.GetStreamMaxVolume(Android.Media.Stream.Ring);
                    int newVolume = (int)(maxVolume * 0.5);
                    audioManager.SetStreamVolume(Android.Media.Stream.Ring, newVolume, VolumeNotificationFlags.PlaySound);
                    await ShowAlert("Ringtone", $"Ringtone ON at {newVolume * 100 / maxVolume}%");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RingtoneOn error: {ex.Message}");
            }
#endif
        }

        public static async Task RingtoneMute()
        {
#if ANDROID
            try
            {
                var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    audioManager.RingerMode = RingerMode.Silent;
                    await ShowAlert("Ringtone", "Ringtone MUTED");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RingtoneMute error: {ex.Message}");
            }
#endif
        }

        public static async Task RingtoneVibrate()
        {
#if ANDROID
            try
            {
                var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    audioManager.RingerMode = RingerMode.Vibrate;
                    await ShowAlert("Ringtone", "Vibrate mode ON");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RingtoneVibrate error: {ex.Message}");
            }
#endif
        }

        // ========== NOVE METODE (DODATE) ==========

        /// <summary>
        /// Postavlja tačnu jačinu zvona (0-100%)
        /// </summary>
        public static async Task SetRingtoneVolume(int level)
        {
#if ANDROID
            try
            {
                var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    level = Math.Clamp(level, 0, 100);
                    int maxVolume = audioManager.GetStreamMaxVolume(Android.Media.Stream.Ring);
                    int newVolume = (int)(maxVolume * (level / 100.0));
                    audioManager.SetStreamVolume(Android.Media.Stream.Ring, newVolume, 0);
                    await ShowAlert("Ringtone", $"Ringtone set to {level}%");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetRingtoneVolume error: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Postavlja tačnu jačinu medija (muzika, video) (0-100%)
        /// </summary>
        public static async Task SetMediaVolume(int level)
        {
#if ANDROID
            try
            {
                var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
                if (audioManager != null)
                {
                    level = Math.Clamp(level, 0, 100);
                    int maxVolume = audioManager.GetStreamMaxVolume(Android.Media.Stream.Music);
                    int newVolume = (int)(maxVolume * (level / 100.0));
                    audioManager.SetStreamVolume(Android.Media.Stream.Music, newVolume, 0);
                    await ShowAlert("Media", $"Media volume set to {level}%");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetMediaVolume error: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Isključuje zvuk (mute) - ista kao RingtoneMute ali jasniji naziv
        /// </summary>
        public static async Task MuteSound()
        {
            await RingtoneMute();
        }

        /// <summary>
        /// Uključuje zvuk - ista kao RingtoneOn ali jasniji naziv
        /// </summary>
        public static async Task UnmuteSound()
        {
            await RingtoneOn();
        }

        // ========== ZAJEDNIČKA METODA ==========

        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Microsoft.Maui.Controls.Application.Current?.MainPage != null)
                {
        //            await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
    }
}