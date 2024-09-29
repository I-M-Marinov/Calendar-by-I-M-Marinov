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
					.Where(c => !string.IsNullOrEmpty(c.FullName)) 
					.GroupBy(c => char.ToUpper(c.FullName[0])) 
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
                    .Where(c => !string.IsNullOrEmpty(c.FullName))
                    .GroupBy(c => char.ToUpper(c.FullName[0]))
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

        [HttpGet]
        [Route("/add-contact")]
        public async Task<ActionResult> AddContact()
        {
            var contactGroups = await _peopleGoogleService.GetContactGroupsAsync();
            ViewBag.ContactGroups = contactGroups; 

            return View();
        }
        [HttpPost]
        [Route("/add-contact")]
        public async Task<IActionResult> AddContact(ContactViewModel newContact, string selectedGroup)
        {
            newContact.Labels = newContact.Labels ?? new List<string>();

            var contactGroups = await _peopleGoogleService.GetContactGroupsAsync();
            var myContactsGroup = contactGroups.FirstOrDefault(g => g.Name == "myContacts");

            if (string.IsNullOrEmpty(selectedGroup) && myContactsGroup != null)
            {
                selectedGroup = myContactsGroup.ResourceName; 
            }

            if (string.IsNullOrEmpty(selectedGroup))
            {
                newContact.Labels.Add(myContactsGroup.ResourceName); 
            }
            else
            {
                newContact.Labels.Add(selectedGroup);
                newContact.Labels.Add(myContactsGroup.ResourceName);
            }

            
            if (string.IsNullOrEmpty(newContact.FirstName))
            {
                return BadRequest("Name is required.");
            }

            try
            {
                ViewBag.ContactGroups = contactGroups;
                ViewBag.NewContactFirstName = newContact.FirstName;
	            ViewBag.NewContactLastName = newContact.LastName;
	            ViewBag.NewContactEmail = string.IsNullOrWhiteSpace(newContact.Email) ? "N/A" : newContact.Email;
				ViewBag.NewContactPhoneNumber = string.IsNullOrWhiteSpace(newContact.PhoneNumber) ? "N/A" : newContact.PhoneNumber;
				ViewBag.NewContactBirthday = string.IsNullOrWhiteSpace(newContact.Birthday) ? "N/A" : newContact.Birthday;
				ViewBag.NewContactLabels = newContact.Labels;

                ViewBag.ShowSuccessMessage = true;

                string resourceId = await _peopleGoogleService.AddContactAsync(newContact, selectedGroup);
                // Ok($"Contact added successfully. Resource ID: {resourceId}");
                return View();


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

    }

}

