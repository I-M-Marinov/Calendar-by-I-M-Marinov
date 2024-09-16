using System.Runtime.CompilerServices;
using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Models
{
	public class CalendarViewModel
	{
		public string CalendarId { get; set; } 
        public string CalendarName { get; set; } 
		public string? Description { get; set; } 
		public string? AccessRole { get; set; } 
		public int? EventsCount { get; set; }
		public bool? Primary { get; set; }

        // List of events associated with this calendar
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
