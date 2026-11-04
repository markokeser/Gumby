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

                // Android 12+ traži BLUETOOTH_CONNECT dozvolu u toku rada
                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                {
                    var status = await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.Bluetooth>());
                    if (status != PermissionStatus.Granted)
                    {
                        await ShowAlert("Permission Denied", "Bluetooth permission is required.");
                        return;
                    }
                }

                var turnOn = enable ?? !adapter.IsEnabled;
                if (turnOn == adapter.IsEnabled)
                {
                    await ShowAlert("Bluetooth", $"Bluetooth is already {(turnOn ? "ON" : "OFF")}");
                    return;
                }

                // Android 13+: BluetoothAdapter.Enable()/Disable() više ne rade za obične aplikacije
                // (uvek vraćaju false), pa korisniku otvaramo sistemski dijalog / podešavanja.
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    var intent = turnOn
                        ? new Intent(BluetoothAdapter.ActionRequestEnable)
                        : new Intent(Android.Provider.Settings.ActionBluetoothSettings);
                    intent.SetFlags(ActivityFlags.NewTask);
                    context.StartActivity(intent);
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
