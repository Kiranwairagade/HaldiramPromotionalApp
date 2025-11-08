using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;
using System.Collections.Generic;

namespace HaldiramPromotionalApp.ViewModels
{
    public class ViewCampaignsViewModel
    {
        public List<DetailedPointsToCashCampaign> DetailedPointsToCashCampaigns { get; set; } = new List<DetailedPointsToCashCampaign>();
        public List<DetailedPointsRewardCampaign> DetailedPointsRewardCampaigns { get; set; } = new List<DetailedPointsRewardCampaign>();
        public List<DetailedFreeProductCampaign> DetailedFreeProductCampaigns { get; set; } = new List<DetailedFreeProductCampaign>();
        public List<AmountReachGoalCampaign> AmountReachGoalCampaigns { get; set; } = new List<AmountReachGoalCampaign>();
        public List<SessionDurationRewardCampaign> SessionDurationRewardCampaigns { get; set; } = new List<SessionDurationRewardCampaign>();
    }
}