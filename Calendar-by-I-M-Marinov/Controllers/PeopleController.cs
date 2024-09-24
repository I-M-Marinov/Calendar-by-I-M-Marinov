using Calendar_by_I_M_Marinov.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Calendar_by_I_M_Marinov.Models.People;


namespace Calendar_by_I_M_Marinov.Controllers
{
	public class PeopleController : Controller
	{
		private readonly IGooglePeopleService _peopleGoogleService;


		public PeopleController(IGooglePeopleService peopleGoogleService)
		{
			_peopleGoogleService = peopleGoogleService;
		}


		[HttpGet]
		[Route("contacts")]
		public async Task<IActionResult> GetAllContacts()
		{
			try
			{
				var contacts = await _peopleGoogleService.GetAllContactsAsync();

				if (contacts.Count == 0)
				{
					return NotFound("No contacts found.");
				}
				
				ViewBag.ContactsCount = contacts.Count;

				return View(contacts);
			}
			catch (System.Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}


		}

	}
}
