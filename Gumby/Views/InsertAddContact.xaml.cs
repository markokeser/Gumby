using System;
using Gumby.Services;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertAddContact : ContentPage
    {
        private bool _returnToCaller;
        public Action<string, string>? OnValueReturn { get; set; }

        // Konstruktor za normalan rad (otvara NfcWriterPage)
        public InsertAddContact()
        {
            InitializeComponent();
            _returnToCaller = false;
        }

        // Konstruktor za return mode (vraća vrednost MultiCommandBuilder-u)
        public InsertAddContact(bool returnToCaller)
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
                // Popuni polje sa brojem telefona
                PhoneEntry.Text = number;
                NameEntry.Text = name;
            };
            await Navigation.PushAsync(contactPage);
        }

        private async void OnWriteClicked(object sender, EventArgs e)
        {
            var name = NameEntry.Text?.Trim();
            var phone = PhoneEntry.Text?.Trim();
            var email = EmailEntry.Text?.Trim();
            var company = CompanyEntry.Text?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                await DisplayAlert("Error", "Please enter at least a name.", "OK");
                return;
            }

            // Generiši vCard
            var vcard = GenerateVCard(name, phone, email, company);
            var encodedData = Uri.EscapeDataString(vcard);
            var commandData = $"8|{encodedData}";  // Kod 8 = CONTACT
            var displayText = $"👤 Contact: {name}";

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

        private string GenerateVCard(string name, string phone, string email, string company)
        {
            var vcard = $"BEGIN:VCARD\nVERSION:3.0\nFN:{name}\n";

            if (!string.IsNullOrEmpty(phone))
                vcard += $"TEL:{phone}\n";

            if (!string.IsNullOrEmpty(email))
                vcard += $"EMAIL:{email}\n";

            if (!string.IsNullOrEmpty(company))
                vcard += $"ORG:{company}\n";

            vcard += "END:VCARD";
            return vcard;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}