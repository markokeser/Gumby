using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertConnection : ContentPage
    {
        public InsertConnection()
        {
            InitializeComponent();
        }

        // ========== WiFi Methods ==========

        private async void OnWifiConnectClicked(object sender, EventArgs e)
        {
            // Otvori postojeću InsertWifiConnection stranicu
            var wifiPage = new InsertWifiConnection();
            await Navigation.PushAsync(wifiPage);
        }

        private void OnWifiSettingsClicked(object sender, EventArgs e)
        {
            // Otvori WiFi podešavanja na telefonu
#if ANDROID
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionWifiSettings);
            intent.SetFlags(Android.Content.ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
#endif
        }

        // ========== Bluetooth Methods ==========

        private async void OnBluetoothToggleClicked(object sender, EventArgs e)
        {
            await Gumby.Tasks.BluetoothTaskService.EnsurePermissionAsync();
            var commandData = "10|2";  // Kod 10 = Bluetooth, 2 = Toggle
            var nfcPage = new NfcWriterPage(commandData, "Bluetooth: Toggle");
            await Navigation.PushAsync(nfcPage);
        }

        private async void OnBluetoothOnClicked(object sender, EventArgs e)
        {
            await Gumby.Tasks.BluetoothTaskService.EnsurePermissionAsync();
            var commandData = "10|1";  // Kod 10 = Bluetooth, 1 = On
            var nfcPage = new NfcWriterPage(commandData, "Bluetooth: On");
            await Navigation.PushAsync(nfcPage);
        }

        private async void OnBluetoothOffClicked(object sender, EventArgs e)
        {
            await Gumby.Tasks.BluetoothTaskService.EnsurePermissionAsync();
            var commandData = "10|0";  // Kod 10 = Bluetooth, 0 = Off
            var nfcPage = new NfcWriterPage(commandData, "Bluetooth: Off");
            await Navigation.PushAsync(nfcPage);
        }

        private void OnBluetoothPairClicked(object sender, EventArgs e)
        {
            // Otvori Bluetooth podešavanja za uparivanje
#if ANDROID
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionBluetoothSettings);
            intent.SetFlags(Android.Content.ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
#endif
        }

        private void OnBluetoothSettingsClicked(object sender, EventArgs e)
        {
            // Otvori Bluetooth podešavanja
#if ANDROID
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionBluetoothSettings);
            intent.SetFlags(Android.Content.ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
#endif
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}