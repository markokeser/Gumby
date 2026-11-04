using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

#if ANDROID
using Android.Content;
using Android.Net;
using AndroidX.Core.Content;
#endif

namespace Gumby.Tasks
{
    public class AddContactTaskService
    {
        public static async Task AddContact(string vcardData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vcardData))
                {
         //           await ShowAlert("Error", "No contact data provided.");
                    return;
                }

                Debug.WriteLine($">>> ContactTaskService: Adding contact");

                // Decode the vCard data
                var vcard = System.Uri.UnescapeDataString(vcardData);

#if ANDROID
                // Save vCard to temporary file
                var fileName = $"contact_{DateTime.Now.Ticks}.vcf";
                var tempFile = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(tempFile, vcard);

                // Open the vCard file using Android intent
                var context = Android.App.Application.Context;
                var javaFile = new Java.IO.File(tempFile);
                var authority = $"{context.PackageName}.fileprovider";
                var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, authority, javaFile);

                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, "text/x-vcard");
                intent.SetFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);

                context.StartActivity(intent);

   //             await ShowAlert("Contact", "Opening contact. Tap 'Save' to add to your contacts.");
#else
                // For other platforms, show instructions
  //              await ShowAlert("Contact", $"Would save contact:\n\n{vcard}\n\nPlease add manually.");
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddContact error: {ex.Message}");
                await ShowAlert("Error", $"Failed to add contact: {ex.Message}");
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
    }
}