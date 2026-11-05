using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertInstagram : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        public InsertInstagram()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        public InsertInstagram(bool returnToCaller)
        {
            InitializeComponent();
            _returnToCaller = returnToCaller;

            // Iz Multi Task-a se korak samo dodaje u listu - upis na čip je tek na kraju
            if (returnToCaller)
                WriteBtn.Text = "Add step";
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            var input = ReelEntry.Text?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                await DisplayAlert("Error", "Please enter Instagram reel URL.", "OK");
                return;
            }

            var reelUrl = ExtractReelUrl(input);

            if (string.IsNullOrEmpty(reelUrl))
            {
                await DisplayAlert("Error", "Invalid Instagram reel URL.", "OK");
                return;
            }

            // KORISTI KOD 13 ZA INSTAGRAM
            var commandData = $"13|{reelUrl}";
            var displayText = $"📸 Instagram Reel";

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

        private string ExtractReelUrl(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

   //         if (input.Contains("instagram.com/reel/"))
            {
   //             if (!input.StartsWith("http"))
                {
  //                  input = "https://" + input;
                }

                var reelMatch = System.Text.RegularExpressions.Regex.Match(input, @"(https?://)?(www\.)?instagram\.com/reel/[a-zA-Z0-9_-]+");
                if (reelMatch.Success)
                {
                    return reelMatch.Value;
                }

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