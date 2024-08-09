using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
using System.Collections.Generic;
using Calendar_by_I_M_Marinov.Services.Contracts;

public class GoogleCalendarService: IGoogleCalendarService
{
	private readonly CalendarService _service;
	private string applicationName = "Calendar-by-I-M-Marinov";

	public GoogleCalendarService(IConfiguration configuration)
	{
		var clientId = configuration["Google:ClientId"];
		var clientSecret = configuration["Google:ClientSecret"];

        var clientSecrets = new ClientSecrets
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        // Ensure you have placed credentials.json in a secure and correct location.
        UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets,
            new[] { CalendarService.Scope.Calendar }, // Use Calendar scope for read and write access.
            "user",
            CancellationToken.None,
            new FileDataStore("token.json", true)).Result;

        _service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        });
    }

	public async Task<IList<Event>> GetEventsAsync()
	{
		var request = _service.Events.List("primary");
		request.TimeMin = DateTime.Now;
		request.ShowDeleted = false;
		request.SingleEvents = true;
		request.MaxResults = 10;
		request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

		var events = await request.ExecuteAsync();
		return events.Items;
	}

	public async Task<Event> AddEventAsync(Event newEvent)
	{
		var insertRequest = _service.Events.Insert(newEvent, "primary");
		var createdEvent = await insertRequest.ExecuteAsync();
		return createdEvent;
	}

	// Add methods for AddEventAsync, UpdateEventAsync, DeleteEventAsync if needed.
}
