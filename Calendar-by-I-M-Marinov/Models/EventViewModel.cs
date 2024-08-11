using System.ComponentModel.DataAnnotations;

namespace Calendar_by_I_M_Marinov.Models
{
    public class EventViewModel
    {
        public string Summary { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; } = DateTime.Now.ToLocalTime();

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime End { get; set; } = DateTime.Now.ToLocalTime();
    }

}
