namespace Gumby.Views;

public partial class InsertLink : ContentPage
{
    private bool _returnToCaller;
    public Action<string, string>? OnValueReturn { get; set; }

    // Konstruktor za normalan rad
    public InsertLink()
    {
        InitializeComponent();
        _returnToCaller = false;
    }

    // Konstruktor za return mode
    public InsertLink(bool returnToCaller)
    {
        InitializeComponent();
        _returnToCaller = returnToCaller;
    }

    private async void OnWriteClicked(object sender, EventArgs e)
    {
        var link = LinkEntry.Text?.Trim();

        if (string.IsNullOrEmpty(link))
        {
            await DisplayAlert("Error", "Please enter a link.", "OK");
            return;
        }

        // Add https:// if missing
        if (!link.StartsWith("http://") && !link.StartsWith("https://"))
        {
            link = "https://" + link;
        }

        var commandData = $"0|{link}";
        var displayText = $"🔗 Link: {link}";

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