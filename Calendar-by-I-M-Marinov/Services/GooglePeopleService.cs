using Calendar_by_I_M_Marinov.Common;
using Calendar_by_I_M_Marinov.Models.People;
using Calendar_by_I_M_Marinov.Services.Contracts;


namespace Calendar_by_I_M_Marinov.Services
{
	using Google;
	using Google.Apis.PeopleService.v1.Data;
	using Google.Apis.Auth.OAuth2;
    using Google.Apis.PeopleService.v1;
	using Google.Apis.Services;
	using Google.Apis.Util.Store;
    using Microsoft.Extensions.Configuration;
	using System.Threading;
    using System.Threading.Tasks;
    using static DateTimeExtensions;

    public class GooglePeopleService: IGooglePeopleService
    {
        private readonly PeopleServiceService _peopleService;
        private readonly Dictionary<string, string> _emailDisplayNameMap = new();
		private readonly string _applicationName = "Calendar-by-I-M-Marinov";


        public GooglePeopleService(IConfiguration configuration)
        {
            var clientId = configuration["Google:ClientId"];
            var clientSecret = configuration["Google:ClientSecret"];

            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                new[] { PeopleServiceService.Scope.Contacts }, // Use the required scope
                "user",
                CancellationToken.None,
                new FileDataStore("people_token.json", true)).Result;

            _peopleService = new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
        }
		public async Task<string> FetchDisplayNameByEmailAsync(string email)
		{
			if (_emailDisplayNameMap.TryGetValue(email, out var cachedDisplayName))
			{
				return cachedDisplayName; // return the DisplayName if it is found in the map 
			}

			try
			{
				string nextPageToken = null;
				do
				{
					var request = _peopleService.People.Connections.List("people/me");
					request.PersonFields = "names,emailAddresses,metadata";
					request.PageSize = 100; // Maximum allowed per request from the Google People API 
					request.PageToken = nextPageToken;

					var response = await request.ExecuteAsync();

					if (response.Connections == null || !response.Connections.Any())
					{
						Console.WriteLine("No connections found.");
						return null;
					}

					// Search for the person by email in the current page
					var person = response.Connections
						.FirstOrDefault(c => c.EmailAddresses != null &&
											 c.EmailAddresses.Any(e => string.Equals(e.Value, email, StringComparison.OrdinalIgnoreCase)));

					if (person != null)
					{
						var displayName = person.Names?.FirstOrDefault()?.DisplayName;

						if (!string.IsNullOrEmpty(displayName))
						{
							_emailDisplayNameMap[email] = displayName; // Add the name in the dictionary map 
						}
						else
						{
							Console.WriteLine($"No display name found for email: {email}");
						}

						return displayName;
					}

					// Get the next page token
					nextPageToken = response.NextPageToken;

				} while (!string.IsNullOrEmpty(nextPageToken)); // Loop until there are no more pages

				Console.WriteLine($"No person found with email: {email}");
				return null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error fetching display name: {ex.Message}");
				return null;
			}
		}
		public async Task<Person> GetContactByIdAsync(string resourceName)
		{
			if (string.IsNullOrEmpty(resourceName))
			{
				throw new ArgumentException("Resource name cannot be null or empty.", nameof(resourceName));
			}
			
			try
			{
				var contact = await GetPersonAsync(resourceName);
				return contact;  
			}
			catch (Google.GoogleApiException ex)
			{
				if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;  
				}
				throw;  // Re-throw 
			}
		}
		public async Task<Person> UpdateContactAsync(string resourceName, ContactViewModel updatedContact)
		{
			if (string.IsNullOrEmpty(resourceName))
				throw new ArgumentException("Resource name cannot be null or empty.", nameof(resourceName));

			var personToUpdate = await GetContactByIdAsync(resourceName);

			if (personToUpdate == null)
			{
				throw new Exception("Contact not found.");
			}

			personToUpdate.Names = new List<Name>
			{
				new Name
				{
					GivenName = updatedContact.FirstName,
					FamilyName = updatedContact.LastName,
					DisplayName = $"{updatedContact.FirstName} {updatedContact.LastName}"
				}
			};

			personToUpdate.EmailAddresses = string.IsNullOrEmpty(updatedContact.Email)
				? null
				: new List<EmailAddress> { new EmailAddress { Value = updatedContact.Email } };

			personToUpdate.PhoneNumbers = string.IsNullOrEmpty(updatedContact.PhoneNumber)
				? null
				: new List<PhoneNumber> { new PhoneNumber { Value = updatedContact.PhoneNumber } };

			// Updating the birthday if provided
			if (!string.IsNullOrEmpty(updatedContact.Birthday))
			{
				var birthDateParts = updatedContact.Birthday.Split('/');
				personToUpdate.Birthdays = new List<Birthday>
		{
			new Birthday
			{
				Date = new Date
				{
					Day = int.Parse(birthDateParts[1]),  // Day
                    Month = int.Parse(birthDateParts[0]), // Month
                    Year = int.Parse(birthDateParts[2])   // Year
                }
			}
            
		};
				
			}
			else
			{
				personToUpdate.Birthdays = null;
			}

			if (updatedContact.Labels != null && updatedContact.Labels.Count > 0)
			{
				if (personToUpdate.Memberships == null)
				{
					personToUpdate.Memberships = new List<Membership>();
				}

				personToUpdate.Memberships = updatedContact.Labels
					.Select(label => new Membership
					{
						ContactGroupMembership = new ContactGroupMembership
						{
							ContactGroupResourceName = label
						}
					})
					.ToList();

			}

			if (!personToUpdate.Memberships.Any(m => m.ContactGroupMembership.ContactGroupResourceName == "contactGroups/myContacts"))
			{
				personToUpdate.Memberships.Add(new Membership
				{
					ContactGroupMembership = new ContactGroupMembership
					{
						ContactGroupResourceName = "contactGroups/myContacts"
					}
				});
			}

			var updateMask = "names,emailAddresses,phoneNumbers,birthdays,memberships"; // Specify fields to update
			var request = _peopleService.People.UpdateContact(personToUpdate, resourceName);
			request.UpdatePersonFields = updateMask;

			try
			{
				var updatedPerson = await request.ExecuteAsync();
				return updatedPerson;
			}
			catch (Exception ex)
			{
				// Log the error and provide a detailed message
				Console.WriteLine($"Error updating contact: {ex.Message}");
				throw;
			}
		}
		public async Task<List<ContactViewModel>> GetAllContactsAsync()
        {
            List<ContactViewModel> contactViewModels = new List<ContactViewModel>();

            var contactGroups = await GetContactGroupsAsync();

            var groupLookup = contactGroups.ToDictionary(g => g.ResourceName, g => g.Name);

            string nextPageToken = null;

            do
            {
                // Fetch all contacts
                var request = _peopleService.People.Connections.List("people/me");
                request.PersonFields = "names,emailAddresses,phoneNumbers,birthdays,memberships"; 
                request.PageSize = 1000;
                request.PageToken = nextPageToken; // Use the page token to fetch the next set of contacts

                ListConnectionsResponse response = await request.ExecuteAsync();

                if (response.Connections != null && response.Connections.Count > 0)
                { 
                    foreach (var person in response.Connections)
                    {
                        var firstName = person.Names?.FirstOrDefault()?.GivenName ?? null;
                        var lastName = person.Names?.FirstOrDefault()?.FamilyName ?? null;
                        var email = person.EmailAddresses?.FirstOrDefault()?.Value ?? "N/A";
                        var phone = person.PhoneNumbers?.FirstOrDefault()?.Value ?? "N/A";
                        var birthday = person.Birthdays?.FirstOrDefault()?.Date != null
                            ? $"{person.Birthdays.FirstOrDefault().Date.Month}/{person.Birthdays.FirstOrDefault().Date.Day}/{person.Birthdays.FirstOrDefault().Date.Year}"
                            : "N/A";

                        var labels = new List<string>();
                        if (person.Memberships != null)
                        {
                            foreach (var membership in person.Memberships)
                            {
                                if (groupLookup.TryGetValue(membership.ContactGroupMembership.ContactGroupResourceName, out var groupName))
                                {
                                    labels.Add(groupName);
                                }
                            }
                        }

                        contactViewModels.Add(new ContactViewModel
                        {
	                        ResourceName = person.ResourceName,
							FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                            PhoneNumber = phone,
                            Birthday = birthday,
                            Labels = labels 
                        });
                    }
                }

                nextPageToken = response.NextPageToken;

            } while (!string.IsNullOrEmpty(nextPageToken));

            return contactViewModels;
        }
        public async Task<List<ContactViewModel>> GetAllContactsAsync(string selectedGroup)
        {
            List<ContactViewModel> contactViewModels = new List<ContactViewModel>();

            var contactGroups = await GetContactGroupsAsync();

            var groupLookup = contactGroups.ToDictionary(g => g.ResourceName, g => g.Name);

            string nextPageToken = null;

            do
            {
                // Fetch all contacts
                var request = _peopleService.People.Connections.List("people/me");
                request.PersonFields = "names,emailAddresses,phoneNumbers,birthdays,memberships";
                request.PageSize = 1000;
                request.PageToken = nextPageToken; // Use the page token to fetch the next set of contacts

                ListConnectionsResponse response = await request.ExecuteAsync();

                if (response.Connections != null && response.Connections.Count > 0)
                {
                    foreach (var person in response.Connections)
                    {
	                    var firstName = person.Names?.FirstOrDefault()?.GivenName ?? null;
	                    var lastName = person.Names?.FirstOrDefault()?.FamilyName ?? null;
						var email = person.EmailAddresses?.FirstOrDefault()?.Value ?? "N/A";
                        var phone = person.PhoneNumbers?.FirstOrDefault()?.Value ?? "N/A";
                        var birthday = person.Birthdays?.FirstOrDefault()?.Date != null
                            ? $"{person.Birthdays.FirstOrDefault().Date.Month}/{person.Birthdays.FirstOrDefault().Date.Day}/{person.Birthdays.FirstOrDefault().Date.Year}"
                            : "N/A";

                        var labels = new List<string>();
                        if (person.Memberships != null)
                        {
                            foreach (var membership in person.Memberships)
                            {
                                if (groupLookup.TryGetValue(membership.ContactGroupMembership.ContactGroupResourceName, out var groupName))
                                {
                                    labels.Add(groupName);
                                }
                            }
                        }

                        if (labels.Contains(selectedGroup))
                        {
                            contactViewModels.Add(new ContactViewModel
                            {
	                            ResourceName = person.ResourceName,
								FirstName = firstName,
                                LastName = lastName,
                                Email = email,
                                PhoneNumber = phone,
                                Birthday = birthday,
                                Labels = labels
                            });
                        }
                    }
                }

                nextPageToken = response.NextPageToken;

            } while (!string.IsNullOrEmpty(nextPageToken));

            // Debugging: Check the number of contacts found for the selected group
            Console.WriteLine($"Total contacts in group '{selectedGroup}': {contactViewModels.Count}");

            return contactViewModels;
        }
        /* Get all the groups ( assuming all are up to 100 ) */
        public async Task<List<ContactGroup>> GetContactGroupsAsync()
        {
            var request = _peopleService.ContactGroups.List();
            request.PageSize = 100;
            request.GroupFields = "name,memberCount,groupType";
			var response = await request.ExecuteAsync();

			return response.ContactGroups.ToList();
        }
        /* Get the Contact Group based on the groupResourceName*/
        public async Task<ContactGroup> GetContactGroupAsync(string groupResourceName)
        {
            return await _peopleService.ContactGroups.Get(groupResourceName).ExecuteAsync();
        }
        /* Get the Person based on the groupResourceName*/
        public async Task<Person> GetPersonAsync(string resourceName)
        {
            var request = _peopleService.People.Get(resourceName);
            request.PersonFields = "names,emailAddresses,phoneNumbers,birthdays,memberships";
            return await request.ExecuteAsync();
        }
        public async Task<string> AddContactAsync(ContactViewModel newContact, string selectedGroup)
        {
            // Ensure Labels is initialized if null
            if (newContact.Labels == null)
            {
                newContact.Labels = new List<string>();
            }

            // Retrieve contact groups
            var contactGroups = await GetContactGroupsAsync();
            var myContactsGroup = contactGroups.FirstOrDefault(g => g.Name == "myContacts");

            // If the user didn't select a group, use the myContacts group ID
            if (string.IsNullOrEmpty(selectedGroup) && myContactsGroup != null)
            {
                selectedGroup = myContactsGroup.ResourceName; // Use the resource name
            }
            else if (string.IsNullOrEmpty(selectedGroup))
            {
                throw new Exception("No valid contact group selected and 'myContacts' group not found.");
            }

            var contactToCreate = new Person
            {
				Names = new List<Name>
				{
					new Name
					{
						DisplayName = $"{newContact.FirstName} {newContact.LastName}", // Full Name for display
						GivenName = newContact.FirstName, // First Name
						FamilyName = newContact.LastName  // Last Name
					}
				},
				EmailAddresses = string.IsNullOrEmpty(newContact.Email)
                    ? null
                    : new List<EmailAddress> { new EmailAddress { Value = newContact.Email } },
                PhoneNumbers = string.IsNullOrEmpty(newContact.PhoneNumber)
                    ? null
                    : new List<PhoneNumber> { new PhoneNumber { Value = newContact.PhoneNumber } },
                Birthdays = string.IsNullOrEmpty(newContact.Birthday)
                    ? null
                    : ParseBirthday(newContact.Birthday),  // Use the parsing method from the DateTimeExtension class to parse the date 
                Memberships = newContact.Labels.Select(label => new Membership
                {
                    ContactGroupMembership = new ContactGroupMembership
                    {
                        ContactGroupResourceName = label
                    }
                }).ToList()
            };

            // Add the contact via Google People API (or another service)
            var request = _peopleService.People.CreateContact(contactToCreate);
            var response = await request.ExecuteAsync();

            return response.ResourceName; // Return the ID of the newly created contact
        }
        public async Task<ContactGroup> CreateContactGroupAsync(string labelName)
        {
	        var contactGroup = new ContactGroup
	        {
		        Name = labelName
	        };

	        var requestBody = new CreateContactGroupRequest
	        {
		        ContactGroup = contactGroup
	        };

			try
	        {
		        var request = _peopleService.ContactGroups.Create(requestBody);
		        var createdGroup = await request.ExecuteAsync();
		        return createdGroup;
	        }
	        catch (Exception ex)
	        {
		        Console.WriteLine($"Error creating contact group: {ex.Message}");
		        throw;
	        }
        }
        public async Task<string> RemoveContactGroupAsync(string labelName)
        {
			try
			{
				var request = _peopleService.ContactGroups.Delete(labelName);
		        var groupToRemove = await request.ExecuteAsync();

		        return $"{groupToRemove} was successfully removed.";
	        }
	        catch (Exception ex)
	        {
		        Console.WriteLine($"Error deleting contact group: {ex.Message}");
		        throw;
	        }
        }
        public async Task<string> DeleteContactAsync(string resourceName)
        {
	        var personToDelete = await GetPersonAsync(resourceName);

	        if (personToDelete == null)
	        {
		        throw new Exception("Contact not found.");
	        }

	        try
	        {
		        var request = _peopleService.People.DeleteContact(resourceName);
		        await request.ExecuteAsync();
		        var displayName = personToDelete.Names?.FirstOrDefault()?.DisplayName ?? "Unnamed Contact";

				return $"{displayName} was removed from the contact list successfully!";
	        }
	        catch (GoogleApiException ex)
	        {
		        Console.WriteLine($"Failed to delete contact: {ex.Message}");
		        throw new ApplicationException("Error occurred while deleting the contact.", ex);
	        }
	        catch (Exception ex)
	        {
		        Console.WriteLine($"Unexpected error: {ex.Message}");
		        throw new ApplicationException("An unexpected error occurred.", ex);
	        }
        }

		public async Task<List<ContactViewModel>> SearchContactsAsync(string text, int pageNumber = 1)
		{
			const int pageSize = 30;

			try
			{
				var request = _peopleService.People.SearchContacts();
				request.Query = text;
				request.ReadMask = "names,emailAddresses,phoneNumbers,memberships";
				request.PageSize = 30; // Limit is 30 inclusive per the Google People API 

				var response = await request.ExecuteAsync();

				if (response.Results == null || response.Results.Count == 0)
				{
					return new List<ContactViewModel>();
				}

				var contactGroups = await GetContactGroupsAsync();
				var groupMapping = contactGroups.ToDictionary(g => g.ResourceName, g => g.FormattedName);

				var allContacts = response.Results.Select(person => new ContactViewModel
				{
					ResourceName = person.Person.ResourceName ?? "N/A",
					FirstName = person.Person.Names?.FirstOrDefault()?.GivenName ?? "N/A",
					LastName = person.Person.Names?.FirstOrDefault()?.FamilyName ?? "N/A",
					Email = person.Person.EmailAddresses?.FirstOrDefault()?.Value ?? "N/A",
					PhoneNumber = person.Person.PhoneNumbers?.FirstOrDefault()?.Value ?? "N/A",
					Birthday = person.Person.Birthdays?.FirstOrDefault()?.Date != null
						? $"{person.Person.Birthdays.First().Date.Month}/{person.Person.Birthdays.First().Date.Day}/{person.Person.Birthdays.First().Date.Year}"
						: null,
					Labels = person.Person.Memberships?
						.Select(m => groupMapping.TryGetValue(m.ContactGroupMembership.ContactGroupResourceName, out var groupName) ? groupName : "Unknown")
						.ToList() ?? new List<string>()
				}).ToList();

				var pagedContacts = allContacts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

				return pagedContacts;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error searching contacts: {ex.Message}");
				throw;
			}
		}




	}
}


