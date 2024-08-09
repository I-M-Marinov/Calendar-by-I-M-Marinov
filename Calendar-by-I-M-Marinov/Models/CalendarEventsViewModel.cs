using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Models
{
    public class CalendarEventsViewModel
    {
        public IList<CalendarViewModel> Calendars { get; set; }
        public string SelectedCalendarId { get; set; }
        public IList<Event> Events { get; set; }

    }
}
