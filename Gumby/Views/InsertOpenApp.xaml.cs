namespace Gumby.Views;

public partial class InsertOpenApp : ContentPage
{
    private string _selectedPackageName = "";
    private string _selectedAppName = "";
    private bool _returnToCaller;
    public Action<string, string>? OnValueReturn { get; set; }

    // Konstruktor za normalan rad
    public InsertOpenApp()
    {
        InitializeComponent();
        _returnToCaller = false;
    }

    // Konstruktor za return mode
    public InsertOpenApp(bool returnToCaller)
    {
        InitializeComponent();
        _returnToCaller = returnToCaller;
    }

    private async void OnSelectAppClicked(object sender, EventArgs e)
    {
        var appPicker = new AppPickerPage();

        // Use OnAppSelectedCallback instead of OnAppSelected
        appPicker.OnAppSelectedCallback = (appName, packageName) =>
        {
            _selectedAppName = appName;
            _selectedPackageName = packageName;

            SelectedAppLabel.Text = $"Selected: {appName}";
            SelectedAppLabel.TextColor = Colors.LightGreen;
            WriteBtn.IsEnabled = true;
        };

        await Navigation.PushAsync(appPicker);
    }

    private async void OnWriteClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedPackageName))
        {
            await DisplayAlert("Error", "Please select an app first.", "OK");
            return;
        }

        var commandData = $"4|{_selectedPackageName}";
        var displayText = $"📱 Open: {_selectedAppName}";

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