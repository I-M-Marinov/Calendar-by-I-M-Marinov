namespace Calendar_by_I_M_Marinov.Services.Contracts
{
    public interface IGooglePeopleService
    {
        Task<string> FetchDisplayNameByEmailAsync(string email);
    }
}
