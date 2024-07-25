namespace Blaved.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task UserRegistration(long UserId, string? firstName, string? lastName, long? whoseReferal, string? language, bool acceptedTermsOfUse = false, int RateReferralExchange = 30);
    }
}
