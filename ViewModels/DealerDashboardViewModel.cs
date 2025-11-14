using HaldiramPromotionalApp.Models;
using System.Collections.Generic;

namespace HaldiramPromotionalApp.ViewModels
{
    public class DealerDashboardViewModel
    {
        public List<Poster> Posters { get; set; } = new List<Poster>();
        public ViewCampaignsViewModel Campaigns { get; set; } = new ViewCampaignsViewModel();
        public List<MaterialMaster> Materials { get; set; } = new List<MaterialMaster>();
        public List<MaterialImage> MaterialImages { get; set; } = new List<MaterialImage>();
        // public List<MaterialPoints> MaterialPoints { get; set; } = new List<MaterialPoints>();
        public Order? RecentOrder { get; set; } // Make this property nullable
        public int TotalPoints { get; set; } = 0; // New property to store total points for the dealer
        
        // New property for DealerBasicOrders
        public List<DealerBasicOrder> RecentDealerBasicOrders { get; set; } = new List<DealerBasicOrder>();
    }
}