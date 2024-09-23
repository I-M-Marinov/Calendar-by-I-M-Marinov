namespace Calendar_by_I_M_Marinov.Models
{
   public class UpdatedEventViewModel
    {
        public string EventId { get; set; }
        public string CalendarId { get; set; }
        public string Summary { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        public bool IsUpdated { get; set; } 
        public string ErrorMessage { get; set; } // Holds error message if any
    }

}
 