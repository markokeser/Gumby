namespace Gumby.Views;

/// <summary>
/// Ekran za izbor vrste koraka u Multi Task rutini.
/// Koristi se preko <see cref="PickAsync"/>, koji vraća ClassId izabrane opcije
/// (npr. "wifi", "bt_on") ili null ako se korisnik vratio nazad.
/// </summary>
public partial class StepPickerPage : ContentPage
{
    // Koraci koji se dodaju odmah, bez dodatnog ekrana za unos
    private static readonly HashSet<string> InstantSteps = new()
    {
        "mute", "vibrate", "bt_on", "bt_off", "bt_toggle",
    };

    private readonly TaskCompletionSource<string?> _result = new();
    private bool _closing;

    public StepPickerPage()
    {
        InitializeComponent();
    }

    public static async Task<string?> PickAsync(INavigation navigation)
    {
        var page = new StepPickerPage();
        await navigation.PushAsync(page);
        return await page._result.Task;
    }

    private async void OnStepClicked(object? sender, EventArgs e)
    {
        if (_closing || sender is not Element element || string.IsNullOrEmpty(element.ClassId))
            return;

        _closing = true;
        var step = element.ClassId;

        // Ako sledi ekran za unos, zatvori bez animacije da izgleda kao direktan prelaz
        await Navigation.PopAsync(animated: InstantSteps.Contains(step));
        _result.TrySetResult(step);
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (_closing)
            return;

        _closing = true;
        await Navigation.PopAsync();
        _result.TrySetResult(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Sistemsko dugme "nazad" - nijedan korak nije izabran.
        // Kada je korak izabran (_closing), rezultat se postavlja posle zatvaranja.
        if (!_closing)
            _result.TrySetResult(null);
    }
}
