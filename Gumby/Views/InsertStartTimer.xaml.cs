using System;

namespace Gumby.Views;

public partial class InsertStartTimer : ContentPage
{
    private bool _returnToCaller;
    public Action<string, string>? OnValueReturn { get; set; }

    // Konstruktor za normalan rad
    public InsertStartTimer()
    {
        InitializeComponent();

        // Set default values
        HourPicker.SelectedIndex = 0;
        MinutePicker.SelectedIndex = 0;
        SecondPicker.SelectedIndex = 2; // 10 seconds default

        // Subscribe to selection changes
        HourPicker.SelectedIndexChanged += OnTimeChanged;
        MinutePicker.SelectedIndexChanged += OnTimeChanged;
        SecondPicker.SelectedIndexChanged += OnTimeChanged;

        UpdateDurationLabel();
        _returnToCaller = false;
    }

    // Konstruktor za return mode
    public InsertStartTimer(bool returnToCaller)
    {
        InitializeComponent();

        // Set default values
        HourPicker.SelectedIndex = 0;
        MinutePicker.SelectedIndex = 0;
        SecondPicker.SelectedIndex = 2; // 10 seconds default

        // Subscribe to selection changes
        HourPicker.SelectedIndexChanged += OnTimeChanged;
        MinutePicker.SelectedIndexChanged += OnTimeChanged;
        SecondPicker.SelectedIndexChanged += OnTimeChanged;

        UpdateDurationLabel();
        _returnToCaller = returnToCaller;

        // Iz Multi Task-a se korak samo dodaje u listu - upis na čip je tek na kraju
        if (returnToCaller)
            WriteBtn.Text = "Add step";
    }

    private void OnTimeChanged(object sender, EventArgs e)
    {
        UpdateDurationLabel();
    }

    private void UpdateDurationLabel()
    {
        int hours = HourPicker.SelectedIndex;
        int minutes = MinutePicker.SelectedIndex;
        int seconds = int.Parse(SecondPicker.SelectedItem?.ToString() ?? "0");

        DurationLabel.Text = $"Duration: {hours}h {minutes}m {seconds}s";
    }

    private int GetTotalSeconds()
    {
        int hours = HourPicker.SelectedIndex;
        int minutes = MinutePicker.SelectedIndex;
        int seconds = int.Parse(SecondPicker.SelectedItem?.ToString() ?? "0");

        return (hours * 3600) + (minutes * 60) + seconds;
    }

    private async void OnWriteClicked(object sender, EventArgs e)
    {
        int totalSeconds = GetTotalSeconds();

        if (totalSeconds <= 0)
        {
            await DisplayAlert("Error", "Please set a duration greater than 0 seconds.", "OK");
            return;
        }

        var commandData = $"6|{totalSeconds}";
        var displayTime = $"{HourPicker.SelectedIndex}h {MinutePicker.SelectedIndex}m {SecondPicker.SelectedItem}s";
        var displayText = $"⏱️ Timer: {displayTime}";

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