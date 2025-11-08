using HaldiramPromotionalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<MaterialMaster> MaterialMaster { get; set; }
        public DbSet<PointsToCashCampaign> PointsToCashCampaigns { get; set; }
        public DbSet<PointsRewardCampaign> PointsRewardCampaigns { get; set; }
        public DbSet<FreeProductCampaign> FreeProductCampaigns { get; set; }
        public DbSet<AmountReachGoalCampaign> AmountReachGoalCampaigns { get; set; }
        public DbSet<SessionDurationRewardCampaign> SessionDurationRewardCampaigns { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Poster> Posters { get; set; }
        public DbSet<MaterialImage> MaterialImages { get; set; }
    }
}