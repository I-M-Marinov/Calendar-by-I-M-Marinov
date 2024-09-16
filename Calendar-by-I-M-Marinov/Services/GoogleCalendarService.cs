using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Calendar.v3.Data;
using Calendar_by_I_M_Marinov.Services;
using Calendar_by_I_M_Marinov.Services.Contracts;

public class GoogleCalendarService : IGoogleCalendarService
{
	private readonly CalendarService _calendarService;
    private readonly GooglePeopleService _peopleService;
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

		_calendarService = new CalendarService(new BaseClientService.Initializer()
		{
			HttpClientInitializer = credential,
			ApplicationName = _applicationName,
		});

        _peopleService = new GooglePeopleService(configuration);

    }
    public async Task<IList<CalendarListEntry>> GetAllCalendarsAsync()
	{
		var calendarListRequest = _calendarService.CalendarList.List();
		var calendarList = await calendarListRequest.ExecuteAsync();

		return calendarList.Items;
	}
	public async Task<IList<CalendarListEntry>> GetEditableCalendarsAsync()
	{
		var calendarListRequest = _calendarService.CalendarList.List();
		var calendarList = await calendarListRequest.ExecuteAsync();

		// Filter the calendars to include only those where the access role is 'owner' or 'writer'
		var filteredCalendars = calendarList.Items
			.Where(c => c.AccessRole == "owner" || c.AccessRole == "writer")
			.ToList();

		return filteredCalendars;
	}
	public async Task<List<Event>> GetEventsForCalendarAsync(string calendarId)
	{
		var events = new List<Event>();

		// Get the current date in UTC
		DateTime nowUtc = DateTime.UtcNow;
		DateTime startOfDayUtc = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);

		// Set the end of the year in UTC
		DateTime endOfYearUtc = new DateTime(nowUtc.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

		var request = _calendarService.Events.List(calendarId);
		request.TimeMin = startOfDayUtc;
		request.TimeMax = endOfYearUtc;
		request.ShowDeleted = false; // Exclude deleted events
		request.SingleEvents = true; // Expand recurring events
		request.MaxResults = 100; // Limit to 100 events per page
		request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime; // Order by start time

		do
		{
			var response = await request.ExecuteAsync();
			events.AddRange(response.Items);

			// Check if there are more pages of events
			request.PageToken = response.NextPageToken;
		} 
		while (!string.IsNullOrEmpty(request.PageToken));

		return events;
	}
	public async Task<IList<Event>> GetEventsAsync(string calendarId)
	{
		var request = _calendarService.Events.List(calendarId);

        request.Fields = "items(id,summary,start,end,location,attendees,creator,guestsCanModify,status,transparency,extendedProperties,description,visibility,recurrence, recurringEventId)";

        // Get the current date in UTC
        DateTime nowUtc = DateTime.UtcNow;
		DateTime startOfDayUtc = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);

		// Set the end of the year in UTC
		DateTime endOfYearUtc = new DateTime(nowUtc.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

		request.TimeMin = startOfDayUtc;
		request.TimeMax = endOfYearUtc;
		request.ShowDeleted = false; // Exclude deleted events
		request.SingleEvents = true; // Expand recurring events
		request.MaxResults = 200; // Limit to 200 events per page
		request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime; // Order by start time

		var response = await request.ExecuteAsync();

        foreach (var evt in response.Items)
        {
            if (evt.Attendees != null)
            {
                foreach (var attendee in evt.Attendees)
                {
                    // Fetch and set display names if needed
                    attendee.DisplayName = await _peopleService.FetchDisplayNameByEmailAsync(attendee.Email);
                }
            }
        }

        return response.Items;
	}
    // this method is only returning events by id from the Primary calendar !!!! 
    public virtual async Task<Event> GetEventByIdAsync(string eventId)
	{
		var request = _calendarService.Events.Get("primary", eventId);
		var resultEvent = await request.ExecuteAsync();

		if (resultEvent.Attendees != null)
		{
			foreach (var attendee in resultEvent.Attendees)
			{
				// Fetch and set display names if needed
				attendee.DisplayName = await _peopleService.FetchDisplayNameByEmailAsync(attendee.Email);
			}
		}


		return resultEvent;
	}
	// overload of the original method taking calendarId and eventId
	public async Task<Event> GetEventByIdAsync(string calendarId, string eventId)
	{
		var request = _calendarService.Events.Get(calendarId, eventId);

		var requestedEvent = await request.ExecuteAsync();

		if (requestedEvent.Attendees != null)
		{
			foreach (var attendee in requestedEvent.Attendees)
			{
				// Fetch and set display names if needed
				attendee.DisplayName = await _peopleService.FetchDisplayNameByEmailAsync(attendee.Email);
			}
		}

		return requestedEvent;
	}
	/* The three methods below are used for Adding and Editing events respectively */
	public async Task<Event> AddEventAsync(Event newEvent)
	{
		var insertRequest = _calendarService.Events.Insert(newEvent, "primary");
        var createdEvent = await insertRequest.ExecuteAsync();
		return createdEvent;
	}
    public async Task<Event> AddEventAsync(string calendarId, Event newEvent, EventsResource.InsertRequest.SendUpdatesEnum sendUpdates)
    {
        var insertRequest = _calendarService.Events.Insert(newEvent, calendarId);
        insertRequest.SendUpdates = sendUpdates; 
        var createdEvent = await insertRequest.ExecuteAsync();

        if (createdEvent.Attendees != null)
        {
            foreach (var attendee in createdEvent.Attendees)
            {
                // Fetch and set display names if needed
                attendee.DisplayName = await _peopleService.FetchDisplayNameByEmailAsync(attendee.Email);
            }
        }

        return createdEvent;
    }
    public async Task<Event> AddEventAsync(string calendarId, string eventId, Event newEvent)
	{
		if (newEvent == null)
			throw new ArgumentNullException(nameof(newEvent), "Event cannot be null.");

		// If you want to update an existing event, use the Update method
		if (!string.IsNullOrEmpty(eventId))
		{
			var updateRequest = _calendarService.Events.Update(newEvent, calendarId, eventId);
			return await updateRequest.ExecuteAsync();
		}

		// Otherwise, create a new event
		var insertRequest = _calendarService.Events.Insert(newEvent, calendarId);
		return await insertRequest.ExecuteAsync();
	}
    public async Task DeleteEventAsync(string eventId)

	{
		var request = _calendarService.Events.Delete("primary", eventId);
		await request.ExecuteAsync();
	}
    public async Task DeleteEventAsync(string calendarId, string eventId)
    {
        try
        {
            var request = _calendarService.Events.Delete(calendarId, eventId);
            await request.ExecuteAsync();
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine($"The event with ID '{eventId}' in calendar '{calendarId}' was not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while trying to delete the event: {ex.Message}");
        }
    }
    public async Task<int> DeleteEventAsync(string calendarId, string eventId, bool deleteSeries = false)
    {
        int deletedInstancesCount = 0;

        try
        {
            var eventRequest = _calendarService.Events.Get(calendarId, eventId);
            var eventToDelete = await eventRequest.ExecuteAsync();

            bool isRecurring = eventToDelete.Recurrence != null && eventToDelete.Recurrence.Any()
                               || !string.IsNullOrEmpty(eventToDelete.RecurringEventId);

            if (isRecurring && deleteSeries)
            {
                var instancesRequest = _calendarService.Events.Instances(calendarId, eventId);
                var instances = await instancesRequest.ExecuteAsync();

                if (instances.Items.Any())
                {
                    // Delete each instance of the recurring event and increment the counter
                    foreach (var instance in instances.Items)
                    {
                        var instanceDeleteRequest = _calendarService.Events.Delete(calendarId, instance.Id);
                        await instanceDeleteRequest.ExecuteAsync();
                        deletedInstancesCount++;  
                    }
                }
                // Delete the "master" event as well
                var masterDeleteRequest = _calendarService.Events.Delete(calendarId, eventId);
                await masterDeleteRequest.ExecuteAsync();
                deletedInstancesCount++; 
            }
            else
            {
                // If deleteSeries is false, or if the event is not recurring, delete the single event
                var deleteRequest = _calendarService.Events.Delete(calendarId, eventId);
                await deleteRequest.ExecuteAsync();
                deletedInstancesCount++; 
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while trying to delete the event: {ex.Message}");
        }

        return deletedInstancesCount; // Return the number of deleted instances
    }
    public async Task<IList<Event>> GetEventByIdAcrossAllCalendarsAsync(string eventId)
	{
		var calendars = await GetEditableCalendarsAsync(); // get all calendars that you are the owner or writer of
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
	public async Task UpdateEventAsync(string calendarId, string eventId, Event updatedEvent)
	{
		var request = _calendarService.Events.Update(updatedEvent, calendarId, eventId);
		await request.ExecuteAsync();
	}
	public async Task UpdateEventAsync(string calendarId, string eventId, Event updatedEvent, EventsResource.UpdateRequest.SendUpdatesEnum sendUpdates)
	{
		try
		{
			var request = _calendarService.Events.Update(updatedEvent, calendarId, eventId);
			request.SendUpdates = sendUpdates;
			await request.ExecuteAsync();
		}
		catch (Google.GoogleApiException ex)
		{
			Console.WriteLine($"Failed to update event with ID: {eventId}. Details: {ex.Message}");
			throw new Exception($"Failed to update event with ID: {eventId}. Details: {ex.Message}");
		}
		catch (Exception ex)
		{
			throw new Exception($"An unexpected error occurred while updating the event: {ex.Message}");
		}
	}
	public async Task<Calendar> CreateCalendarAsync(string summary, string timeZone, string? description = null)
    {
        var newCalendar = new Calendar
        {
            Summary = summary,
            TimeZone = timeZone,
            Description = description
        };

        var createdCalendar = await _calendarService.Calendars.Insert(newCalendar).ExecuteAsync();

		return createdCalendar;
    }

	public async Task<Calendar> UpdateExistingCalendar(string calendarId, Calendar calendarToUpdate)
	{

		var requestedCalendar = await _calendarService.Calendars.Get(calendarId).ExecuteAsync();

		requestedCalendar.Summary = calendarToUpdate.Summary;
		requestedCalendar.Description = calendarToUpdate.Description;

		var updatedCalendar = await _calendarService.Calendars.Update(requestedCalendar, calendarId).ExecuteAsync();

		return updatedCalendar;
	}
	public async Task<string> GetPrimaryCalendarTimeZoneAsync()
    {
	    var calendarId = "primary"; // ID for the primary calendar
	    var calendar = await _calendarService.Calendars.Get(calendarId).ExecuteAsync();
	    return calendar.TimeZone; // This will return the time zone of the primary calendar
    }
    public async Task<bool> DeleteCalendarAsync(string calendarId)
    {
	    try
	    {
		    await _calendarService.CalendarList.Delete(calendarId).ExecuteAsync();
		    return true;
	    }
	    catch (GoogleApiException ex)
	    {
		    Console.WriteLine($"An error occurred: {ex.Message}"); // for debugging purposes
		    return false;
	    }
    }
    public Task<Calendar> GetCalendarByIdAsync(string calendarId)
    {
	    var calendar = _calendarService.Calendars.Get(calendarId).ExecuteAsync(); 

	    return calendar;
    }

}
