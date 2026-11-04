using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Gumby.Tasks
{
    public class MessageTaskService
    {
        public static async Task SendMessage(string phoneNumber, string message = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    await ShowAlert("Error", "No phone number provided.");
                    return;
                }

                // ✅ IMPORTANT: Request SEND_SMS permission for direct sending
                var status = await Permissions.RequestAsync<Permissions.Sms>();

                if (status != PermissionStatus.Granted)
                {
                    await ShowAlert("Permission Denied",
                        "Cannot send message without SMS permission.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Sent from Gumby NFC app";
                }

#if ANDROID
                // ✅ FIX: Use SmsManager to send message in the background
                var context = Android.App.Application.Context;
                var smsManager = Android.Telephony.SmsManager.Default;

                // Format the phone number 
                var formattedNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

                // Send the message directly (keeps user in your app)
                smsManager.SendTextMessage(formattedNumber, null, message, null, null);

                await ShowAlert("Message Sent", $"Message sent to {formattedNumber}");
#else
                // Fallback for other platforms: open the SMS app
                await Microsoft.Maui.ApplicationModel.Communication.Sms.ComposeAsync(
                    new Microsoft.Maui.ApplicationModel.Communication.SmsMessage(message, phoneNumber));
#endif
            }
            catch (FeatureNotSupportedException)
            {
                await ShowAlert("Error", "SMS is not supported on this device.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Message error: {ex.Message}");
                await ShowAlert("Error", $"Failed to send message: {ex.Message}");
            }
        }

        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
          //          await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
    }
}