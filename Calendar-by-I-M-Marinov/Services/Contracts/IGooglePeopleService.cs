﻿using Calendar_by_I_M_Marinov.Models.People;
using Google.Apis.PeopleService.v1.Data;

namespace Calendar_by_I_M_Marinov.Services.Contracts
{
    public interface IGooglePeopleService
    {
        Task<string> FetchDisplayNameByEmailAsync(string email);
        Task<List<ContactViewModel>> GetAllContactsAsync();


    }
}
