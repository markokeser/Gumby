using Gumby.Controls;
using System.Diagnostics;

namespace Gumby.Views;

public partial class NfcWriterPage : ContentPage
{
    private string _dataToWrite;
    private string _displayName;
    private CancellationTokenSource? _timeoutCts;
    private bool _pulsing;
    private int _timeoutSeconds = 10;
    private int _remainingSeconds;

    public NfcWriterPage(string dataToWrite, string displayName)
    {
        InitializeComponent();

        _dataToWrite = dataToWrite;
        _displayName = displayName;

        ActionNameLabel.Text = _displayName;
        DataLabel.Text = _dataToWrite;
        DataLabel.IsVisible = !string.IsNullOrEmpty(_dataToWrite);

        // SET VARIABLES IN MAINACTIVITY
        SetWriteMode();

//        StartTimer();
    }

    private void SetWriteMode()
    {
        ArmWriteMode();
        SetStatus("Waiting for NFC chip…", AppColors.Accent);
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

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (success)
            {
                _timeoutCts?.Cancel();
                ShowSuccess();
            }
            else
            {
                // Upis nije uspeo - ostavi stranicu otvorenu i omogući novi pokušaj
                ShowFailure();
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
                    SetStatus("Timed out — no chip detected", AppColors.Danger);
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

    // ===================== IZGLED =====================

    private void SetStatus(string text, Color color)
    {
        StatusLabel.Text = text;
        StatusLabel.TextColor = color;
        StatusPill.BackgroundColor = color.WithAlpha(0.14f);
    }

    private async void ShowSuccess()
    {
        _pulsing = false;
        TitleLabel.Text = "All done!";
        HintLabel.Text = "Your chip is ready. Tap it any time to run the action.";
        SetStatus("Chip written successfully", AppColors.Success);

        CenterIcon.Text = "✓";
        CenterIcon.FontSize = 56;
        CenterIcon.FontFamily = "OpenSansSemibold";
        CenterIcon.TextColor = AppColors.TextOnAccent;
        CenterBadge.Background = new SolidColorBrush(AppColors.Success);
        CloseBtn.Text = "Done";

        await CenterBadge.ScaleToAsync(1.08, 120, Easing.CubicOut);
        await CenterBadge.ScaleToAsync(1, 160, Easing.CubicIn);
    }

    private async void ShowFailure()
    {
        SetStatus("Couldn't write — hold the chip again", AppColors.Danger);

        // Kratko "odmahivanje" kao povratna informacija
        for (int i = 0; i < 2; i++)
        {
            await CenterBadge.TranslateToAsync(-10, 0, 50);
            await CenterBadge.TranslateToAsync(10, 0, 50);
        }
        await CenterBadge.TranslateToAsync(0, 0, 50);
    }

    private async void StartPulse()
    {
        if (_pulsing)
            return;

        _pulsing = true;
        while (_pulsing)
        {
            var first = PulseRing(Ring1);
            await Task.Delay(700);
            var second = PulseRing(Ring2);
            await Task.WhenAll(first, second);
        }
    }

    private static async Task PulseRing(VisualElement ring)
    {
        ring.Scale = 0.6;
        ring.Opacity = 0.8;
        await Task.WhenAll(
            ring.ScaleToAsync(1.0, 1400, Easing.CubicOut),
            ring.FadeToAsync(0, 1400, Easing.CubicIn));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartPulse();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _pulsing = false;
        _timeoutCts?.Cancel();
        ClearWriteMode();
    }
}