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

				return View(groupedContacts);
			}
			catch (System.Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex.Message}");
			}


		}
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                var contacts = await _peopleGoogleService.GetAllContactsAsync();

                var groupedContacts = contacts
                    .Where(c => c.Labels != null && c.Labels.Count > 0) 
                    .GroupBy(c => c.Labels.First()) 
                    .OrderBy(g => g.Key) 
                    .ToList();

                ViewBag.ContactsCount = contacts.Count; 
                ViewBag.ContactGroups = await _peopleGoogleService.GetContactGroupsAsync(); 

                return View(groupedContacts); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}"); 
            }
        }

        [HttpPost]
        public async Task<ActionResult> Index(string selectedGroup)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedGroup))
                {
                    ModelState.AddModelError("", "Please select a contact group.");
                    ViewBag.ContactGroups = await _peopleGoogleService.GetContactGroupsAsync();
                    return View(new List<IGrouping<char, Person>>());
                }

                var contactGroup = await _peopleGoogleService.GetContactGroupAsync(selectedGroup);

                var contacts = new List<Person>();

                if (contactGroup.MemberResourceNames != null && contactGroup.MemberResourceNames.Count > 0)
                {
                    foreach (var memberResourceName in contactGroup.MemberResourceNames)
                    {
                        var person = await _peopleGoogleService.GetPersonAsync(memberResourceName);
                        if (person != null)
                        {
                            contacts.Add(person);
                        }
                    }
                }

                var groupedContacts = contacts
                    .Where(c => c.Names != null && c.Names.Count > 0) 
                    .GroupBy(c => char.ToUpper(c.Names[0].DisplayName[0])) 
                    .OrderBy(g => g.Key) 
                    .ToList();

                ViewBag.ContactsCount = contacts.Count; 
                ViewBag.ContactGroups = await _peopleGoogleService.GetContactGroupsAsync(); 

                return View(groupedContacts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





    }

}

