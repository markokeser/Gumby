using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertRefs : ContentPage
    {
        public InsertRefs()
        {
            InitializeComponent();
        }

        private async void OnLinkClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertLink());
        }

        private async void OnOpenAppClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertOpenApp());
        }

        private async void OnInstagramClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertInstagram());
        }

        private async void OnGoogleMapsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertGoogleMaps());
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}