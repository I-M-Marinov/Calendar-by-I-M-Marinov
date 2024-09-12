using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Models
{
	public class CalendarEventsViewModel
	{
		public List<CalendarViewModel> Calendars { get; set; } = new List<CalendarViewModel>();
		public string SelectedCalendarId { get; set; } = null!;
        public string SelectedCalendarName { get; set; } = null!;
        public string AccessRole { get; set; } = null!;
        public List<Event>? Events { get; set; } = new List<Event>();
		public int EventsCount { get; set; }
		public Dictionary<string, string> EventCalendarMap { get; set; } = new Dictionary<string, string>(); // EventId to CalendarId map

    }
}
