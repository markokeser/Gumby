using System;
using System.Diagnostics;
using Debug = System.Diagnostics.Debug;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Bluetooth;
using Android.Content;
using Android.OS;
#endif

namespace Gumby.Tasks
{
    public class BluetoothTaskService
    {
        public static Task BluetoothOn() => SetBluetoothState(true);

        public static Task BluetoothOff() => SetBluetoothState(false);

        public static Task BluetoothToggle() => SetBluetoothState(null);

        /// <param name="enable">true = uključi, false = isključi, null = promeni stanje</param>
        private static async Task SetBluetoothState(bool? enable)
        {
            Debug.WriteLine($">>> BluetoothTaskService: SetBluetoothState({enable?.ToString() ?? "toggle"})");

#if ANDROID
            try
            {
                var context = Android.App.Application.Context;
                var manager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
                var adapter = manager?.Adapter;
                if (adapter == null)
                {
                    await ShowAlert("Error", "Bluetooth not supported on this device.");
                    return;
                }

                var turnOn = enable ?? !adapter.IsEnabled;
                if (turnOn == adapter.IsEnabled)
                {
                    await ShowAlert("Bluetooth", $"Bluetooth is already {(turnOn ? "ON" : "OFF")}");
                    return;
                }

                // Android 12+ traži BLUETOOTH_CONNECT dozvolu. Ne tražimo je ovde jer se čip
                // čita u pozadini - traži se kada korisnik pravi Bluetooth komandu.
                // Bez dozvole otvaramo Bluetooth podešavanja.
                if (Build.VERSION.SdkInt >= BuildVersionCodes.S &&
                    await Permissions.CheckStatusAsync<Permissions.Bluetooth>() != PermissionStatus.Granted)
                {
                    OpenBluetoothSettings(context);
                    return;
                }

                // Android 13+: BluetoothAdapter.Enable()/Disable() više ne rade za obične aplikacije
                // (uvek vraćaju false), pa korisniku otvaramo sistemski dijalog / podešavanja.
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (turnOn)
                    {
                        var intent = new Intent(BluetoothAdapter.ActionRequestEnable);
                        intent.SetFlags(ActivityFlags.NewTask);
                        context.StartActivity(intent);
                    }
                    else
                    {
                        OpenBluetoothSettings(context);
                    }
                    return;
                }

#pragma warning disable CA1422 // Zastarelo od API 33, ali radi na starijim verzijama
                var changed = turnOn ? adapter.Enable() : adapter.Disable();
#pragma warning restore CA1422
                await ShowAlert("Bluetooth", changed
                    ? $"Bluetooth turned {(turnOn ? "ON" : "OFF")}"
                    : "Failed to change Bluetooth state");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Bluetooth error: {ex.Message}");
                await ShowAlert("Error", $"Failed: {ex.Message}");
            }
#else
            await Task.CompletedTask;
#endif
        }

#if ANDROID
        private static void OpenBluetoothSettings(Context context)
        {
            var intent = new Intent(Android.Provider.Settings.ActionBluetoothSettings);
            intent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
#endif

        /// <summary>
        /// Traži Bluetooth dozvolu (Android 12+). Poziva se iz aplikacije, kada korisnik
        /// pravi Bluetooth komandu, da bi kasnije radila i kada se čip čita u pozadini.
        /// </summary>
        public static async Task EnsurePermissionAsync()
        {
#if ANDROID
            if (Build.VERSION.SdkInt < BuildVersionCodes.S)
                return;

            try
            {
                if (await Permissions.CheckStatusAsync<Permissions.Bluetooth>() != PermissionStatus.Granted)
                    await Permissions.RequestAsync<Permissions.Bluetooth>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Bluetooth permission error: {ex.Message}");
            }
#else
            await Task.CompletedTask;
#endif
        }

        public static async Task HandleCommand(string value)
        {
            var parts = value.Split('|');
            var action = parts.Length > 0 ? parts[0] : "";

            switch (action)
            {
                case "0":
                    await BluetoothOff();
                    break;
                case "1":
                    await BluetoothOn();
                    break;
                default:
                    await BluetoothToggle();
                    break;
            }
        }

        private static async Task ShowAlert(string title, string message)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Microsoft.Maui.Controls.Application.Current?.MainPage != null)
                {
       //             await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
    }
}
