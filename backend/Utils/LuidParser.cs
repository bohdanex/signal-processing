using System;
using System.Globalization;

namespace backend.Utils
{
    public static class LuidParser
    {
        /// <summary>
        /// Attempts to parse the 64-bit LUID value from a GPU performance counter instance name string.
        /// The format is expected to be "luid_0x<HighPart>_0x<LowPart>_phys_<Index>".
        /// </summary>
        /// <param name="instanceName">The full performance counter instance string.</param>
        /// <param name="luid">The resulting 64-bit LUID (long) if successful.</param>
        /// <returns>True if the LUID was successfully parsed; otherwise, false.</returns>
        public static bool TryParseLuid(string instanceName, out long luid)
        {
            luid = 0;
            if (string.IsNullOrWhiteSpace(instanceName)) return false;

            // Split the string by the underscore character
            var parts = instanceName.Split('_');

            // Check for the minimum expected parts and format prefix
            // We expect parts[1] and parts[2] to contain the 0x<hex> values
            if (parts.Length < 3 || parts[0] != "luid" || !parts[1].StartsWith("0x") || !parts[2].StartsWith("0x"))
            {
                return false;
            }

            try
            {
                // Convert the hexadecimal strings to unsigned 32-bit integers (uint)
                // We use NumberStyles.HexNumber and skip the "0x" prefix by using substring(2)
                uint highPart = uint.Parse(parts[1].Substring(2), NumberStyles.HexNumber);
                uint lowPart = uint.Parse(parts[2].Substring(2), NumberStyles.HexNumber);

                // Combine the high and low parts into a single 64-bit long (LUID)
                // The high part is shifted 32 bits left (<< 32)
                luid = ((long)highPart << 32) | lowPart;

                return true;
            }
            catch (FormatException)
            {
                // Parsing failed due to non-hex characters
                return false;
            }
            catch (OverflowException)
            {
                // Should not happen if using uint, but included for robustness
                return false;
            }
        }
    }
}