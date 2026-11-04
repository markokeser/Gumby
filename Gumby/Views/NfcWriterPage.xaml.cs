using Plugin.NFC;
using System.Diagnostics;

namespace Gumby.Views;

public partial class NfcWriterPage : ContentPage
{
    private string _dataToWrite;
    private string _displayName;
    private CancellationTokenSource _timeoutCts;
    private int _timeoutSeconds = 10;
    private int _remainingSeconds;

    public NfcWriterPage(string dataToWrite, string displayName)
    {
        InitializeComponent();

        _dataToWrite = dataToWrite;
        _displayName = displayName;

        DataLabel.Text = $"{_displayName}\n{_dataToWrite}";
        DataLabel.IsVisible = true;

        // SET VARIABLES IN MAINACTIVITY
        SetWriteMode();

//        StartTimer();
    }

    private void SetWriteMode()
    {
#if ANDROID
        // JUST SET THESE VARIABLES - NOTHING ELSE
        MainActivity.IsWriteModeActive = true;
        MainActivity.PendingWriteText = _dataToWrite;
        MainActivity.WriteCompletionSource = new TaskCompletionSource<bool>();

        Debug.WriteLine($">>> Write mode set with: {_dataToWrite}");
#endif
        StatusLabel.Text = "✅ Ready - Tap your NFC chip";
        StatusLabel.TextColor = Colors.LightGreen;
    }

    private void ClearWriteMode()
    {
#if ANDROID
        // CLEAR VARIABLES
        MainActivity.IsWriteModeActive = false;
        MainActivity.PendingWriteText = null;
        MainActivity.WriteCompletionSource = null;
#endif
    }

    private void StartTimer()
    {
        _remainingSeconds = _timeoutSeconds;
        TimerLabel.Text = $"Time remaining: {_remainingSeconds} seconds";

        _timeoutCts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (_remainingSeconds > 0 && !_timeoutCts.IsCancellationRequested)
            {
                await Task.Delay(1000);
                _remainingSeconds--;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TimerLabel.Text = $"Time remaining: {_remainingSeconds} seconds";
                });
            }

            if (_remainingSeconds <= 0)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    StatusLabel.Text = "⏰ Timeout - No chip detected";
                    StatusLabel.TextColor = Colors.Red;
                    await Task.Delay(1000);

                    ClearWriteMode();
                    await Navigation.PopAsync();
                });
            }
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        _timeoutCts?.Cancel();
        ClearWriteMode();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Navigation.PopAsync();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timeoutCts?.Cancel();
        ClearWriteMode();
    }
}