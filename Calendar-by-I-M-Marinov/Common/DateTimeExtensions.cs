namespace Calendar_by_I_M_Marinov.Common
{
	public static class DateTimeExtensions
	{
		public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, string timeZoneId = "Europe/Sofia")
		{
			// Assume the dateTime is in UTC if no time zone is provided
			return new DateTimeOffset(dateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId).GetUtcOffset(dateTime));
		}

		public static DateTime ToDateTime(this DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.DateTime;
		}
	}
}
