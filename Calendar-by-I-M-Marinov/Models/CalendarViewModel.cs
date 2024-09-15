﻿using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Models
{
	public class CalendarViewModel
	{
		public string CalendarId { get; set; } = null!;
        public string CalendarName { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string AccessRole { get; set; } = null!;
		public int EventsCount { get; set; }
		public bool? Primary { get; set; }

		// List of events associated with this calendar
		public List<Event> Events { get; set; } = new List<Event>();
    }
}
