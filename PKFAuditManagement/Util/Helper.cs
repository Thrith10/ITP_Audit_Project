using System;

namespace PKFAuditManagement.Util
{
    public class Helper
    {
        // Method
        public static string GenerateQCFormFileReference()
        {
            const string prefix = "FREF";
            const int digitsLength = 6;
            const string digits = "0123456789";

            Random random = new Random();

            char[] result = new char[digitsLength];
            for (int i = 0; i < digitsLength; i++)
            {
                result[i] = digits[random.Next(digits.Length)];
            }

            return prefix + new string(result);
        }
    }
}
