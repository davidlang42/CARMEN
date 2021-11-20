using System;

namespace Carmen.ShowModel.Import
{
    public class ParseException : Exception
    {
        public ParseException(string field_name, string invalid_value, string? expected_values = null)
            : base(BuildMessage(field_name, invalid_value, expected_values))
        { }

        private static string BuildMessage(string field, string value, string? expected)
        {
            var m = $"Invalid {field} '{value}'";
            if (expected != null)
                m += " expected " + expected;
            return m;
        }
    }
}
