using Google.Apis.PeopleService.v1.Data;

namespace Calendar_by_I_M_Marinov.Common
{
	public static class DateTimeExtensions
	{
        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, string timeZoneId = "Europe/Sofia")
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return new DateTimeOffset(dateTime, timeZoneInfo.GetUtcOffset(dateTime));
        }

        public static DateTime ToDateTime(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.DateTime;
        }

        public static DateTimeOffset ConvertToLocalTime(this DateTimeOffset dateTimeOffset, string timeZoneId)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
        }

        public static DateTime ToLocalTime(this DateTime dateTime)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Europe/Sofia"); 
            return TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
        }

        public static List<Birthday> ParseBirthday(string birthday)
        {
	        if (DateTime.TryParse(birthday, out DateTime parsedDate))
	        {
		        return new List<Birthday>
		        {
			        new Birthday
			        {
				        Date = new Date
				        {
					        Day = parsedDate.Day,
					        Month = parsedDate.Month,
					        Year = parsedDate.Year
				        }
			        }
		        };
	        }

	        return null; 
        }
	}
}
