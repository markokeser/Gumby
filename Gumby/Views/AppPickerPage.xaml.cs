using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Gumby.Views;

public partial class AppPickerPage : ContentPage
{
    public ObservableCollection<AppInfo> Apps { get; set; } = new();
    public Action<string, string>? OnAppSelectedCallback { get; set; }

    public AppPickerPage()
    {
        InitializeComponent();
        AppsCollectionView.ItemsSource = Apps;
        AppsCollectionView.SelectionChanged += OnAppSelectionChanged;
        SearchBar.TextChanged += OnSearchBarTextChanged;
        LoadApps();
    }

    private async void LoadApps()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            await Task.Run(() =>
            {
                var apps = GetInstalledApps();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var app in apps)
                    {
                        Apps.Add(app);
                    }

                    LoadingIndicator.IsVisible = false;
                    LoadingIndicator.IsRunning = false;
                });
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadApps error: {ex.Message}");
            LoadingIndicator.IsVisible = false;
            await DisplayAlert("Error", "Failed to load apps list.", "OK");
        }
    }

    private List<AppInfo> GetInstalledApps()
    {
        var appList = new List<AppInfo>();

#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var packageManager = context.PackageManager;

            var intent = new Android.Content.Intent(Android.Content.Intent.ActionMain);
            intent.AddCategory(Android.Content.Intent.CategoryLauncher);

            var resolveInfoList = packageManager.QueryIntentActivities(intent, 0);
            var processedPackages = new HashSet<string>();

            foreach (var resolveInfo in resolveInfoList)
            {
                var packageName = resolveInfo.ActivityInfo.PackageName;

                if (processedPackages.Contains(packageName))
                    continue;

                processedPackages.Add(packageName);

                try
                {
                    var appInfo = packageManager.GetApplicationInfo(packageName, 0);
                    var appName = packageManager.GetApplicationLabel(appInfo)?.ToString() ?? packageName;

                    appList.Add(new AppInfo
                    {
                        AppName = appName,
                        PackageName = packageName
                    });
                }
                catch
                {
                    appList.Add(new AppInfo
                    {
                        AppName = packageName,
                        PackageName = packageName
                    });
                }
            }

            appList = appList.OrderBy(a => a.AppName).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetInstalledApps error: {ex.Message}");
        }
#endif
        return appList;
    }

    private async void OnAppSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AppInfo selectedApp)
        {
            OnAppSelectedCallback?.Invoke(selectedApp.AppName, selectedApp.PackageName);
            await Navigation.PopAsync();
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            AppsCollectionView.ItemsSource = Apps;
        }
        else
        {
            var filtered = Apps.Where(a =>
                a.AppName.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase) ||
                a.PackageName.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase)).ToList();
            AppsCollectionView.ItemsSource = filtered;
        }
    }
}

public class AppInfo
{
    public string AppName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
}