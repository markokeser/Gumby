using System;
using Gumby.Views;
using Gumby.Tasks;

namespace Gumby.Views
{
    public partial class InsertGoogleMaps : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        public InsertGoogleMaps()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        public InsertGoogleMaps(bool returnToCaller)
        {
            InitializeComponent();
            _returnToCaller = returnToCaller;
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            var location = LocationEntry.Text?.Trim();

            if (string.IsNullOrEmpty(location))
            {
                await DisplayAlert("Error", "Please enter a location.", "OK");
                return;
            }

            // Encode location for URL
            var encodedLocation = Uri.EscapeDataString(location);
            var url = $"https://www.google.com/maps/search/?api=1&query={encodedLocation}";

            var commandData = $"14|{url}";
            var displayText = $"🗺️ Maps: {location}";

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