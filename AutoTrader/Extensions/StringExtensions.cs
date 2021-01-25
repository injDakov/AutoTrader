namespace AT.Worker.Extensions
{
    /// <summary>StringExtensions class.</summary>
    public static class StringExtensions
    {
        /// <summary>Converts string value to long.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The long value.</returns>
        public static long ConvertToLong(this string str)
        {
            long number = long.TryParse(str, out number) ? number : 0;

            return number;

            // add argument exception with param... to be active or not
        }

        /// <summary>Converts string value to int.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The int value.</returns>
        public static int ConvertToInt(this string str)
        {
            int number = int.TryParse(str, out number) ? number : 0;

            return number;
        }

        /// <summary>Converts string value to boolean.</summary>
        /// <param name="str">The string.</param>
        /// <returns>The boolean value. </returns>
        public static bool ConvertToBoolean(this string str)
        {
            switch (str.ToLower())
            {
                case "1":
                case "yes":
                case "true":
                    return true;

                default:
                    return false;
            }
        }
    }
}