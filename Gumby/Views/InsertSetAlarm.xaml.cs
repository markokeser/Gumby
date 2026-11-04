namespace Gumby.Views;

public partial class InsertSetAlarm : ContentPage
{
    private bool _isPM = false;
    private bool _use24HourFormat = true;
    private bool _returnToCaller;
    public Action<string, string>? OnValueReturn { get; set; }

    // Konstruktor za normalan rad
    public InsertSetAlarm()
    {
        InitializeComponent();

        // Set default selections
        HourPicker.SelectedIndex = DateTime.Now.Hour;
        MinutePicker.SelectedIndex = DateTime.Now.Minute;
        _returnToCaller = false;
    }

    // Konstruktor za return mode
    public InsertSetAlarm(bool returnToCaller)
    {
        InitializeComponent();

        // Set default selections
        HourPicker.SelectedIndex = DateTime.Now.Hour;
        MinutePicker.SelectedIndex = DateTime.Now.Minute;
        _returnToCaller = returnToCaller;
    }

    private void OnAMClicked(object sender, EventArgs e)
    {
        _isPM = false;
        _use24HourFormat = false;
        AMBtn.BackgroundColor = Colors.LightGreen;
        AMBtn.TextColor = Colors.Black;
        PMBtn.BackgroundColor = Color.FromArgb("#2A4A6B");
        PMBtn.TextColor = Colors.White;
        AmPmLabel.Text = "Using 12-hour format (AM)";
    }

    private void OnPMClicked(object sender, EventArgs e)
    {
        _isPM = true;
        _use24HourFormat = false;
        PMBtn.BackgroundColor = Colors.LightGreen;
        PMBtn.TextColor = Colors.Black;
        AMBtn.BackgroundColor = Color.FromArgb("#2A4A6B");
        AMBtn.TextColor = Colors.White;
        AmPmLabel.Text = "Using 12-hour format (PM)";
    }

    private async void OnWriteClicked(object sender, EventArgs e)
    {
        try
        {
            int hour = HourPicker.SelectedIndex;
            int minute = MinutePicker.SelectedIndex;

            // Convert to 24-hour format if using 12-hour
            if (!_use24HourFormat)
            {
                if (_isPM && hour != 12)
                    hour += 12;
                else if (!_isPM && hour == 12)
                    hour = 0;
            }

            // Validate time
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
            {
                await DisplayAlert("Error", "Invalid time selected.", "OK");
                return;
            }

            // Format as HH:MM
            var timeString = $"{hour:D2}:{minute:D2}";
            var commandData = $"5|{timeString}";
            var displayText = $"⏰ Alarm at {timeString}";

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
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}