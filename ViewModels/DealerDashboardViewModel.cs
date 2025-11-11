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
        public Order? RecentOrder { get; set; } // Make this property nullable
    }
}