using Gumby.Services;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertCall : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        // Konstruktor za normalan rad (otvara NfcWriterPage)
        public InsertCall()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        // Konstruktor za return mode (vraća vrednost MultiCommandBuilder-u)
        public InsertCall(bool returnToCaller)
        {
            InitializeComponent();
            _returnToCaller = returnToCaller;

            // Iz Multi Task-a se korak samo dodaje u listu - upis na čip je tek na kraju
            if (returnToCaller)
                WriteBtn.Text = "Add step";
        }

        private async void OnSelectContactClicked(object sender, EventArgs e)
        {
            var contactPage = new ContactPickerPage();
            contactPage.OnContactSelected = (name, number) =>
            {
                NumberEntry.Text = number;
            };
            await Navigation.PushAsync(contactPage);
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            var numberText = NumberEntry.Text?.Trim();

            if (string.IsNullOrEmpty(numberText))
            {
                await DisplayAlert("Error", "Please enter a phone number.", "OK");
                return;
            }

            var commandData = $"2|{numberText}";  // Kod 2 = CALL
            var displayText = $"📞 Call: {numberText}";

            if (_returnToCaller && OnValueReturn != null)
            {
                // Vrati vrednost MultiCommandBuilder-u
                OnValueReturn.Invoke(commandData, displayText);
                await Navigation.PopAsync();
            }
            else
            {
                // Normalan rad - otvori NfcWriterPage
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