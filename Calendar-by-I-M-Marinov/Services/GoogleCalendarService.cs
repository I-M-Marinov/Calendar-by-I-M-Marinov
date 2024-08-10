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
using Google;

public class GoogleCalendarService: IGoogleCalendarService
{
	private readonly CalendarService _service;
	private readonly string _applicationName = "Calendar-by-I-M-Marinov";

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
            ApplicationName = _applicationName,
        });
    }

    public async Task<IList<CalendarListEntry>> GetCalendarsAsync()
    {
        var calendarListRequest = _service.CalendarList.List();
        var calendarList = await calendarListRequest.ExecuteAsync();

        return calendarList.Items;
    }

    public async Task<IList<Event>> GetEventsAsync(string calendarId)
	{
		var request = _service.Events.List(calendarId);
		request.TimeMin = DateTime.Now;
        request.TimeMax = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59); // Until the end of the year
        request.ShowDeleted = false;
		request.SingleEvents = true;
		request.MaxResults = 100;
		request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

		var events = await request.ExecuteAsync();
		return events.Items;
	}

    // this method is only returning events by id from the Primary calendar !!!! 
    public virtual async Task<Event> GetEventByIdAsync(string eventId)
    {
        var request = _service.Events.Get("primary", eventId); 
        return await request.ExecuteAsync();
    }

    public async Task<Event> GetEventByIdAsync(string calendarId, string eventId)
    {
        var request = _service.Events.Get(calendarId, eventId);
        return await request.ExecuteAsync();
    }

    public async Task<Event> AddEventAsync(Event newEvent)
	{
		var insertRequest = _service.Events.Insert(newEvent, "primary");
		var createdEvent = await insertRequest.ExecuteAsync();
		return createdEvent;
	}

    public async Task<Event> AddEventAsync(string calendarId, string eventId, Event newEvent)
    {
        if (newEvent == null)
            throw new ArgumentNullException(nameof(newEvent), "Event cannot be null.");

        // If you want to update an existing event, use the Update method
        if (!string.IsNullOrEmpty(eventId))
        {
            var updateRequest = _service.Events.Update(newEvent, calendarId, eventId);
            return await updateRequest.ExecuteAsync();
        }

        // Otherwise, create a new event
        var insertRequest = _service.Events.Insert(newEvent, calendarId);
        return await insertRequest.ExecuteAsync();
    }

    public async Task<IList<Event>> GetEventByIdAcrossAllCalendarsAsync(string eventId)
    {
        var calendars = await GetCalendarsAsync(); // get all calendars
        var matchingEvents = new List<Event>();

        foreach (var calendar in calendars)
        {
            try
            {
                var events = await GetEventsAsync(calendar.Id);

                var filteredEvents = events.Where(e =>
                    (e.Id != null && e.Id.Contains(eventId, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Summary != null && e.Summary.Contains(eventId, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                if (filteredEvents.Count > 0)
                {
                    matchingEvents.AddRange(filteredEvents);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving events from calendar {calendar.Id}: {ex.Message}");
            }
        }

        return matchingEvents;
    }

    public async Task<Event> UpdateEventAsync(string calendarId, string eventId, Event updatedEvent)
    {
        var updateRequest = _service.Events.Update(updatedEvent, calendarId, eventId);

        var result = await updateRequest.ExecuteAsync();
        return result;
    }

    //TODO:
    // Add methods like DeleteEventAsync.
}
