using System.ComponentModel.DataAnnotations;

namespace Calendar_by_I_M_Marinov.Models
{
    public class EventViewModel
    {
        public string EventId { get; set; }
        public string Summary { get; set; } = null!;

        public string? Description { get; set; }

        public string? Location { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? Start { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? End { get; set; }

        public string Visibility { get; set; } = "public"; // Default to public
        public string EventType { get; set; } = "single"; // Options: single, annual, allDay // Default to single
        public List<string> Attendants { get; set; } = new();

    }

}
