using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Services.Contracts
{
    public interface IGoogleCalendarService
    {
         Task<IList<CalendarListEntry>> GetAllCalendarsAsync();
         Task<IList<CalendarListEntry>> GetEditableCalendarsAsync();
         Task<List<Event>> GetEventsForCalendarAsync(string calendarId);
		 Task<IList<Event>> GetEventsAsync(string calendarId);
         Task<Event> GetEventByIdAsync(string eventId);
         Task<Event> GetEventByIdAsync(string calendarId, string eventId);
         Task<Event> AddEventAsync(string calendarId, Event newEvent, EventsResource.InsertRequest.SendUpdatesEnum sendUpdates);
         Task<Event> AddEventAsync(string calendarId, string eventId, Event newEvent);
         Task<IList<Event>> GetEventByIdAcrossAllCalendarsAsync(string eventId);
         Task UpdateEventAsync(string calendarId, string eventId, Event updatedEvent);
         Task UpdateEventAsync(string calendarId, string eventId, Event updatedEvent, EventsResource.UpdateRequest.SendUpdatesEnum sendUpdates);
         Task<int> DeleteEventAsync(string calendarId, string eventId, bool deleteSeries = false);
         Task<Calendar> CreateCalendarAsync(string summary, string timeZone, string? description);
         Task<Calendar> UpdateExistingCalendar(string calendarId, Calendar calendarToUpdate);
         Task<string> GetPrimaryCalendarTimeZoneAsync();
         Task<bool> DeleteCalendarAsync(string calendarId);
         Task<Calendar> GetCalendarByIdAsync(string calendarId);
         Task<Event> CopyEventToCalendarAsync(string sourceCalendarId, string eventId, string destinationCalendarId);

    }
}
