using System;
using System.Collections.Generic;
using System.Text;

namespace Gumby.Services
{
    public class CommandService
    {
        //INSERT CONTACT PART
        public static string? GenerateInsertContactCommand(string name, string phone, string email, string company)
        {
            if (string.IsNullOrEmpty(name))
            {
                //    await DisplayAlert("Error", "Please enter at least a name.", "OK");
                return null;
            }

            // Create vCard format (standard contact format)
            var vcard = GenerateVCard(name, phone, email, company);

            // Encode for NFC
            var encodedData = Uri.EscapeDataString(vcard);
            var commandData = $"{CommandCodes.Contact}|{encodedData}";

            return commandData;
        }

        private static string GenerateVCard(string name, string phone, string email, string company)
        {
            // Simple vCard version 3.0 format
            var vcard = $"BEGIN:VCARD\nVERSION:3.0\nFN:{name}\n";

            if (!string.IsNullOrEmpty(phone))
                vcard += $"TEL:{phone}\n";

            if (!string.IsNullOrEmpty(email))
                vcard += $"EMAIL:{email}\n";

            if (!string.IsNullOrEmpty(company))
                vcard += $"ORG:{company}\n";

            vcard += "END:VCARD";

            return vcard;
        }

        //INSERT CALL PART
        public static string GenerateInsertCallCommand(string numberText)
        {
            var commandData = $"{CommandCodes.Call}|{numberText}";

            return commandData;
        }

        //INSERT SMS PART
        // Format: 3|<telefon>|<poruka>, oba dela URL-enkodovana (ChipService ih dekoduje)
        public static string GenerateInsertSmsCommand(string phoneNumber, string message)
        {
            var encodedPhone = Uri.EscapeDataString(phoneNumber ?? "");
            var encodedMessage = string.IsNullOrEmpty(message) ? "" : Uri.EscapeDataString(message);

            return $"{CommandCodes.Sms}|{encodedPhone}|{encodedMessage}";
        }

    }
}
