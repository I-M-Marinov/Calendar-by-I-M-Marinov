using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Models
{
	public class CalendarEventsViewModel
	{
		public ICollection<CalendarViewModel> Calendars { get; set; } = new HashSet<CalendarViewModel>();
		public string SelectedCalendarId { get; set; } = null!;
        public string SelectedCalendarName { get; set; } = null!;
        public string AccessRole { get; set; } = null!;
        public ICollection<Event>? Events { get; set; } = new HashSet<Event>();
    }
}
