namespace HaldiramPromotionalApp.DTOs
{
    /// <summary>
    /// DTO for points details
    /// </summary>
    public class PointsDetailsDto
    {
        public int TotalPointsEarned { get; set; }
        public int AvailablePoints { get; set; }
        public int PointsUsed { get; set; }
        public List<PointsTransactionDto> Transactions { get; set; }
    }

    /// <summary>
    /// DTO for points transaction
    /// </summary>
    public class PointsTransactionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public int Points { get; set; }
        public string OrderNumber { get; set; }
    }

    /// <summary>
    /// DTO for redemption history
    /// </summary>
    public class RedemptionHistoryDto
    {
        public List<VoucherDto> RedeemedVouchers { get; set; }
        public List<RedeemedProductDto> RedeemedProducts { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal TotalValue { get; set; }
    }

    /// <summary>
    /// DTO for redeemed product
    /// </summary>
    public class RedeemedProductDto
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public string VoucherCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime RedemptionDate { get; set; }
        public string VoucherType { get; set; }
    }

    /// <summary>
    /// DTO for paginated products
    /// </summary>
    public class PaginatedProductsDto
    {
        public List<MaterialDto> Products { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasMore { get; set; }
        public int NextPage { get; set; }
        public string Category { get; set; }
    }
}
