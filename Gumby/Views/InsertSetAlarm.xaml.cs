using Gumby.Controls;

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

        // Iz Multi Task-a se korak samo dodaje u listu - upis na čip je tek na kraju
        if (returnToCaller)
            WriteBtn.Text = "Add step";
    }

    private void OnAMClicked(object sender, EventArgs e)
    {
        _isPM = false;
        _use24HourFormat = false;
        SetChipSelected(AMBtn, true);
        SetChipSelected(PMBtn, false);
        AmPmLabel.Text = "Using 12-hour format (AM)";
    }

    private void OnPMClicked(object sender, EventArgs e)
    {
        _isPM = true;
        _use24HourFormat = false;
        SetChipSelected(PMBtn, true);
        SetChipSelected(AMBtn, false);
        AmPmLabel.Text = "Using 12-hour format (PM)";
    }

    private static void SetChipSelected(Button button, bool selected)
    {
        button.BackgroundColor = selected ? AppColors.Accent : AppColors.SurfaceRaised;
        button.TextColor = selected ? AppColors.TextOnAccent : AppColors.TextPrimary;
        button.BorderColor = selected ? AppColors.Accent : AppColors.Stroke;
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