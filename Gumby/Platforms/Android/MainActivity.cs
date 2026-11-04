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
                var tagData = ReadTextRecord(intent);
                if (tagData == null)
                    return;

                // Komande se pokreću odmah i nastavljaju da rade u pozadini
                _ = ExecuteTagAsync(tagData);

                // Zatvori samo ako je aplikacija pokrenuta iz zatvorenog stanja
                if (_shouldFinishAfterRead)
                {
                    Finish();
                }
            }
        }

        /// <summary>
        /// Čita prvi NDEF Text zapis. Payload format (NFC Forum RTD Text):
        /// [status bajt][kod jezika][tekst], gde donjih 6 bitova status bajta
        /// predstavlja dužinu koda jezika, a bit 7 označava UTF-16 kodiranje.
        /// </summary>
        private static string? ReadTextRecord(Intent intent)
        {
            var rawMsgs = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            if (rawMsgs == null || rawMsgs.Length == 0)
                return null;

            if (rawMsgs[0] is not NdefMessage msg)
                return null;

            var records = msg.GetRecords();
            if (records == null || records.Length == 0)
                return null;

            var payload = records[0].GetPayload();
            if (payload == null || payload.Length == 0)
                return null;

            var status = payload[0];
            var languageLength = status & 0x3F;
            var textStart = 1 + languageLength;
            if (textStart > payload.Length)
                return null;

            var encoding = (status & 0x80) != 0
                ? System.Text.Encoding.BigEndianUnicode
                : System.Text.Encoding.UTF8;

            return encoding.GetString(payload, textStart, payload.Length - textStart);
        }

        private static async Task ExecuteTagAsync(string tagData)
        {
            try
            {
                await ChipService.HandleNfcTagDiscovered(tagData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing tag: {ex.Message}");
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
            // Preuzmi podatke odmah, da izmena statičkog stanja tokom upisa ne utiče na ovaj upis
            var textToWrite = PendingWriteText;
            var completion = WriteCompletionSource;
            ResetWriteMode();

            bool success = false;
            try
            {
                var nativeTag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Android.Nfc.Tag;
                if (nativeTag == null || string.IsNullOrEmpty(textToWrite))
                {
                    ShowToast("No tag detected", ToastLength.Short);
                    return;
                }

                var techList = nativeTag.GetTechList() ?? Array.Empty<string>();
                if (!techList.Contains("android.nfc.tech.Ndef") && !techList.Contains("android.nfc.tech.NdefFormatable"))
                {
                    ShowToast("Tag doesn't support NDEF format", ToastLength.Long);
                    return;
                }

                ShowToast("✍️ Writing to chip...", ToastLength.Short);

                // I/O sa čipom ide na pozadinskoj niti
                success = await Task.Run(() => WriteToTag(nativeTag, textToWrite));
            }
            catch (Exception ex)
            {
                ShowToast($"Write error: {ex.Message}", ToastLength.Short);
            }
            finally
            {
                completion?.TrySetResult(success);
            }
        }

        private bool WriteToTag(Android.Nfc.Tag nativeTag, string text)
        {
            var record = NdefRecord.CreateTextRecord("en", text);
            var message = new NdefMessage(new[] { record });
            var messageSize = message.ToByteArray()?.Length ?? 0;

            // Prazan čip: formatiraj i upiši
            var formatable = NdefFormatable.Get(nativeTag);
            if (formatable != null)
            {
                try
                {
                    formatable.Connect();
                    formatable.Format(message);
                    ShowToast("✅ Chip formatted and written!", ToastLength.Long);
                    return true;
                }
                catch (Exception formatEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Format failed, trying NDEF write: {formatEx.Message}");
                }
                finally
                {
                    try { formatable.Close(); } catch { }
                }
            }

            // Već formatiran čip
            var ndef = Ndef.Get(nativeTag);
            if (ndef == null)
            {
                ShowToast("Cannot write to this tag type", ToastLength.Long);
                return false;
            }

            try
            {
                ndef.Connect();

                if (!ndef.IsWritable)
                {
                    ShowToast("Tag is read-only", ToastLength.Short);
                    return false;
                }

                if (messageSize > ndef.MaxSize)
                {
                    ShowToast($"Data too large for this chip ({messageSize} / {ndef.MaxSize} bytes)", ToastLength.Long);
                    return false;
                }

                ndef.WriteNdefMessage(message);
                ShowToast("✅ Chip written successfully!", ToastLength.Long);
                return true;
            }
            catch (Exception ndefEx)
            {
                ShowToast($"Write error: {ndefEx.Message}", ToastLength.Short);
                return false;
            }
            finally
            {
                try { ndef.Close(); } catch { }
            }
        }

        private void ShowToast(string text, ToastLength length)
        {
            RunOnUiThread(() => Toast.MakeText(this, text, length)?.Show());
        }

        private void ResetWriteMode()
        {
            IsWriteModeActive = false;
            PendingWriteText = null;
        }

    }
}