namespace HaldiramPromotionalApp.DTOs
{
    /// <summary>
    /// DTO for dealer dashboard data
    /// </summary>
    public class DealerDashboardDto
    {
        public int DealerId { get; set; }
        public string DealerName { get; set; }
        public string PhoneNumber { get; set; }
        public int TotalPoints { get; set; }
        public int AvailablePoints { get; set; }
        public int UsedPoints { get; set; }
        public List<PosterDto> Posters { get; set; }
        public List<CampaignSummaryDto> ActiveCampaigns { get; set; }
        public List<MaterialDto> Materials { get; set; }
        public OrderSummaryDto RecentOrder { get; set; }
        public List<BasicOrderDto> RecentBasicOrders { get; set; }
    }

    /// <summary>
    /// DTO for poster information
    /// </summary>
    public class PosterDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for campaign summary
    /// </summary>
    public class CampaignSummaryDto
    {
        public int Id { get; set; }
        public string CampaignName { get; set; }
        public string CampaignType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public bool IsActive { get; set; }
        public decimal? VoucherValue { get; set; }
        public int? PointsRequired { get; set; }
        public int VoucherValidity { get; set; }
        public decimal TargetAmount { get; set; }
        public int SessionDuration { get; set; }
        public List<CampaignMaterialDto> MaterialDetails { get; set; }
        public string RewardProductName { get; set; }
        public List<CampaignFreeProductDto> FreeProductDetails { get; set; }
    }

    public class CampaignMaterialDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public int Points { get; set; }
    }

    public class CampaignFreeProductDto
    {
        public int MaterialId { get; set; }
        public int FreeProductId { get; set; }
        public string FreeProductName { get; set; }
        public int FreeQuantity { get; set; }
    }

    /// <summary>
    /// DTO for material/product information
    /// </summary>
    public class MaterialDto
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string ShortName { get; set; }
        public string MaterialCode { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal DealerPrice { get; set; }
        public string ImagePath { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for order summary
    /// </summary>
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public int TotalPoints { get; set; }
        public string Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    /// <summary>
    /// DTO for order item
    /// </summary>
    public class OrderItemDto
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int Points { get; set; }
    }

    /// <summary>
    /// DTO for basic order
    /// </summary>
    public class BasicOrderDto
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string SapCode { get; set; }
        public string ShortCode { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
