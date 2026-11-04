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
        ArmWriteMode();
        StatusLabel.Text = "✅ Ready - Tap your NFC chip";
        StatusLabel.TextColor = Colors.LightGreen;
    }

    private void ArmWriteMode()
    {
#if ANDROID
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        MainActivity.PendingWriteText = _dataToWrite;
        MainActivity.WriteCompletionSource = completion;
        MainActivity.IsWriteModeActive = true;

        Debug.WriteLine($">>> Write mode set with: {_dataToWrite}");

        _ = WaitForWriteResultAsync(completion);
#endif
    }

    private async Task WaitForWriteResultAsync(TaskCompletionSource<bool> completion)
    {
        bool success;
        try
        {
            success = await completion.Task;
        }
        catch (TaskCanceledException)
        {
            // Stranica je zatvorena pre nego što je čip prislonjen
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (success)
            {
                _timeoutCts?.Cancel();
                StatusLabel.Text = "✅ Chip written successfully!";
                StatusLabel.TextColor = Colors.LightGreen;

                await Task.Delay(1200);
                if (Navigation.NavigationStack.LastOrDefault() == this)
                    await Navigation.PopAsync();
            }
            else
            {
                // Upis nije uspeo - ostavi stranicu otvorenu i omogući novi pokušaj
                StatusLabel.Text = "❌ Write failed - tap the chip again";
                StatusLabel.TextColor = Colors.Red;
                ArmWriteMode();
            }
        });
    }

    private void ClearWriteMode()
    {
#if ANDROID
        // CLEAR VARIABLES
        MainActivity.IsWriteModeActive = false;
        MainActivity.PendingWriteText = null;
        MainActivity.WriteCompletionSource?.TrySetCanceled();
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