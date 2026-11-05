using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Gumby.Services;

namespace Gumby.Views
{
    public partial class MultiCommandBuilder : ContentPage
    {
        public ObservableCollection<CommandItem> Commands { get; set; }
        public ICommand RemoveCommand { get; }

        public MultiCommandBuilder()
        {
            InitializeComponent();
            Commands = new ObservableCollection<CommandItem>();
            CommandsView.ItemsSource = Commands;
      //      RemoveCommand = new Command<CommandItem>(RemoveCommandItem);
            BindingContext = this;
        }

        private void OnRemoveCommand(object sender, EventArgs e)
        {
            var button = sender as Button;
            var item = button?.CommandParameter as CommandItem;

            if (item != null && Commands.Contains(item))
            {
                Commands.Remove(item);
            }
        }

        private bool _pickingStep;

        private async void OnAddTaskClicked(object sender, EventArgs e)
        {
            if (_pickingStep)
                return;

            _pickingStep = true;
            try
            {
                var step = await StepPickerPage.PickAsync(Navigation);

                switch (step)
                {
                    case "volume": await AddVolumeCommand(); break;
                    case "mute": AddMuteCommand(); break;
                    case "vibrate": AddVibrateCommand(); break;

                    case "wifi": await AddWifiCommand(); break;
                    case "bt_on": await AddBluetoothCommand("1", "🔵 Bluetooth on"); break;
                    case "bt_off": await AddBluetoothCommand("0", "⚪ Bluetooth off"); break;
                    case "bt_toggle": await AddBluetoothCommand("2", "🔁 Bluetooth toggle"); break;

                    case "app": await AddOpenAppCommand(); break;
                    case "link": await AddLinkCommand(); break;
                    case "maps": await AddMapsCommand(); break;

                    case "spotify": await AddSpotifyCommand(); break;
                    case "youtube": await AddYoutubeCommand(); break;
                    case "instagram": await AddInstagramCommand(); break;

                    case "call": await AddCallCommand(); break;
                    case "sms": await AddSmsCommand(); break;
                    case "contact": await AddContactCommand(); break;

                    case "alarm": await AddAlarmCommand(); break;
                    case "timer": await AddTimerCommand(); break;
                }
            }
            finally
            {
                _pickingStep = false;
            }
        }

        // ========== METODE ZA DODAVANJE KORAKA ==========

        private void AddStep(string type, string value, string displayText)
        {
            Commands.Add(new CommandItem
            {
                Type = type,
                Value = value,
                DisplayText = displayText
            });
        }

        private async Task AddBluetoothCommand(string mode, string displayText)
        {
            await Gumby.Tasks.BluetoothTaskService.EnsurePermissionAsync();
            AddStep("BLUETOOTH", $"{CommandCodes.Bluetooth}|{mode}", displayText);
        }

        private async Task AddMapsCommand()
        {
            var page = new InsertGoogleMaps(returnToCaller: true);
            page.OnValueReturn = (command, displayText) => AddStep("MAPS", command, displayText);
            await Navigation.PushAsync(page);
        }

        private async Task AddSpotifyCommand()
        {
            var page = new InsertSpotifyPlaylist(returnToCaller: true);
            page.OnValueReturn = (command, displayText) => AddStep("SPOTIFY", command, displayText);
            await Navigation.PushAsync(page);
        }

        private async Task AddYoutubeCommand()
        {
            var page = new InsertYoutubePlaylist(returnToCaller: true);
            page.OnValueReturn = (command, displayText) => AddStep("YOUTUBE", command, displayText);
            await Navigation.PushAsync(page);
        }

        private async Task AddInstagramCommand()
        {
            var page = new InsertInstagram(returnToCaller: true);
            page.OnValueReturn = (command, displayText) => AddStep("INSTAGRAM", command, displayText);
            await Navigation.PushAsync(page);
        }

        private async Task AddLinkCommand()
        {
            var linkPage = new InsertLink(returnToCaller: true);
            linkPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "URL",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(linkPage);
        }  

        private async Task AddVolumeCommand()
        {
            var volumePage = new InsertRingtone(returnToCaller: true);
            volumePage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "VOLUME",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(volumePage);
        }

        private void AddMuteCommand()
        {
            Commands.Add(new CommandItem
            {
                Type = "VOLUME",
                Value = $"{CommandCodes.Volume}|2",  // Mute komanda
                DisplayText = "🔇 Mute"
            });
        }

        private void AddVibrateCommand()
        {
            // Prebacuje telefon u režim vibracije (bez trajanja - to nije podržano)
            Commands.Add(new CommandItem
            {
                Type = "VOLUME",
                Value = $"{CommandCodes.Volume}|4",
                DisplayText = "📳 Vibrate mode"
            });
        }

        private async Task AddSmsCommand()
        {
            var smsPage = new InsertMessage(returnToCaller: true);
            smsPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "SMS",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(smsPage);
        }  

        private async Task AddCallCommand()
        {
            var callPage = new InsertCall(returnToCaller: true);
            callPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "CALL",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(callPage);
        }

        private async Task AddWifiCommand()
        {
            var wifiPage = new InsertWifiConnection(returnToCaller: true);
            wifiPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "WIFI",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(wifiPage);
        }

        private async Task AddContactCommand()
        {
            var contactPage = new InsertAddContact(returnToCaller: true);
            contactPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "CONTACT",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(contactPage);
        }

        private async Task AddAlarmCommand()
        {
            var alarmPage = new InsertSetAlarm(returnToCaller: true);
            alarmPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "SET_ALARM",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(alarmPage);
        }

        private async Task AddTimerCommand()
        {
            var timerPage = new InsertStartTimer(returnToCaller: true);
            timerPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "START_TIMER",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(timerPage);
        }

        private async Task AddOpenAppCommand()
        {
            var appPage = new InsertOpenApp(returnToCaller: true);
            appPage.OnValueReturn = (command, displayText) =>
            {
                Commands.Add(new CommandItem
                {
                    Type = "OPEN_APP",
                    Value = command,
                    DisplayText = displayText
                });
            };
            await Navigation.PushAsync(appPage);
        }   

        private void OnClearClicked(object sender, EventArgs e)
        {
            Commands.Clear();
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            if (Commands.Count == 0)
            {
                await DisplayAlert("Error", "No commands added.", "OK");
                return;
            }

            // Kreiraj multi komandu sa separatorom \x1F.
            // Svaka vrednost već sadrži kod komande (npr. "0|https://...").
            var tasks = new List<string>();

            foreach (var cmd in Commands)
            {
                if (string.IsNullOrEmpty(cmd.Value))
                    continue;

                if (cmd.Value.Contains(CommandCodes.TaskSeparator))
                {
                    // Separator u vrednosti bi pokvario podelu komandi - upiši celu komandu kao base64
                    var encodedValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cmd.Value));
                    tasks.Add($"{CommandCodes.Base64}|{encodedValue}");
                }
                else
                {
                    tasks.Add(cmd.Value);
                }
            }

            var encodedTasks = string.Join(CommandCodes.TaskSeparator, tasks);
            var nfcPage = new NfcWriterPage(encodedTasks, $"Multi: {Commands.Count} commands");
            await Navigation.PushAsync(nfcPage);
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

    public class CommandItem
    {
        public string Type { get; set; } = "";
        public string Value { get; set; } = "";
        public string DisplayText { get; set; } = "";
    }
}