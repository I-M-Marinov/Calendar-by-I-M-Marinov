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
                        var name = person.Names?.FirstOrDefault()?.DisplayName ?? "No Name";
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
                            Name = name,
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
                        var name = person.Names?.FirstOrDefault()?.DisplayName ?? "No Name";
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
                                // Assuming ResourceName is what you need
                                if (groupLookup.TryGetValue(membership.ContactGroupMembership.ContactGroupResourceName, out var groupName))
                                {
                                    labels.Add(groupName);
                                }
                            }
                        }

                        // Add only contacts that belong to the selected group
                        if (labels.Contains(selectedGroup))
                        {
                            contactViewModels.Add(new ContactViewModel
                            {
                                Name = name,
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

    }

}
