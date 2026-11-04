using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertSpotifyPlaylist : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        public InsertSpotifyPlaylist()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        public InsertSpotifyPlaylist(bool returnToCaller)
        {
            InitializeComponent();
            _returnToCaller = returnToCaller;
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            var input = PlaylistEntry.Text?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                await DisplayAlert("Error", "Please enter Spotify playlist ID or URL.", "OK");
                return;
            }

            var playlistId = ExtractPlaylistId(input);

            if (string.IsNullOrEmpty(playlistId))
            {
                await DisplayAlert("Error", "Invalid Spotify playlist ID or URL.", "OK");
                return;
            }

            var url = $"https://open.spotify.com/playlist/{playlistId}";

            // KORISTI KOD 11 ZA SPOTIFY
            var commandData = $"11|{url}";
            var displayText = $"🎧 Spotify Playlist";

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

        private string ExtractPlaylistId(string input)
        {
            // Ako je već samo ID
            if (input.Length > 10 && !input.Contains("/") && !input.Contains("?"))
            {
                return input;
            }

            // Ekstrakcija iz URL-a
            if (input.Contains("spotify.com"))
            {
                if (input.Contains("/playlist/"))
                {
                    var start = input.IndexOf("/playlist/") + 10;
                    var end = input.IndexOf("?", start);
                    if (end == -1) end = input.Length;
                    return input.Substring(start, end - start);
                }
            }

            return null;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}