using HaldiramPromotionalApp.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HaldiramPromotionalApp.ViewModels
{
    public class VoucherQRCodeData
    {
        public int VoucherId { get; set; }
        public string QRCodeBase64 { get; set; } = string.Empty;
    }

    public class VoucherCampaignDetails
    {
        public string CampaignType { get; set; } = string.Empty;
        public string CampaignName { get; set; } = string.Empty;
        public decimal VoucherValue { get; set; }
        public string RewardProductName { get; set; } = string.Empty;
        public Dictionary<string, int> FreeProducts { get; set; } = new Dictionary<string, int>();
        public decimal TargetAmount { get; set; }
        public int SessionDuration { get; set; } // In minutes
    }

    public class VoucherViewModel
    {
        public List<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public List<Voucher> RedemptionHistory { get; set; } = new List<Voucher>(); // Add redemption history
        public List<RedeemedProduct> RedeemedProducts { get; set; } = new List<RedeemedProduct>(); // Add redeemed products
        public int TotalPoints { get; set; } = 0;
        public DealerMaster Dealer { get; set; }
        public ShopkeeperMaster Shopkeeper { get; set; }
        public Dictionary<int, string> VoucherQRCodeData { get; set; } = new Dictionary<int, string>();
        
        // Campaign details for each voucher
        public Dictionary<int, VoucherCampaignDetails> VoucherCampaignDetails { get; set; } = new Dictionary<int, VoucherCampaignDetails>();
        
        // Properties for product details after voucher redemption
        public int VoucherId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public bool ShowProductForm { get; set; } = false;
    }
}