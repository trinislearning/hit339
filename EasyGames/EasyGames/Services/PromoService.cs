namespace EasyGames.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using EasyGames.Data;
    using EasyGames.Models;
    using Microsoft.EntityFrameworkCore;

    // Minimal version that logs to DB; you can swap to your EmailService later.
    public class PromoService : IPromoService
    {
        private readonly ApplicationDbContext _db;
        public PromoService(ApplicationDbContext db) { _db = db; }

        public async Task<int> SendCampaignAsync(EmailCampaign campaign)
        {
            var q = _db.CustomerProfiles.Include(c => c.User).AsQueryable();
            if (campaign.TargetTier.HasValue)
                q = q.Where(c => c.Tier == campaign.TargetTier.Value);

            var recipients = await q.Where(c => c.User!.Email != null).ToListAsync();
            // TODO: if you have a real EmailService, send emails here.

            campaign.Recipients = recipients.Count;
            campaign.Sent = recipients.Count;
            _db.EmailCampaigns.Add(campaign);
            await _db.SaveChangesAsync();
            return recipients.Count;
        }
    }
}
