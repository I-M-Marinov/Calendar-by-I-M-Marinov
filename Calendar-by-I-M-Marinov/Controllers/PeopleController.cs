﻿using Calendar_by_I_M_Marinov.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Calendar_by_I_M_Marinov.Models.People;
using Google.Apis.PeopleService.v1.Data;
using Calendar_by_I_M_Marinov.Models.Calendar;
using Newtonsoft.Json;


namespace Calendar_by_I_M_Marinov.Controllers
{
	public class PeopleController : Controller
	{
		private readonly IGooglePeopleService _peopleGoogleService;
		private readonly IGoogleCalendarService _calendarService;


		public PeopleController(IGooglePeopleService peopleGoogleService, IGoogleCalendarService calendarService)
		{
			_peopleGoogleService = peopleGoogleService;
			_calendarService = calendarService;
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
				
                var contactGroupList = await _peopleGoogleService.GetContactGroupsAsync();

				if (contactGroupList == null || !contactGroupList.Any())
                {
                    return StatusCode(500, "No contact groups returned from the service.");
                }

                ViewBag.ContactGroups = contactGroupList;
                ViewBag.ContactsCount = contacts.Count;

                return View(groupedContacts);
			}
			catch (Exception ex)
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
	                TempData["ErrorMessage"] = "No contact groups returned from the service.";
					return StatusCode(500, "No contact groups returned from the service.");
                }

                var formattedNameSelectedGroup =
	                contactGroupList.FirstOrDefault(g => g.Name == selectedGroup)?.FormattedName;


				ViewBag.ContactGroups = contactGroupList; // Set groups in ViewBag
                ViewBag.ContactsCount = contacts.Count; // Total contacts count

                if (formattedNameSelectedGroup != null)
                {
	                ViewBag.ContactGroupSelected = formattedNameSelectedGroup; // Save the name of the group to use it in the next View
				}


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
            var contactGroups = await _peopleGoogleService.GetContactGroupsAsync();
            var myContactsGroup = contactGroups.FirstOrDefault(g => g.Name == "myContacts");
            var newGroup = contactGroups.FirstOrDefault(g => g.ResourceName == selectedGroup);
            var newContactGroups = new List<string>();

            if (string.IsNullOrEmpty(selectedGroup))
            {
                newContact.Labels.Add(myContactsGroup.ResourceName);
                newContactGroups.Add(myContactsGroup.FormattedName);
			}
            else
            {
                newContact.Labels.Add(newGroup.ResourceName);
                newContact.Labels.Add(myContactsGroup.ResourceName);
                newContactGroups.Add(newGroup.FormattedName);
                newContactGroups.Add(myContactsGroup.FormattedName);
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

				ViewBag.NewContactLabels = new List<string>();

				foreach (var label in newContactGroups)
				{
					ViewBag.NewContactLabels.Add(label);
				}


				ViewBag.ShowSuccessMessage = true;

	            await _peopleGoogleService.AddContactAsync(newContact, selectedGroup);
                return View();


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("/update-contact")]
		public async Task<IActionResult> UpdateContact(string resourceName)
		{

			Console.WriteLine($"Received ID: {resourceName}");
			

			var contact = await _peopleGoogleService.GetContactByIdAsync(resourceName);

			if (contact == null)
			{
				return NotFound(); // Return 404 if contact not found
			}

			try
			{
				var contactViewModel = new ContactViewModel
				{
					ResourceName = contact.ResourceName,
					FirstName = contact.Names?.FirstOrDefault()?.GivenName ?? string.Empty,
					LastName = contact.Names?.FirstOrDefault()?.FamilyName ?? string.Empty,
					Email = contact.EmailAddresses?.FirstOrDefault()?.Value ?? string.Empty,
					PhoneNumber = contact.PhoneNumbers?.FirstOrDefault()?.Value ?? string.Empty,
					Birthday = contact.Birthdays?.FirstOrDefault() != null
						? $"{contact.Birthdays.FirstOrDefault().Date.Year}/{contact.Birthdays.FirstOrDefault().Date.Month}/{contact.Birthdays.FirstOrDefault().Date.Day}"
						: string.Empty, // Format as MM/DD/YYYY if available

					Labels = contact.Memberships?.Select(m => m.ContactGroupMembership?.ContactGroupResourceName).Where(resourceName => resourceName != null).ToList() ?? new List<string>()
				};

				var contactGroups = await _peopleGoogleService.GetContactGroupsAsync();
				ViewBag.ContactGroups = contactGroups;

				return View(contactViewModel); 
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			
		}

		[HttpPost]
		[Route("/update-contact")]
		public async Task<IActionResult> UpdateContact(ContactViewModel updatedContact, string resourceName)
		{

			if (!ModelState.IsValid)
			{
				return View(updatedContact); 
			}

			try
			{
				var contactGroups = await _peopleGoogleService.GetContactGroupsAsync(); // get the groups 
				ViewBag.ContactGroups = contactGroups; // pass the groups to the View


				var updatedPerson = await _peopleGoogleService.UpdateContactAsync(resourceName, updatedContact);
				var groupMapping = contactGroups.ToDictionary(g => g.ResourceName, g => g.FormattedName); // dictionary storing the ResourceName as key and value FormattedName of each group


				ViewBag.ShowSuccessMessage = true;
				ViewBag.NewContactFirstName = updatedContact.FirstName;
				ViewBag.NewContactLastName = updatedContact.LastName;
				ViewBag.NewContactEmail = string.IsNullOrWhiteSpace(updatedContact.Email) ? "N/A" : updatedContact.Email;
				ViewBag.NewContactPhoneNumber = string.IsNullOrWhiteSpace(updatedContact.PhoneNumber) ? "N/A" : updatedContact.PhoneNumber;
				ViewBag.NewContactBirthday = string.IsNullOrWhiteSpace(updatedContact.Birthday) ? "N/A" : updatedContact.Birthday;
				ViewBag.NewContactLabels = updatedPerson.Memberships?
					.Select(m => groupMapping.TryGetValue(m.ContactGroupMembership.ContactGroupResourceName, out var groupName) ? groupName : "Unknown")
					.ToList();

				return View(updatedContact); // Return updated contact to the view
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error updating contact: {ex.Message}");
				return View(updatedContact); // Return to the view with error message
			}
		}

		[HttpPost]
		public async Task<IActionResult> DeleteContact(string resourceName, string returnUrl)
		{
			var contact = await _peopleGoogleService.GetContactByIdAsync(resourceName);
			var displayName = contact.Names?.Select(n => n.DisplayName).FirstOrDefault();


			if (!string.IsNullOrEmpty(displayName))
			{
				TempData["Message"] = $"{displayName} was successfully removed!";
			}
			else
			{
				TempData["Message"] = "Contact was successfully removed!";
			}

			await _peopleGoogleService.DeleteContactAsync(resourceName);

			if (!string.IsNullOrEmpty(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction(nameof(GetAllContacts));
		}

        [HttpPost]
        public async Task<IActionResult> CreateContactGroup(string labelName)
        {
            if (string.IsNullOrEmpty(labelName))
            {
                return BadRequest("Label name cannot be empty");
            }

            try
            {
                var createdGroup = await _peopleGoogleService.CreateContactGroupAsync(labelName);

                TempData["ShowSuccessMessage"] = true;
                TempData["Message"] = $"Label '{createdGroup.FormattedName}' created successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating label: {ex.Message}";
            }

            return RedirectToAction(nameof(GetAllContacts));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveContactGroup(string labelName)
        {
	        try
	        {
		        var message = await _peopleGoogleService.RemoveContactGroupAsync(labelName);

		        TempData["ShowSuccessMessage"] = true;
		        TempData["Message"] = message;
			}
	        catch (Exception ex)
	        {
		        TempData["ErrorMessage"] = $"Error deleting group: {ex.Message}";
	        }

	        return RedirectToAction(nameof(GetAllContacts));
        }

        [HttpPost]
        public async Task<IActionResult> EditContactGroup(string groupResourceName, string newGroupName, string returnUrl)
        {
	        var oldAndNewNameDictTask = _peopleGoogleService.EditContactGroupAsync(groupResourceName, newGroupName);

	        Dictionary<string, ContactGroup> oldAndNewNameDict = await oldAndNewNameDictTask;


			if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
	        {
		        return Redirect(returnUrl);
	        }

	        foreach (var entry in oldAndNewNameDict)
	        {
		        string oldName = entry.Key; 
		        ContactGroup updatedGroup = entry.Value;

		        TempData["ShowSuccessMessage"] = true;
				TempData["Message"] = $"{oldName} was successfully renamed to {updatedGroup.FormattedName}.";
	        }

			return RedirectToAction(nameof(GetAllContacts));
		}

		[HttpGet]
        [Route("/search-contact")]
		public IActionResult SearchContacts()
		{
			return View();
		}

		[HttpPost]
        [Route("/search-contact")]

        public async Task<IActionResult> SearchContacts(string text, int pageNumber = 1)
        {
	        const int pageSize = 20; // Default page size

			if (string.IsNullOrWhiteSpace(text))
	        {
		        TempData["ErrorMessage"] = "Please enter a name to search.";
				return View(new List<ContactViewModel>());
	        }

			try
			{
				var foundContacts = await _peopleGoogleService.SearchContactsAsync(text.Trim(), pageNumber);

				var totalContacts = foundContacts.Count; 
				var totalPages = (int)Math.Ceiling((double)totalContacts / pageSize);


				if (foundContacts == null || !foundContacts.Any())
				{
					TempData["ErrorMessage"] = "No contacts were found. Try again.";
					return View("SearchContacts", new List<ContactViewModel>());
				}

				// Ensure the page number is valid
				if (pageNumber < 1) pageNumber = 1;
				if (pageNumber > totalPages) pageNumber = totalPages;

				ViewBag.TotalPages = totalPages;
				ViewBag.TotalContacts = totalContacts;
				ViewBag.CurrentPage = pageNumber;

				TempData["SearchTerm"] = text;

				var pagedContacts = foundContacts
					.Skip((pageNumber - 1) * pageSize)
					.Take(pageSize)
					.ToList();


                return View(pagedContacts);
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Error searching for contacts: {ex.Message}";

				return View("SearchContacts", new List<ContactViewModel>());
			}

		}

		[HttpPost]
		public IActionResult CreateEventForContact(string calendarId, string contactEmail)
		{

			var model = new EventViewModel
			{
				CalendarId = calendarId,
				Attendants = new List<string> { contactEmail }
			};

			TempData["EventViewModel"] = JsonConvert.SerializeObject(model);

			return RedirectToAction("CreateEvent", "Calendar");

		}

	}

}

