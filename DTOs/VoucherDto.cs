namespace HaldiramPromotionalApp.DTOs
{
    /// <summary>
    /// DTO for voucher information
    /// </summary>
    public class VoucherDto
    {
        public int Id { get; set; }
        public string VoucherCode { get; set; }
        public string CampaignType { get; set; }
        public string CampaignName { get; set; }
        public int CampaignId { get; set; }
        public decimal VoucherValue { get; set; }
        public int PointsUsed { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRedeemed { get; set; }
        public DateTime? RedeemedDate { get; set; }
        public string QRCodeBase64 { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
        public int DaysUntilExpiry => (ExpiryDate - DateTime.UtcNow).Days;
    }

    /// <summary>
    /// DTO for voucher list response
    /// </summary>
    public class VoucherListDto
    {
        public List<VoucherDto> Vouchers { get; set; }
        public int TotalPoints { get; set; }
        public int AvailablePoints { get; set; }
        public int UsedPoints { get; set; }
        public int ActiveVouchersCount { get; set; }
        public int RedeemedVouchersCount { get; set; }
        public int ExpiredVouchersCount { get; set; }
    }

    /// <summary>
    /// DTO for paginated vouchers
    /// </summary>
    public class PaginatedVouchersDto
    {
        public List<VoucherDto> Vouchers { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasMore { get; set; }
        public int NextPage { get; set; }
    }

    /// <summary>
    /// DTO for generating voucher request
    /// </summary>
    public class GenerateVoucherRequestDto
    {
        public int CampaignId { get; set; }
        public string CampaignType { get; set; }
        public int PointsToUse { get; set; }
    }

    /// <summary>
    /// DTO for generate voucher response
    /// </summary>
    public class GenerateVoucherResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public VoucherDto Voucher { get; set; }
    }
}
