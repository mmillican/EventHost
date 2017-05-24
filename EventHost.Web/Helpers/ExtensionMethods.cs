using System;
using System.Text.RegularExpressions;
using EventHost.Web.Entities.Events;
using EventHost.Web.Models.Events;

namespace EventHost.Web.Helpers
{
    public static class ExtensionMethods
    {
        public static string Clean(this string input)
        {
            var regex = new Regex("[^a-z0-9\\-_]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            input = input.Replace(" ", "-");
            var cleaned = regex.Replace(input, "").ToLower();

            while (cleaned.Contains("--"))
            {
                cleaned = cleaned.Replace("--", "-");
            }

            return cleaned;
        }

        public static bool IsEventRegistrationOpen(this Event evt, DateTime testDate)
        {
            return IsDateInRange(evt.RegistrationStartOn, evt.RegistrationEndOn, testDate);
        }

        public static bool IsEventRegistrationOpen(this EventModel evt, DateTime testDate)
        {
            return IsDateInRange(evt.RegistrationStartOn, evt.RegistrationEndOn, testDate);
        }

        public static bool IsDateInRange(DateTime? start, DateTime? end, DateTime test)
        {
            return (!start.HasValue || start.Value <= test) && (!end.HasValue || end.Value > test);
        }
    }
}
