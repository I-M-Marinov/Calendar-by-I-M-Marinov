namespace Calendar_by_I_M_Marinov.Models.Calendar
{
    public class SearchEventViewModel
    {
        public string EventName { get; set; } = null!;
        public string CalendarId { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string? Location { get; set; }

        // New properties to verify the bool isEditable
        public bool IsCreator { get; set; }
        public bool GuestsCanModify { get; set; }
        public string Status { get; set; } = null!;

        public bool IsEditable => (IsCreator || GuestsCanModify) && Status != "cancelled";

    }

}