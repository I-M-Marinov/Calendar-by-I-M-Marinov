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
            try
            {
                // Fetch the user profile information
                var request = _peopleService.People.Connections.List("people/me");
                request.PersonFields = "names,emailAddresses";

                var response = await request.ExecuteAsync();

                // Log the number of connections and their details
                Console.WriteLine($"Response contains {response.Connections.Count} connections.");

                if (response.Connections == null || !response.Connections.Any())
                {
                    Console.WriteLine("No connections found.");
                    return null;
                }

                foreach (var connection in response.Connections)
                {
                    Console.WriteLine($"Connection email addresses: {string.Join(", ", connection.EmailAddresses.Select(e => e.Value))}");
                }

                // Look for the person by email
                var person = response.Connections
                    .FirstOrDefault(c => c.EmailAddresses != null &&
                                         c.EmailAddresses.Any(e => e.Value == email));

                if (person == null)
                {
                    Console.WriteLine($"No person found with email: {email}");
                    return null;
                }

                // Check if Names is null before accessing
                var displayName = person.Names?.FirstOrDefault()?.DisplayName;

                if (displayName == null)
                {
                    Console.WriteLine($"No display name found for email: {email}");
                }

                return displayName;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error fetching display name: {ex.Message}");
                return null;
            }
        }


    }

}
