using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
#if ANDROID
using Android.Content;
using Android.Net;
using AndroidX.Core.Content;
#endif

namespace Gumby.Tasks
{
    public class DeviceTaskService
    {
        public static async Task OpenFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    await ShowAlert("Error", "No file path specified.");
                    return;
                }

                if (!File.Exists(filePath))
                {
                    await ShowAlert("Error", $"File not found:\n{filePath}");
                    return;
                }

                Debug.WriteLine($">>> DeviceTaskService: Opening file {filePath}");

#if ANDROID
                // Get the Android application context
                var context = Android.App.Application.Context;

                // Create a standard Java File object
                var javaFile = new Java.IO.File(filePath);

                // Create a URI for the file using FileProvider
                var authority = $"{context.PackageName}.fileprovider";
                var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, authority, javaFile);

                // Create an Intent with ACTION_VIEW
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, "application/vnd.apple.mpegurl");
                intent.SetFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);

                try
                {
                    // Try to open with VLC or default media player
                    context.StartActivity(intent);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Intent start error: {ex.Message}");

                    // Fallback: Guide user to open VLC manually
                    await ShowAlert("Cannot Open Playlist",
                        "Could not open playlist directly.\n\n" +
                        "Please try one of these options:\n\n" +
                        "1. Open VLC manually, then:\n" +
                        "   - Tap the playlist icon\n" +
                        "   - Browse to: " + Path.GetFileName(filePath) + "\n\n" +
                        "2. Install a specialized M3U player from Play Store\n\n" +
                        "3. Open this file from your Files app instead");
                }
#else
        // Fallback for other platforms
        var file = new ShareFile(filePath, "audio/x-mpegurl");
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Open playlist",
            File = file
        });
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OpenFile error: {ex.Message}");
                await ShowAlert("Error", $"Failed to open file: {ex.Message}");
            }
        }

#if ANDROID
        private static bool IsVlcInstalled()
        {
            try
            {
                var context = Android.App.Application.Context;
                var packageManager = context.PackageManager;
                packageManager.GetPackageInfo("org.videolan.vlc", 0);
                return true;
            }
            catch
            {
                return false;
            }
        }
#endif

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