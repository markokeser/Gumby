using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Bluetooth;
using Android.Content;
using Android.App;
#endif

namespace Gumby.Tasks
{
    public class BluetoothTaskService
    {
        public static async Task BluetoothOn()
        {
            Debug.WriteLine(">>> BluetoothTaskService: Turn Bluetooth ON");

#if ANDROID
            try
            {
                var adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                {
                    await ShowAlert("Error", "Bluetooth not supported on this device.");
                    return;
                }

                if (!adapter.IsEnabled)
                {
                    adapter.Enable();
                    await ShowAlert("Bluetooth", "Bluetooth turned ON");
                }
                else
                {
                    await ShowAlert("Bluetooth", "Bluetooth is already ON");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BluetoothOn error: {ex.Message}");
                await ShowAlert("Error", $"Failed: {ex.Message}");
            }
#endif
        }

        public static async Task BluetoothOff()
        {
            Debug.WriteLine(">>> BluetoothTaskService: Turn Bluetooth OFF");

#if ANDROID
            try
            {
                var adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                {
                    await ShowAlert("Error", "Bluetooth not supported on this device.");
                    return;
                }

                if (adapter.IsEnabled)
                {
                    adapter.Disable();
                    await ShowAlert("Bluetooth", "Bluetooth turned OFF");
                }
                else
                {
                    await ShowAlert("Bluetooth", "Bluetooth is already OFF");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BluetoothOff error: {ex.Message}");
                await ShowAlert("Error", $"Failed: {ex.Message}");
            }
#endif
        }

        public static async Task BluetoothToggle()
        {
            Debug.WriteLine(">>> BluetoothTaskService: Toggle Bluetooth");

#if ANDROID
            try
            {
                var adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                {
                    await ShowAlert("Error", "Bluetooth not supported on this device.");
                    return;
                }

                if (adapter.IsEnabled)
                {
                    adapter.Disable();
                    await ShowAlert("Bluetooth", "Bluetooth turned OFF");
                }
                else
                {
                    adapter.Enable();
                    await ShowAlert("Bluetooth", "Bluetooth turned ON");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BluetoothToggle error: {ex.Message}");
                await ShowAlert("Error", $"Failed: {ex.Message}");
            }
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
                case "2":
                    await BluetoothToggle();
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