using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertPlaylist : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        // Konstruktor za normalan rad
        public InsertPlaylist()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        // Konstruktor za return mode
        public InsertPlaylist(bool returnToCaller)
        {
            InitializeComponent();
            _returnToCaller = returnToCaller;
        }

        private async void OnYoutubeClicked(object sender, EventArgs e)
        {
            if (_returnToCaller && OnValueReturn != null)
            {
                var youtubePage = new InsertYoutubePlaylist(true);
                youtubePage.OnValueReturn = (command, displayText) =>
                {
                    OnValueReturn.Invoke(command, displayText);
                };
                await Navigation.PushAsync(youtubePage);
            }
            else
            {
                await Navigation.PushAsync(new InsertYoutubePlaylist());
            }
        }

        private async void OnSpotifyClicked(object sender, EventArgs e)
        {
            if (_returnToCaller && OnValueReturn != null)
            {
                var spotifyPage = new InsertSpotifyPlaylist(true);
                spotifyPage.OnValueReturn = (command, displayText) =>
                {
                    OnValueReturn.Invoke(command, displayText);
                };
                await Navigation.PushAsync(spotifyPage);
            }
            else
            {
                await Navigation.PushAsync(new InsertSpotifyPlaylist());
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}