using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gumby.Services;
using Microsoft.Maui.Controls;

namespace Gumby.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadBrowserSettings();
        }

        private async void LoadBrowserSettings()
        {
            // Samo učitava trenutna podešavanja, NE otvara dijalog
            BrowserSwitch.IsToggled = BrowserSettingsService.UseCustomBrowser;
            BrowserSelectionFrame.IsVisible = BrowserSettingsService.UseCustomBrowser;

            if (BrowserSettingsService.UseCustomBrowser)
            {
                SelectedBrowserLabel.Text = BrowserSettingsService.SelectedBrowserName;
                SelectedBrowserPackageLabel.Text = BrowserSettingsService.SelectedBrowserPackage;
                BrowserInfoLabel.Text = "Links will open in your chosen browser";
            }
            else
            {
                var defaultBrowser = await BrowserSettingsService.GetDefaultBrowser();
                SelectedBrowserLabel.Text = defaultBrowser.BrowserName;
                SelectedBrowserPackageLabel.Text = defaultBrowser.PackageName;
                BrowserInfoLabel.Text = "Using system default browser";
            }
        }

        private async void OnBrowserToggled(object sender, ToggledEventArgs e)
        {
            BrowserSelectionFrame.IsVisible = e.Value;

            if (!e.Value)
            {
                // Vrati se na default browser
                BrowserSettingsService.ResetToDefault();
                var defaultBrowser = await BrowserSettingsService.GetDefaultBrowser();
                SelectedBrowserLabel.Text = defaultBrowser.BrowserName;
                SelectedBrowserPackageLabel.Text = defaultBrowser.PackageName;
                BrowserInfoLabel.Text = "Using system default browser";
            }
            else
            {
                BrowserInfoLabel.Text = "Select browser from list below";
            }
        }

        private async void OnSelectBrowserClicked(object sender, EventArgs e)
        {
            await ShowBrowserPicker();
        }

        private async Task ShowBrowserPicker()
        {
            try
            {
                var browsers = await BrowserSettingsService.GetInstalledBrowsers();

                if (browsers == null || browsers.Count == 0)
                {
                    var manualEntry = await DisplayAlert("No Browser Found",
                        "Could not detect your browser.\n\n" +
                        "Would you like to enter the package name manually?",
                        "Manual Entry", "Cancel");

                    if (manualEntry)
                    {
                        await ShowManualBrowserEntry();
                    }
                    return;
                }

                var options = browsers.Select(b => b.BrowserName).ToArray();

                var selected = await DisplayActionSheet("Select Browser", "Cancel", null, options);

                if (!string.IsNullOrEmpty(selected) && selected != "Cancel")
                {
                    var selectedBrowser = browsers.FirstOrDefault(b => b.BrowserName == selected);

                    if (selectedBrowser != null)
                    {
                        BrowserSettingsService.SaveSelectedBrowser(
                            selectedBrowser.PackageName,
                            selectedBrowser.BrowserName);

                        SelectedBrowserLabel.Text = selectedBrowser.BrowserName;
                        SelectedBrowserPackageLabel.Text = selectedBrowser.PackageName;
                        BrowserInfoLabel.Text = $"Links will open in {selectedBrowser.BrowserName}";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowBrowserPicker error: {ex.Message}");
                await DisplayAlert("Error", "Failed to load browsers list.", "OK");
            }
        }

        private async Task ShowManualBrowserEntry()
        {
            var packageName = await DisplayPromptAsync("Manual Entry",
                "Enter browser package name:\n\n" +
                "Examples:\n" +
                "com.android.chrome (Chrome)\n" +
                "com.brave.browser (Brave)\n" +
                "org.mozilla.firefox (Firefox)",
                placeholder: "com.android.chrome",
                maxLength: 100,
                keyboard: Keyboard.Text);

            if (!string.IsNullOrEmpty(packageName))
            {
                var browserName = await DisplayPromptAsync("Browser Name",
                    "Enter display name for this browser:",
                    placeholder: packageName.Split('.').Last());

                if (!string.IsNullOrEmpty(browserName))
                {
                    BrowserSettingsService.SaveSelectedBrowser(packageName, browserName);

                    SelectedBrowserLabel.Text = browserName;
                    SelectedBrowserPackageLabel.Text = packageName;
                    BrowserInfoLabel.Text = $"Links will open in {browserName}";
                }
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}