using System;
using System.Globalization;

namespace AT.Common.Extensions
{
    /// <summary>StringExtensions class.</summary>
    public static class StringExtensions
    {
        /// <summary>Converts string value to int.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The int value.</returns>
        public static int ConvertToInt(this string str)
        {
            int number = int.TryParse(str, out number) ? number : throw new FormatException($"The value '{str}' is not a valid integer number.");

            return number;
        }

        /// <summary>Converts string value to long.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The long value.</returns>
        public static long ConvertToLong(this string str)
        {
            long number = long.TryParse(str, out number) ? number : throw new FormatException($"The value '{str}' is not a valid long number.");

            return number;
        }

        /// <summary>Converts string value to decimal.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The decimal value.</returns>
        public static decimal ConvertToDecimal(this string str)
        {
            try
            {
                decimal number = Convert.ToDecimal(str, CultureInfo.InvariantCulture);

                return number;
            }
            catch
            {
                throw new FormatException($"The value '{str}' is not a valid decimal number.");
            }
        }

        /// <summary>Converts string value to boolean.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The boolean value. </returns>
        public static bool ConvertToBoolean(this string str)
        {
            return str.ToLower() switch
            {
                "1" or "yes" or "true" => true,
                "0" or "no" or "false" => false,
                _ => throw new FormatException($"The value '{str}' is not a valid boolean value."),
            };
        }
    }
}