using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertClock : ContentPage
    {
        public InsertClock()
        {
            InitializeComponent();
        }

        private async void OnAlarmClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertSetAlarm());
        }

        private async void OnTimerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertStartTimer());
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}