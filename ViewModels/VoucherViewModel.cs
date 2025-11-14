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

    public class VoucherViewModel
    {
        public List<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public List<Voucher> RedemptionHistory { get; set; } = new List<Voucher>(); // Add redemption history
        public int TotalPoints { get; set; } = 0;
        public DealerMaster Dealer { get; set; }
        public ShopkeeperMaster Shopkeeper { get; set; }
        public Dictionary<int, string> VoucherQRCodeData { get; set; } = new Dictionary<int, string>();
        
        // Properties for product details after voucher redemption
        public int VoucherId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public bool ShowProductForm { get; set; } = false;
    }
}