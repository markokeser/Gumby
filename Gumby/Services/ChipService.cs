using Gumby.Tasks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gumby.Services
{
    public class ChipService
    {
        /// <summary>
        /// Izvršava sve komande sa čipa, jednu po jednu, redosledom kojim su upisane.
        /// </summary>
        public static async Task HandleNfcTagDiscovered(string tagData)
        {
            if (string.IsNullOrWhiteSpace(tagData))
                return;

            var tasks = tagData.Split(CommandCodes.TaskSeparator);
            foreach (var task in tasks)
            {
                await DoTask(task);
            }
        }

        private static async Task DoTask(string tagData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tagData))
                    return;

                var parts = tagData.Split('|');
                var command = parts[0];
                var value = parts.Length > 1 ? string.Join("|", parts.Skip(1)) : "";

                switch (command)
                {
                    case CommandCodes.Base64:
                        // Komanda je upisana kao base64 jer je sadržala separator komandi
                        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                        await DoTask(decoded);
                        break;

                    case CommandCodes.Url:
                        await LinkTaskService.OpenUrl(value);
                        break;

                    // VOLUME & SOUND (format: 1|0|50, 1|1|75, 1|2, 1|3, 1|4)
                    case CommandCodes.Volume:
                        await HandleVolume(value);
                        break;

                    case CommandCodes.Call:
                        await CallTaskService.CallNumber(value);
                        break;

                    // SMS (format: 3|telefon|poruka, oba URL-enkodovana)
                    case CommandCodes.Sms:
                        var phone = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
                        var text = parts.Length > 2 ? Uri.UnescapeDataString(parts[2]) : "";
                        await MessageTaskService.SendMessage(phone, text);
                        break;

                    case CommandCodes.OpenApp:
                        await OpenAppTaskService.OpenApp(value);
                        break;

                    case CommandCodes.SetAlarm:
                        await AlarmTaskService.SetAlarm(value);
                        break;

                    case CommandCodes.StartTimer:
                        if (int.TryParse(value, out int seconds))
                            await TimerTaskService.StartTimer(seconds);
                        break;

                    case CommandCodes.Wifi:
                        await WifiConnectionTaskService.ConnectToWifi(value);
                        break;

                    case CommandCodes.Contact:
                        await AddContactTaskService.AddContact(value);
                        break;

                    case CommandCodes.Bluetooth:
                        await BluetoothTaskService.HandleCommand(value);
                        break;

                    case CommandCodes.Spotify:
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.spotify.music");
                        break;

                    case CommandCodes.YouTube:
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.google.android.youtube");
                        break;

                    case CommandCodes.Instagram:
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.instagram.android");
                        break;

                    case CommandCodes.GoogleMaps:
                        await LinkTaskService.OpenUrlWithBrowser(value, "com.google.android.apps.maps");
                        break;

                    default:
                        Debug.WriteLine($">>> GUMBY: Unknown command '{command}'");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> GUMBY Error: {ex.Message}");
            }
        }

        private static async Task HandleVolume(string value)
        {
            var volumeParts = value.Split('|');
            var volumeType = volumeParts[0]; // "0"=ringtone, "1"=media, "2"=mute, "3"=unmute, "4"=vibrate

            switch (volumeType)
            {
                case "0":
                case "1":
                    var volumeLevel = volumeParts.Length > 1 ? volumeParts[1] : "50";
                    if (int.TryParse(volumeLevel, out int level))
                    {
                        if (volumeType == "0")
                            await RingtoneTaskService.SetRingtoneVolume(level);
                        else
                            await RingtoneTaskService.SetMediaVolume(level);
                    }
                    break;
                case "2":
                    await RingtoneTaskService.RingtoneMute();
                    break;
                case "3":
                    await RingtoneTaskService.RingtoneOn();
                    break;
                case "4":
                    await RingtoneTaskService.RingtoneVibrate();
                    break;
            }
        }
    }
}
