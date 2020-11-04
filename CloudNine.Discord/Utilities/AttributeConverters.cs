using System;
using System.Globalization;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace CloudNine.Discord.Utilities
{
    public static class AttributeConverters
    {
        public class DateTimeAttributeConverter : IArgumentConverter<DateTime>
        {
            public Task<Optional<DateTime>> ConvertAsync(string value, CommandContext ctx)
            {
                var lowercase = value.ToLower().Trim();

                // Check to see if there is a splitable part...

                string[]? monthDayParts = null;

                // By seeing if there is a / or \ ...
                if (lowercase.Contains("/"))
                {
                    // And if there is, splitting the string by the / or \.
                    monthDayParts = lowercase.Split("/", StringSplitOptions.RemoveEmptyEntries);
                }
                else if (lowercase.Contains(@"\"))
                {
                    monthDayParts = lowercase.Split(@"\", StringSplitOptions.RemoveEmptyEntries);
                }

                if (!(monthDayParts is null))
                {
                    // Do conversion for MM/DD format.

                    // If the lenght of the parts is greater than 2 ...
                    if (monthDayParts.Length >= 2)
                    {
                        // ... get the month from the item at index of 0 ...
                        if (int.TryParse(monthDayParts[0], out int month))
                        {
                            // ... and verify the month is a valid month (from 1 to 12) ...

                            if (0 < month && month <= 12)
                            {
                                // ... then get the day from the item at index of 1 ...
                                if (int.TryParse(monthDayParts[1], out int day))
                                {
                                    // ... and verify the day is a valid day, depending on the month ...
                                    if (day.IsValidDayOfMonth(month))
                                    {
                                        // ... then create a DateTime from the month and day ...
                                        DateTime date = new DateTime(DateTime.Now.Year, month, day);
                                        // ... and return the result.
                                        return Task.FromResult(Optional.FromValue(date));
                                    }
                                }
                            }
                        }
                    }

                    // ... otherwise something went wrong, so return no value found.
                    return Task.FromResult(Optional.FromNoValue<DateTime>());
                }
                else
                {
                    // Do conversion for Month DD format.
                    if (DateTime.TryParseExact(lowercase, "MMMM dd", null, DateTimeStyles.AdjustToUniversal, out DateTime date))
                    {
                        return Task.FromResult(Optional.FromValue(date));
                    }
                    else if (DateTime.TryParseExact(lowercase, "dd MMMM", null, DateTimeStyles.AdjustToUniversal, out date))
                    {
                        return Task.FromResult(Optional.FromValue(date));
                    }
                    if (DateTime.TryParseExact(lowercase, "MMMM d", null, DateTimeStyles.AdjustToUniversal, out date))
                    {
                        return Task.FromResult(Optional.FromValue(date));
                    }
                    else if (DateTime.TryParseExact(lowercase, "d MMMM", null, DateTimeStyles.AdjustToUniversal, out date))
                    {
                        return Task.FromResult(Optional.FromValue(date));
                    }
                }

                return Task.FromResult(Optional.FromNoValue<DateTime>());
            }
        }

        public static bool IsValidDayOfMonth(this int day, int month)
        {
            return month switch
            {
                1 => 0 < day && day <= 31,
                2 => 0 < day && day <= 28,
                3 => 0 < day && day <= 31,
                4 => 0 < day && day <= 30,
                5 => 0 < day && day <= 31,
                6 => 0 < day && day <= 30,
                7 => 0 < day && day <= 31,
                8 => 0 < day && day <= 31,
                9 => 0 < day && day <= 30,
                10 => 0 < day && day <= 31,
                11 => 0 < day && day <= 30,
                12 => 0 < day && day <= 31,
                _ => false,
            };
        }
    }
}
