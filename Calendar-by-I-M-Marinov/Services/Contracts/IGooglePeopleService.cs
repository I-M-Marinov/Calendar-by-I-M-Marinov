﻿using Calendar_by_I_M_Marinov.Models.People;
using Google.Apis.PeopleService.v1.Data;

namespace Calendar_by_I_M_Marinov.Services.Contracts
{
    public interface IGooglePeopleService
    {
        Task<string> FetchDisplayNameByEmailAsync(string email);
        Task<List<ContactViewModel>> GetAllContactsAsync();
        Task<List<ContactViewModel>> GetAllContactsAsync(string selectedGroup);
        Task<List<ContactGroup>> GetContactGroupsAsync();
        Task<ContactGroup> GetContactGroupAsync(string groupResourceName);
        Task<ContactGroup> CreateContactGroupAsync(string labelName);
        Task<string> RemoveContactGroupAsync(string labelName);
        Task<Dictionary<string, ContactGroup>> EditContactGroupAsync(string groupResourceName, string newGroupName);
        Task<Person> GetPersonAsync(string personResourceName);
        Task<string> AddContactAsync(ContactViewModel newContact, string selectedGroup);
        Task<Person> GetContactByIdAsync(string resourceName);
        Task<Person> UpdateContactAsync(string resourceName, ContactViewModel updatedContact);
        Task<string> DeleteContactAsync(string resourceName);
        Task<List<ContactViewModel>> SearchContactsAsync(string name, int pageNumber);


    }
}
