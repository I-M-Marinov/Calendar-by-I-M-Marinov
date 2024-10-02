using System.ComponentModel.DataAnnotations;

namespace Calendar_by_I_M_Marinov.Models.People
{
	public class ContactViewModel
	{
		public string ResourceName { get; set; } = null!; 

		[Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } = null!;

		[Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; } = null!;
		public string FullName => $"{FirstName} {LastName}";

		[EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }  // Optional

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }  // Optional

        public string? Birthday { get; set; }  // Optional

        public List<string> Labels { get; set; } = new List<string>(); 
	}

}
