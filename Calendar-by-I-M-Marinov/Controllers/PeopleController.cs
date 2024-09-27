using Calendar_by_I_M_Marinov.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Calendar_by_I_M_Marinov.Models.People;
using Google.Apis.PeopleService.v1.Data;


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

				var groupedContacts = contacts
					.Where(c => !string.IsNullOrEmpty(c.Name)) 
					.GroupBy(c => char.ToUpper(c.Name[0])) 
					.OrderBy(g => g.Key) 
					.ToList();

				if (contacts.Count == 0)
				{
					return NotFound("No contacts found.");
				}
				
				ViewBag.ContactsCount = contacts.Count;

                var contactGroupList = await _peopleGoogleService.GetContactGroupsAsync();

                if (contactGroupList == null || !contactGroupList.Any())
                {
                    return StatusCode(500, "No contact groups returned from the service.");
                }

                ViewBag.ContactGroups = contactGroupList;
                ViewBag.ContactsCount = contacts.Count;

                return View(groupedContacts);
			}
			catch (System.Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}


		}
        [HttpGet]
        [Route("contacts/group")] 
        public async Task<ActionResult> GetContactsFromAGroup(string selectedGroup)
        {
            try
            {
                var contacts = await _peopleGoogleService.GetAllContactsAsync(selectedGroup);

                var groupedContacts = contacts
                    .Where(c => !string.IsNullOrEmpty(c.Name))
                    .GroupBy(c => char.ToUpper(c.Name[0]))
                    .OrderBy(g => g.Key)
                    .ToList();

                var contactGroupList = await _peopleGoogleService.GetContactGroupsAsync();
                if (contactGroupList == null || !contactGroupList.Any())
                {
                    return StatusCode(500, "No contact groups returned from the service.");
                }

                ViewBag.ContactGroups = contactGroupList; // Set groups in ViewBag
                ViewBag.ContactsCount = contacts.Count; // Total contacts count
                ViewBag.ContactGroupSelected = selectedGroup; // Save the name of the group to use it in the next View


				return View(groupedContacts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }

}

