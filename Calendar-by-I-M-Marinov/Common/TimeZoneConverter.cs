
namespace Calendar_by_I_M_Marinov.Common
{
    public static class TimeZoneConverter
    {
        // Full mapping of Windows time zones to IANA time zones
        private static readonly Dictionary<string, string> TimeZoneMap = new Dictionary<string, string>
        {
            // Standard time zones
            { "Dateline Standard Time", "Pacific/Kwajalein" },
            { "UTC-11", "Pacific/Pago_Pago" },
            { "Hawaiian Standard Time", "Pacific/Honolulu" },
            { "Alaskan Standard Time", "America/Anchorage" },
            { "Pacific Standard Time (Mexico)", "America/Tijuana" },
            { "Pacific Standard Time", "America/Los_Angeles" },
            { "US Mountain Standard Time", "America/Denver" },
            { "Mountain Standard Time (Mexico)", "America/Chihuahua" },
            { "Mountain Standard Time", "America/Denver" },
            { "Central America Standard Time", "America/Guatemala" },
            { "Central Standard Time", "America/Chicago" },
            { "Central Standard Time (Mexico)", "America/Mexico_City" },
            { "Eastern Standard Time", "America/New_York" },
            { "US Eastern Standard Time", "America/New_York" },
            { "Venezuela Standard Time", "America/Caracas" },
            { "Paraguay Standard Time", "America/Asuncion" },
            { "Atlantic Standard Time", "America/Halifax" },
            { "Central Brazilian Standard Time", "America/Cuiaba" },
            { "E. South America Standard Time", "America/Sao_Paulo" },
            { "Argentina Standard Time", "America/Argentina/Buenos_Aires" },
            { "Greenland Standard Time", "America/Godthab" },
            { "Cape Verde Standard Time", "Atlantic/Cape_Verde" },
            { "Azores Standard Time", "Atlantic/Azores" },
            { "UTC", "Etc/UTC" },
            { "Monrovia Standard Time", "Africa/Monrovia" },
            { "GMT Standard Time", "Europe/London" },
            { "Greenwich Standard Time", "Europe/London" },
            { "Central European Standard Time", "Europe/Berlin" },
            { "Romance Standard Time", "Europe/Paris" },
            { "Eastern European Standard Time", "Europe/Bucharest" },
            { "Jordan Standard Time", "Asia/Amman" },
            { "Middle East Standard Time", "Asia/Beirut" },
            { "Egypt Standard Time", "Africa/Cairo" },
            { "South Africa Standard Time", "Africa/Johannesburg" },
            { "FLE Standard Time", "Europe/Sofia" },
            { "Israel Standard Time", "Asia/Jerusalem" },
            { "Kaliningrad Standard Time", "Europe/Kaliningrad" },
            { "Arabian Standard Time", "Asia/Dubai" },
            { "Turkey Standard Time", "Europe/Istanbul" },
            { "Arab Standard Time", "Asia/Riyadh" },
            { "Iran Standard Time", "Asia/Tehran" },
            { "Astrakhan Standard Time", "Europe/Astrakhan" },
            { "UTC+03", "Asia/Baghdad" },
            { "Central Asia Standard Time", "Asia/Almaty" },
            { "Sri Lanka Standard Time", "Asia/Colombo" },
            { "Nepal Standard Time", "Asia/Kathmandu" },
            { "Bangladesh Standard Time", "Asia/Dhaka" },
            { "Omsk Standard Time", "Asia/Omsk" },
            { "China Standard Time", "Asia/Shanghai" },
            { "Singapore Standard Time", "Asia/Singapore" },
            { "W. Australia Standard Time", "Australia/Perth" },
            { "Krasnoyarsk Standard Time", "Asia/Krasnoyarsk" },
            { "North Korea Standard Time", "Asia/Pyongyang" },
            { "Tokyo Standard Time", "Asia/Tokyo" },
            { "Korea Standard Time", "Asia/Seoul" },
            { "Cen. Australia Standard Time", "Australia/Adelaide" },
            { "AUS Central Standard Time", "Australia/Adelaide" },
            { "E. Australia Standard Time", "Australia/Sydney" },
            { "Lord Howe Standard Time", "Australia/Lord_Howe" },
            { "New Zealand Standard Time", "Pacific/Auckland" },
            { "Fiji Standard Time", "Pacific/Fiji" },
            { "Tonga Standard Time", "Pacific/Tongatapu" },
            { "Chatham Islands Standard Time", "Pacific/Chatham" }
        };

        static TimeZoneConverter()
        {
            try
            {
                // Dictionary already initialized
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing TimeZoneConverter: {ex.Message}"); // for easier debugging
                throw; // Re-throw the exception
            }
        }

        public static string ConvertToIanaTimeZone(string windowsTimeZone)
        {
            try
            {
                if (TimeZoneMap.TryGetValue(windowsTimeZone, out var ianaTimeZone))
                {
                    return ianaTimeZone;
                }
                else
                {
                    Console.WriteLine($"Unsupported time zone: {windowsTimeZone}"); // for easier debugging
                    throw new ArgumentException($"Unsupported time zone: {windowsTimeZone}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions that may occur in the conversion method
                Console.WriteLine($"Error converting time zone: {ex.Message}");  // for easier debugging
                throw;
            }
        }
    }
}
