using Calendar_by_I_M_Marinov.Models;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Mvc;

namespace Calendar_by_I_M_Marinov.Services.Contracts
{
    public interface IGoogleCalendarService
    {
         Task<IList<CalendarListEntry>> GetCalendarsAsync();
         Task<IList<Event>> GetEventsAsync(string calendarId);
         Task<Event> GetEventByIdAsync(string eventId);
         Task<Event> GetEventByIdAsync(string calendarId, string eventId);
         Task<Event> AddEventAsync(Event newEvent);
         Task<Event> AddEventAsync(string calendarId, Event newEvent);
         Task<Event> AddEventAsync(string calendarId, string eventId, Event newEvent);
         Task<IList<Event>> GetEventByIdAcrossAllCalendarsAsync(string eventId);
         Task<Event> UpdateEventAsync(string calendarId, string eventId, Event updatedEvent);
         Task DeleteEventAsync(string calendarId, string eventId);
         Task DeleteEventAsync(string eventId);
    }
}
