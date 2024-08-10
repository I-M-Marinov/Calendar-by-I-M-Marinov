namespace Calendar_by_I_M_Marinov.Models
{
    public class EditEventViewModel
    {
        public string EventId { get; set; } = null!;
        public string CalendarId { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? Location { get; set; }
    }

}
