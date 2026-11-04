using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using Android.Widget;
using Android.Nfc.Tech;
using Gumby.Services;
using Gumby.Tasks;
using Gumby.Views;
using Plugin.NFC;

namespace Gumby
{
    [Activity(Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              LaunchMode = LaunchMode.SingleTop,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
          ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(new[] { NfcAdapter.ActionNdefDiscovered, NfcAdapter.ActionTagDiscovered },
                  Categories = new[] { Intent.CategoryDefault },
                  DataMimeType = "text/plain")]
    public class MainActivity : MauiAppCompatActivity
    {
        private bool _shouldFinishAfterRead = false;
        public static bool IsWriteModeActive { get; set; } = false;
        public static string? PendingWriteText { get; set; } = null;
        public static TaskCompletionSource<bool>? WriteCompletionSource { get; set; }

        private NfcAdapter? nfcAdapter;
        private PendingIntent? nfcPendingIntent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Aplikacija je pokrenuta iz zatvorenog stanja
            _shouldFinishAfterRead = true;

            // Initialize Plugin.NFC
            Plugin.NFC.CrossNFC.Init(this);

            // Setup NFC adapter
            nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
            nfcPendingIntent = PendingIntent.GetActivity(
                this, 0,
                new Intent(this, GetType()).AddFlags(ActivityFlags.SingleTop),
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable);

            ProcessTagIntent(this.Intent);
        }

        /*   public static void SetNfcListening(bool enabled)
           {
               if (enabled)
               {
                   CrossNFC.Current.StartListening();
               }
               else
               {
                   CrossNFC.Current.StopListening();
               }
           }  */

        private void ProcessTagIntent(Intent intent)
        {
            if (intent == null)
                return;

            // WRITE MODE
            if (IsWriteModeActive && !string.IsNullOrEmpty(PendingWriteText))
            {
                HandleWriteMode(intent);
                return;
            }

            // READ MODE
            if (intent.Action == NfcAdapter.ActionNdefDiscovered)
            {
                var rawMsgs = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
                if (rawMsgs != null && rawMsgs.Length > 0)
                {
                    var msg = (NdefMessage)rawMsgs[0];
                    if (msg.GetRecords().Length > 0)
                    {
                        var record = msg.GetRecords()[0];
                        var payload = record.GetPayload();
                        var tagData = System.Text.Encoding.UTF8.GetString(payload, 3, payload.Length - 3);
                        ChipService.HandleNfcTagDiscovered(tagData);

                        // Zatvori samo ako je aplikacija pokrenuta iz zatvorenog stanja
                        if (_shouldFinishAfterRead)
                        {
                            Finish();
                        }
                    }
                }
            }
            else if (intent.Action == NfcAdapter.ActionTagDiscovered)
            {
                return;
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Aplikacija je već bila otvorena (samo je dobila novi intent)
            _shouldFinishAfterRead = false;

            this.Intent = intent;
            ProcessTagIntent(intent);
            Plugin.NFC.CrossNFC.OnNewIntent(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Plugin.NFC.CrossNFC.OnResume();

            // Enable foreground dispatch to intercept NFC tags BEFORE Android system handler
            if (nfcAdapter != null && nfcPendingIntent != null)
            {
                try
                {
                    // null, null = intercept ALL tags (both empty and with data)
                    nfcAdapter.EnableForegroundDispatch(this, nfcPendingIntent, null, null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error enabling foreground dispatch: {ex.Message}");
                }
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Disable foreground dispatch when app is not in foreground
            if (nfcAdapter != null)
            {
                try
                {
                    nfcAdapter.DisableForegroundDispatch(this);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disabling foreground dispatch: {ex.Message}");
                }
            }
        }

        private async void HandleWriteMode(Intent intent)
        {
            try
            {
                // Get the native Android tag from intent
                var nativeTag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Android.Nfc.Tag;

                if (nativeTag == null)
                {
                    RunOnUiThread(() => Toast.MakeText(this, "No tag detected", ToastLength.Short).Show());
                    WriteCompletionSource?.TrySetResult(false);
                    ResetWriteMode();
                    return;
                }

                // Check what technologies the tag supports
                var techList = nativeTag.GetTechList();
                bool supportsNdef = false;
                foreach (var tech in techList)
                {
                    if (tech == "android.nfc.tech.Ndef" || tech == "android.nfc.tech.NdefFormatable")
                    {
                        supportsNdef = true;
                        break;
                    }
                }

                if (!supportsNdef)
                {
                    RunOnUiThread(() => Toast.MakeText(this, "Tag doesn't support NDEF format", ToastLength.Long).Show());
                    WriteCompletionSource?.TrySetResult(false);
                    ResetWriteMode();
                    return;
                }

                // IMPORTANT: Run the write operation on a background thread
                await Task.Run(() =>
                {
                    try
                    {
                        RunOnUiThread(() => Toast.MakeText(this, "✍️ Writing to chip...", ToastLength.Short).Show());

                        // ✅ KEY FIX: Try NdefFormatable FIRST for empty tags
                        var formatable = Android.Nfc.Tech.NdefFormatable.Get(nativeTag);
                        if (formatable != null)
                        {
                            try
                            {
                                formatable.Connect();
                                var record = Android.Nfc.NdefRecord.CreateTextRecord("en", PendingWriteText);
                                var message = new Android.Nfc.NdefMessage(new[] { record });
                                formatable.Format(message);
                                formatable.Close();

                                RunOnUiThread(() => Toast.MakeText(this, "✅ Chip formatted and written!", ToastLength.Long).Show());
                                WriteCompletionSource?.TrySetResult(true);
                                return;
                            }
                            catch (Exception formatEx)
                            {
                                RunOnUiThread(() => Toast.MakeText(this, $"Format error: {formatEx.Message}", ToastLength.Short).Show());
                                try { formatable.Close(); } catch { }
                            }
                        }

                        // If not formatable or formatting failed, try Ndef for already-formatted tags
                        var ndef = Android.Nfc.Tech.Ndef.Get(nativeTag);
                        if (ndef != null)
                        {
                            try
                            {
                                ndef.Connect();

                                // Check if writable
                                if (!ndef.IsWritable)
                                {
                                    RunOnUiThread(() => Toast.MakeText(this, "Tag is read-only", ToastLength.Short).Show());
                                    WriteCompletionSource?.TrySetResult(false);
                                    ndef.Close();
                                    return;
                                }

                                // Create NDEF record
                                var record = Android.Nfc.NdefRecord.CreateTextRecord("en", PendingWriteText);
                                var message = new Android.Nfc.NdefMessage(new[] { record });

                                // Write to the chip
                                ndef.WriteNdefMessage(message);
                                ndef.Close();

                                // Success
                                RunOnUiThread(() => Toast.MakeText(this, "✅ Chip written successfully!", ToastLength.Long).Show());
                                WriteCompletionSource?.TrySetResult(true);
                            }
                            catch (Exception ndefEx)
                            {
                                RunOnUiThread(() => Toast.MakeText(this, $"Write error: {ndefEx.Message}", ToastLength.Short).Show());
                                WriteCompletionSource?.TrySetResult(false);
                                try { ndef.Close(); } catch { }
                            }
                        }
                        else
                        {
                            RunOnUiThread(() => Toast.MakeText(this, "Cannot write to this tag type", ToastLength.Long).Show());
                            WriteCompletionSource?.TrySetResult(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        RunOnUiThread(() => Toast.MakeText(this, $"Write error: {ex.Message}", ToastLength.Short).Show());
                        WriteCompletionSource?.TrySetResult(false);
                    }
                });
            }
            catch (Exception ex)
            {
                WriteCompletionSource?.TrySetResult(false);
            }
            finally
            {
                ResetWriteMode();
            }
        }

        private void ResetWriteMode()
        {
            IsWriteModeActive = false;
            PendingWriteText = null;
        }

    }
}