using HaldiramPromotionalApp.Models;
using System.Collections.Generic;

namespace HaldiramPromotionalApp.ViewModels
{
    public class DetailedPointsToCashCampaign : PointsToCashCampaign
    {
        public List<MaterialDetail> MaterialDetails { get; set; } = new List<MaterialDetail>();
    }

    public class DetailedPointsRewardCampaign : PointsRewardCampaign
    {
        public List<MaterialDetail> MaterialDetails { get; set; } = new List<MaterialDetail>();
    }

    public class DetailedFreeProductCampaign : FreeProductCampaign
    {
        public List<MaterialDetail> MaterialDetails { get; set; } = new List<MaterialDetail>();
        public Dictionary<int, MaterialFreeProductDetail> FreeProductDetails { get; set; } = new Dictionary<int, MaterialFreeProductDetail>();
    }

    public class MaterialDetail
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Points { get; set; }
    }

    public class MaterialFreeProductDetail
    {
        public int FreeProductId { get; set; }
        public string FreeProductName { get; set; } = string.Empty;
        public int FreeQuantity { get; set; }
    }
}