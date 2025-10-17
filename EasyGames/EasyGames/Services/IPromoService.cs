namespace EasyGames.Services
{
    using System.Threading.Tasks;
    using EasyGames.Models;

    public interface IPromoService
    {
        Task<int> SendCampaignAsync(EmailCampaign campaign);
    }
}
