﻿﻿﻿﻿﻿using HaldiramPromotionalApp.Models;
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
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DealerMaster> DealerMasters { get; set; }
        public DbSet<ShopkeeperMaster> ShopkeeperMasters { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DealerBasicOrder> DealerBasicOrders { get; set; }
        public DbSet<EmpToCustMap> EmpToCustMaps { get; set; } 
        public DbSet<Cust2EmpMap> Cust2EmpMaps { get; set; } 
        public DbSet<Customer_Master> Customer_Masters { get; set; }    


    }
}