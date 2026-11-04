#if ANDROID
using Android.AdServices.Common;
#endif
using Gumby.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gumby.Services
{
    public class ChipService
    {
        public static void HandleNfcTagDiscovered(string tagData)
        {
                var tasks = tagData.Split('\x1F');
            foreach (var task in tasks)
            {
                DoTask(task);
            }
        }

        private static async Task DoTask(string tagData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tagData))
                {
                    //    await ShowAlert("Empty Tag", "NFC tag contains no data.");
                    return;
                }

                var parts = tagData.Split('|');
                var command = parts.Length > 0 ? parts[0] : "";
                var value = parts.Length > 1 ? string.Join("|", parts.Skip(1)) : "";

                switch (command)
                {
                    //URL
                    case "0":
                        LinkTaskService.OpenUrl(value);
                        break;

                    // VOLUME & SOUND (RINGTONE, MEDIA, MUTE, UNMUTE, VIBRATE)
                    case "1": // Volume komanda (format: 1|0|50, 1|1|75, 1|2, 1|3, 1|4)
                        var volumeParts = value.Split('|');
                        var volumeType = volumeParts[0]; // "0"=ringtone, "1"=media, "2"=mute, "3"=unmute, "4"=vibrate

                        if (volumeType == "0" || volumeType == "1")
                        {
                            // Volume komande (imaju nivo jačine)
                            var volumeLevel = volumeParts.Length > 1 ? volumeParts[1] : "50";
                            if (int.TryParse(volumeLevel, out int level))
                            {
                                if (volumeType == "0")
                                    await RingtoneTaskService.SetRingtoneVolume(level);
                                else if (volumeType == "1")
                                    await RingtoneTaskService.SetMediaVolume(level);
                            }
                        }
                        else if (volumeType == "2")
                        {
                            // Mute
                            await RingtoneTaskService.RingtoneMute();
                        }
                        else if (volumeType == "3")
                        {
                            // Unmute
                            await RingtoneTaskService.RingtoneOn();
                        }
                        else if (volumeType == "4")
                        {
                            // Vibrate
                            await RingtoneTaskService.RingtoneVibrate();
                        }
                        break;

                    //CALL
                    case "2":
                        CallTaskService.CallNumber(value);
                        break;

                    case "SMS":
                        var phoneNumber = value;
                        MessageTaskService.SendMessage(phoneNumber);
                        break;

                    //SMS WITH MESSAGE
                    case "3":
                        parts = tagData.Split('|');
                        var phone = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
                        var text = parts.Length > 2 ? Uri.UnescapeDataString(parts[2]) : "";
                        MessageTaskService.SendMessage(phone, text);
                        break;

                    //OPEN APP
                    case "4":
                        OpenAppTaskService.OpenApp(value);
                        break;

                    //SET ALARM
                    case "5":
                        AlarmTaskService.SetAlarm(value);
                        break;

                    //START TIMER
                    case "6": 
                        if (int.TryParse(value, out int seconds))
                            TimerTaskService.StartTimer(seconds);
                        break;

                    //CONNECT TO WIFI
                    case "7":
                        WifiConnectionTaskService.ConnectToWifi(value);
                        break;

                    //ADD CONTACT
                    case "8":
                        AddContactTaskService.AddContact(value);
                        break;

                    case "10":
                        BluetoothTaskService.HandleCommand(value); 
                        break;

                    case "11": // Spotify Playlist
                               // Spotify package name: com.spotify.music
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.spotify.music");
                        break;

                    case "12": // YouTube Playlist
                               // YouTube package name: com.google.android.youtube
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.google.android.youtube");
                        break;

                    case "13": // Instagram Reel
                               // Instagram package name: com.instagram.android
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.instagram.android");
                        break;

                    case "14": // Google Maps
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.google.android.apps.maps");
                        break;

                    default:
                        //   await ShowAlert("NFC", $"Command: {command}\nValue: {value}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> GUMBY Error: {ex.Message}");
            }
        }

    }
}
