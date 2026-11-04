using System;
using Gumby.Views;

namespace Gumby.Views
{
    public partial class InsertPhone : ContentPage
    {
        // Callback for MultiCommandBuilder
        public Action<string, string>? OnPhoneCommandSaved { get; set; }

        private string _currentAction = "";

        public InsertPhone()
        {
            InitializeComponent();
        }

        private async void OnCallClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertCall());
        }

        private async void OnSmsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertMessage());
        }

        private async void OnSmsWithTextClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertMessage());
        }

        private async void OnAddContactClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InsertAddContact());
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}