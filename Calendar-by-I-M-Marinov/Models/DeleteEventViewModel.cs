namespace Calendar_by_I_M_Marinov.Models
{
	public class DeleteEventViewModel
	{
		public string EventId { get; set; }
		public string CalendarId { get; set; }
		public string? RecurringEventId { get; set; }
		public bool DeleteSeries { get; set; } = false; // default to false
		public string Summary { get; set; }
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public string Location { get; set; }
        public bool IsAllDayEvent { get; set; } 


    }

}
