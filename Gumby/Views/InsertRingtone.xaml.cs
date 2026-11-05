using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertRingtone : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        // Konstruktor za normalan rad
        public InsertRingtone()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        // Konstruktor za return mode
        public InsertRingtone(bool returnToCaller)
        {
            InitializeComponent();
            _returnToCaller = returnToCaller;

            if (returnToCaller)
                ModeHintLabel.Text = "Choosing an option adds it as a step to your routine.";
        }

        // Ringtone Volume
        private async void OnRingtone0Clicked(object sender, EventArgs e)
        {
            var commandData = "1|0|0";
            var displayText = "Ringtone 0%";

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
        private async void OnRingtone25Clicked(object sender, EventArgs e)
        {
            var commandData = "1|0|25";
            var displayText = "Ringtone 25%";

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
        private async void OnRingtone50Clicked(object sender, EventArgs e)
        {
            var commandData = "1|0|50";
            var displayText = "Ringtone 50%";

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
        private async void OnRingtone75Clicked(object sender, EventArgs e)
        {
            var commandData = "1|0|75";
            var displayText = "Ringtone 75%";

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
        private async void OnRingtone100Clicked(object sender, EventArgs e)
        {
            var commandData = "1|0|100";
            var displayText = "Ringtone 100%";

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

        // Media Volume
        private async void OnMedia0Clicked(object sender, EventArgs e)
        {
            var commandData = "1|1|0";
            var displayText = "Media Volume 0%";

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
        private async void OnMedia25Clicked(object sender, EventArgs e)
        {
            var commandData = "1|1|25";
            var displayText = "Media Volume 25%";

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
        private async void OnMedia50Clicked(object sender, EventArgs e)
        {
            var commandData = "1|1|50";
            var displayText = "Media Volume 50%";

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
        private async void OnMedia75Clicked(object sender, EventArgs e)
        {
            var commandData = "1|1|75";
            var displayText = "Media Volume 75%";

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
        private async void OnMedia100Clicked(object sender, EventArgs e)
        {
            var commandData = "1|1|100";
            var displayText = "Media Volume 100%";

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

        // Sound Controls
        private async void OnMuteClicked(object sender, EventArgs e)
        {
            var commandData = "1|2";
            var displayText = "Mute";

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
        private async void OnUnmuteClicked(object sender, EventArgs e)
        {
            var commandData = "1|3";
            var displayText = "Unmute";

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
        private async void OnVibrateClicked(object sender, EventArgs e)
        {
            var commandData = "1|4";
            var displayText = "Vibrate";

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