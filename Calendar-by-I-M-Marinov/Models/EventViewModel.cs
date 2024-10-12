using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Calendar_by_I_M_Marinov.Models
{
    public class EventViewModel
    {
        public string EventId { get; set; } = null!;
		public string CalendarId { get; set; } = null!;
		public string? RecurringEventId { get; set; }
		public string Summary { get; set; } = null!;

        public string? Description { get; set; }

        public string? Location { get; set; }

		[DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? Start { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? End { get; set; }

        public string Visibility { get; set; } 
        public string EventType { get; set; }  // Options: single, annual
        public bool IsAllDayEvent { get; set; }
        public List<string> Attendants { get; set; } = new();
        public string SendUpdates { get; set; } // Options: all, externalOnly, none 
        

		public List<SelectListItem> VisibilityOptions { get; set; }
        public List<SelectListItem> EventTypeOptions { get; set; }


	}

}
