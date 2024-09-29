using Calendar_by_I_M_Marinov.Models.People;
using Calendar_by_I_M_Marinov.Services.Contracts;
using Google.Apis.PeopleService.v1.Data;

namespace Calendar_by_I_M_Marinov.Services
{
	using Google.Apis.Auth.OAuth2;
    using Google.Apis.PeopleService.v1;
	using Google.Apis.Services;
	using Google.Apis.Util.Store;
    using Microsoft.Extensions.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

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
            request.GroupFields = "name,memberCount"; 
            var response = await request.ExecuteAsync();

            return response.ContactGroups.ToList();
        }
        /* Get the Contact Group based on the groupResourceName*/
        public async Task<ContactGroup> GetContactGroupAsync(string groupResourceName)
        {
            return await _peopleService.ContactGroups.Get(groupResourceName).ExecuteAsync();
        }
        /* Get the Person based on the groupResourceName*/
        public async Task<Person> GetPersonAsync(string personResourceName)
        {
            var request = _peopleService.People.Get(personResourceName);
            request.PersonFields = "names,emailAddresses,phoneNumbers,birthdays";
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
                    : new List<Birthday>
                    {
                        new Birthday
                        {
                            Date = new Date
                            {
                                Day = int.Parse(newContact.Birthday.Split('/')[1]),
                                Month = int.Parse(newContact.Birthday.Split('/')[0]),
                                Year = int.Parse(newContact.Birthday.Split('/')[2])
                            }
                        }
                    },
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




    }

}
