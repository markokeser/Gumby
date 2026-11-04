using System;
using Gumby.Tasks;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertWifiConnection : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        // Konstruktor za normalan rad
        public InsertWifiConnection()
        {
            InitializeComponent();

            // Set default encryption selection
            EncryptionPicker.SelectedIndex = 0;

            // Bind password visibility
            PasswordEntry.BindingContext = this;
            ShowPasswordCheck.CheckedChanged += OnShowPasswordCheckedChanged;
            _returnToCaller = false;
        }

        // Konstruktor za return mode
        public InsertWifiConnection(bool returnToCaller)
        {
            InitializeComponent();

            // Set default encryption selection
            EncryptionPicker.SelectedIndex = 0;

            // Bind password visibility
            PasswordEntry.BindingContext = this;
            ShowPasswordCheck.CheckedChanged += OnShowPasswordCheckedChanged;
            _returnToCaller = returnToCaller;
        }

        private void OnShowPasswordTapped(object sender, EventArgs e)
        {
            ShowPasswordCheck.IsChecked = !ShowPasswordCheck.IsChecked;
        }

        private void OnShowPasswordCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            PasswordEntry.IsPassword = !e.Value;
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            var ssid = SsidEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(ssid))
            {
                await DisplayAlert("Error", "Please enter WiFi network name (SSID).", "OK");
                return;
            }

            // Get encryption type
            string encryption = "";
            var selectedIndex = EncryptionPicker.SelectedIndex;

            switch (selectedIndex)
            {
                case 0: // WPA/WPA2
                    encryption = "WPA";
                    break;
                case 1: // WEP
                    encryption = "WEP";
                    break;
                case 2: // WPA3
                    encryption = "WPA3";
                    break;
                case 3: // Open
                    encryption = "nopass";
                    password = "";
                    break;
                default:
                    encryption = "WPA";
                    break;
            }

            // Format WiFi connection string (bez WIFI: prefiksa)
            var wifiData = $"S:{WifiConnectionTaskService.EscapeWifiValue(ssid)};" +
                           $"T:{encryption};" +
                           $"P:{WifiConnectionTaskService.EscapeWifiValue(password ?? "")};;";
            var commandData = $"7|{Uri.EscapeDataString(wifiData)}";
            var displayText = $"📶 WiFi: {ssid}";

            if (_returnToCaller && OnValueReturn != null)
            {
                OnValueReturn.Invoke(commandData, displayText);
                await Navigation.PopAsync();
            }
            else
            {
                var nfcPage = new NfcWriterPage(commandData, displayText);
                await Navigation.PushAsync(nfcPage);
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}