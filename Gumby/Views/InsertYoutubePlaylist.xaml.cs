using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertYoutubePlaylist : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        public InsertYoutubePlaylist()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        public InsertYoutubePlaylist(bool returnToCaller)
        {
            InitializeComponent();
            _returnToCaller = returnToCaller;
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            var input = PlaylistEntry.Text?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                await DisplayAlert("Error", "Please enter YouTube playlist ID or URL.", "OK");
                return;
            }

            var playlistId = ExtractPlaylistId(input);

            if (string.IsNullOrEmpty(playlistId))
            {
                await DisplayAlert("Error", "Invalid YouTube playlist ID or URL.", "OK");
                return;
            }

        //    var url = $"https://www.youtube.com/playlist?list={playlistId}";
              var url = playlistId;

            // KORISTI KOD 12 ZA YOUTUBE
            var commandData = $"12|{url}";
            var displayText = $"🎬 YouTube Playlist";

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
            // Ako je već samo ID (npr. PLxZx123456789)
            if (input.StartsWith("PL") && !input.Contains("/") && !input.Contains("?"))
            {
                return input;
            }

            // Ekstrakcija iz URL-a
            if (input.Contains("youtube.com") || input.Contains("youtu.be"))
            {
                /*   if (input.Contains("list=") || input.Contains("list="))
                   {
                       var start = input.IndexOf("list=") + 5;
                       var end = input.IndexOf("&", start);
                       if (end == -1) end = input.Length;
                       return input.Substring(start, end - start);
                   } */
                return input;
            }

            return null;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}