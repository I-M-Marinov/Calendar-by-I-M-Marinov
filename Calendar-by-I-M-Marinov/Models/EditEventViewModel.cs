using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Models
{
    public class EditEventViewModel
    {
        public string EventId { get; set; } = null!;
        public string CalendarId { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public string? Description { get; set; }
		public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? Location { get; set; }
        
        // New properties to verify the bool isEditable
        public bool IsCreator { get; set; }
        public bool GuestsCanModify { get; set; }
        public string Status { get; set; }

        // New properties to assess editability
        public string? Source { get; set; } // Event source (e.g., Gmail, Reservation)
        public bool? Locked { get; set; }   // Indicates if the event is locked
        public string? Transparency { get; set; } // Event transparency (e.g., "opaque", "transparent")

        public string Visibility { get; set; } = "public"; // Default to public ( Options: public, private )
        public string EventType { get; set; } = "single"; //  Default to single ( Options: single, annual, allDay )
        public Dictionary<string, string> EventCalendarMap { get; set; } = new Dictionary<string, string>(); // EventId to CalendarId map



        // Enhanced logic for determining if the event is editable
        public bool IsEditable =>
            (IsCreator || GuestsCanModify) &&
            Status != "cancelled" &&
            (Locked != true) &&
            (Source?.ToLower() != "gmail");

    }

}
