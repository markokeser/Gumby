using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

#if ANDROID
using Android.Content;
using Android.Database;
using Android.Provider;
#endif

namespace Gumby.Views
{
    public partial class ContactPickerPage : ContentPage
    {
        public ObservableCollection<ContactInfo> Contacts { get; set; } = new();
        public Action<string, string>? OnContactSelected { get; set; }

        public ContactPickerPage()
        {
            InitializeComponent();
            ContactsView.ItemsSource = Contacts;
            ContactsView.SelectionChanged += OnContactSelectedFromList;
            SearchBar.TextChanged += OnSearchTextChanged;
            LoadContacts();
        }

        private async void LoadContacts()
        {
            var status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Permisija", "Nije dozvoljen pristup kontaktima.", "OK");
                return;
            }

            try
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                await Task.Run(() =>
                {
                    var contacts = GetContactsFromDevice();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var contact in contacts)
                        {
                            Contacts.Add(contact);
                        }

                        LoadingIndicator.IsVisible = false;
                        LoadingIndicator.IsRunning = false;
                        NoContactsLabel.IsVisible = Contacts.Count == 0;
                    });
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadContacts error: {ex.Message}");
                LoadingIndicator.IsVisible = false;
                await DisplayAlert("Error", "Failed to load contacts.", "OK");
            }
        }

        private List<ContactInfo> GetContactsFromDevice()
        {
            var contactList = new List<ContactInfo>();

#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var contentResolver = context.ContentResolver;
                var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

                string[] projection = {
                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName,
                    ContactsContract.CommonDataKinds.Phone.Number
                };

                var cursor = contentResolver.Query(uri, projection, null, null, null);

                if (cursor != null && cursor.Count > 0)
                {
                    int nameIndex = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName);
                    var numberIndex = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);

                    var processedNames = new HashSet<string>();

                    while (cursor.MoveToNext())
                    {
                        var name = cursor.GetString(nameIndex) ?? "Unknown";
                        var number = cursor.GetString(numberIndex) ?? "";

                        // Očisti broj telefona (ukloni razmake, crtice, zagrade)
                        number = System.Text.RegularExpressions.Regex.Replace(number, @"[^\d+]", "");

                        if (!string.IsNullOrEmpty(number) && !processedNames.Contains(name))
                        {
                            processedNames.Add(name);
                            contactList.Add(new ContactInfo { Name = name, Number = number });
                        }
                    }
                    cursor.Close();
                }

                contactList = contactList.OrderBy(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetContactsFromDevice error: {ex.Message}");
            }
#endif
            return contactList;
        }

        private async void OnContactSelectedFromList(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ContactInfo selectedContact)
            {
                OnContactSelected?.Invoke(selectedContact.Name, selectedContact.Number);
                await Navigation.PopAsync();
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                ContactsView.ItemsSource = Contacts;
            }
            else
            {
                var filtered = Contacts.Where(c =>
                    c.Name.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase) ||
                    c.Number.Contains(e.NewTextValue)).ToList();
                ContactsView.ItemsSource = filtered;
            }
        }
    }

    public class ContactInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
    }
}