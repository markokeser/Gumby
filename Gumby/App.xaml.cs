using Gumby.Tasks;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Gumby
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Request Do Not Disturb permission when app starts
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500); // Small delay to ensure UI is ready
                await RequestDoNotDisturbPermission();
            });

            return window;
        }

        private async Task RequestDoNotDisturbPermission()
        {
#if ANDROID
            try
            {
                var notificationManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.NotificationService) as Android.App.NotificationManager;

                // Check if we already have the permission
                if (notificationManager != null && notificationManager.IsNotificationPolicyAccessGranted)
                {
                    Debug.WriteLine("Do Not Disturb permission already granted");
                    return;
                }

                // Ask user to grant permission
                var result = await MainPage.DisplayAlert(
                    "Permission Required",
                    "Gumby needs 'Do Not Disturb' access to change your ringtone mode (mute/vibrate/sound).\n\n" +
                    "This allows the app to control your phone's ringer when you tap NFC chips.\n\n" +
                    "Do you want to grant this permission?",
                    "Yes",
                    "No");

                if (result)
                {
                    // Open settings for user to grant permission
                    var intent = new Android.Content.Intent(Android.Provider.Settings.ActionNotificationPolicyAccessSettings);
                    intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                    Android.App.Application.Context.StartActivity(intent);

                    await MainPage.DisplayAlert("Permission Required",
                        "Please find 'Gumby' in the list and toggle the permission ON.\n\n" +
                        "Then come back to the app and restart it.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Permission request error: {ex.Message}");
            }
#endif
        }
    }
}