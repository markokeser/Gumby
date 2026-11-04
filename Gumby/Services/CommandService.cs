using System;
using System.Collections.Generic;
using System.Text;

namespace Gumby.Services
{
    public class CommandService
    {
        //INSERT CONTACT PART
        public static string GenerateInsertContactCommand(string name, string phone, string email, string company)
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
            var commandData = $"8|{encodedData}";

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
            var commandData = $"2|{numberText}";

            return commandData;
        }

        //INSERT SMS PART
        public static string GenerateInsertSmsCommand(string phoneNumber, string message) {
            var commandData = "";

            if (string.IsNullOrEmpty(message))
            {
                commandData = $"3|{phoneNumber}|";
            }
            else
            {
                commandData = $"3|{phoneNumber}|{Uri.EscapeDataString(message)}";
            }

            return commandData;
        }

    }
}
