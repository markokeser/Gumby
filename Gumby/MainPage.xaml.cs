using Gumby.Views;

namespace Gumby
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
        }

        /*    private async void OnOpenPlaylistClicked(object sender, EventArgs e)
            {
            //    await Navigation.PushAsync(new PlaylistPage());
            }   */

        private async void OnInsertRefsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertRefs());
        }

        private async void OnVolumeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertRingtone());
        }

        private async void OnInsertPhoneClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertPhone());
        }

        private async void OnInsertClockClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertClock());
        }

        private async void OnInsertConnectionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertConnection());
        }

        private async void OnOpenAppClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertOpenApp());
        }

        private async void OnAlarmClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertSetAlarm());
        }

        private async void OnTimerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertStartTimer());
        }

        private async void OnInsertWifiClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertWifiConnection());
        }

        private async void OnInsertContactClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertAddContact());
        }

        private async void OnInsertPlaylistClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertPlaylist());
        }

        private async void OnInsertMultiCommandClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MultiCommandBuilder());
        }

        private async void OnInsertBluetoothClicked(object sender, EventArgs e)
        {
            
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
