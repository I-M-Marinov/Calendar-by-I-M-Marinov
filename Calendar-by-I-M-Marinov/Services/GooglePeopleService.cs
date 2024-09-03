using Calendar_by_I_M_Marinov.Services.Contracts;

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
                new[] { PeopleServiceService.Scope.ContactsReadonly }, // Use the required scope
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

	}

}
