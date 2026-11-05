using Gumby.Services;

namespace Gumby.Views;

public partial class InsertMessage : ContentPage
{
    private bool _returnToCaller;
    public Action<string, string>? OnValueReturn { get; set; }

    // Konstruktor za normalan rad
    public InsertMessage()
    {
        InitializeComponent();
        _returnToCaller = false;
    }

    // Konstruktor za return mode
    public InsertMessage(bool returnToCaller)
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
            PhoneEntry.Text = number;
        };
        await Navigation.PushAsync(contactPage);
    }

    private async void OnWriteClicked(object sender, EventArgs e)
    {
        var phoneNumber = PhoneEntry.Text?.Trim();

        if (string.IsNullOrEmpty(phoneNumber))
        {
            await DisplayAlert("Error", "Please enter a phone number.", "OK");
            return;
        }

        var message = MessageEntry.Text?.Trim();

        // Broj i poruka se URL-enkoduju, pa "|" ili "%" u poruci ne kvare komandu
        var commandData = CommandService.GenerateInsertSmsCommand(phoneNumber, message ?? "");
        var displayText = $"💬 SMS to: {phoneNumber}";

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