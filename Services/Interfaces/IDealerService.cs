using HaldiramPromotionalApp.DTOs;

namespace HaldiramPromotionalApp.Services.Interfaces
{
    /// <summary>
    /// Interface for dealer-related business logic
    /// </summary>
    public interface IDealerService
    {
        /// <summary>
        /// Get dealer dashboard data
        /// </summary>
        /// <param name="phoneNumber">Dealer's phone number</param>
        /// <returns>Dashboard data DTO</returns>
        Task<DealerDashboardDto> GetDashboardDataAsync(string phoneNumber);

        /// <summary>
        /// Get dealer points details
        /// </summary>
        /// <param name="phoneNumber">Dealer's phone number</param>
        /// <returns>Points details DTO</returns>
        Task<PointsDetailsDto> GetPointsDetailsAsync(string phoneNumber);

        /// <summary>
        /// Get dealer redemption history
        /// </summary>
        /// <param name="phoneNumber">Dealer's phone number</param>
        /// <returns>Redemption history DTO</returns>
        Task<RedemptionHistoryDto> GetRedemptionHistoryAsync(string phoneNumber);

        /// <summary>
        /// Get dealer vouchers
        /// </summary>
        /// <param name="phoneNumber">Dealer's phone number</param>
        /// <returns>Voucher list DTO</returns>
        Task<VoucherListDto> GetVouchersAsync(string phoneNumber);

        /// <summary>
        /// Get paginated vouchers for dealer
        /// </summary>
        /// <param name="phoneNumber">Dealer's phone number</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="status">Voucher status filter (active, redeemed, expired)</param>
        /// <param name="campaignType">Campaign type filter</param>
        /// <returns>Paginated vouchers DTO</returns>
        Task<PaginatedVouchersDto> GetPaginatedVouchersAsync(string phoneNumber, int page, int pageSize, string status = null, string campaignType = null);

        /// <summary>
        /// Generate a new voucher for dealer
        /// </summary>
        /// <param name="phoneNumber">Dealer's phone number</param>
        /// <param name="request">Voucher generation request</param>
        /// <returns>Generation response DTO</returns>
        Task<GenerateVoucherResponseDto> GenerateVoucherAsync(string phoneNumber, GenerateVoucherRequestDto request);

        /// <summary>
        /// Get paginated products/materials
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="category">Category filter</param>
        /// <returns>Paginated products DTO</returns>
        Task<PaginatedProductsDto> GetPaginatedProductsAsync(int page, int pageSize, string category = null);

        /// <summary>
        /// Get dealer by phone number
        /// </summary>
        /// <param name="phoneNumber">Dealer's phone number</param>
        /// <returns>Dealer ID or null if not found</returns>
        Task<int?> GetDealerIdByPhoneAsync(string phoneNumber);

        /// <summary>
        /// Get total points for dealer
        /// </summary>
        /// <param name="dealerId">Dealer ID</param>
        /// <returns>Total points</returns>
        Task<int> GetTotalPointsAsync(int dealerId);

        /// <summary>
        /// Process automatic voucher generation for dealer
        /// </summary>
        /// <param name="dealerId">Dealer ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="totalPoints">Total points</param>
        /// <returns>Number of vouchers generated</returns>
        Task<int> ProcessAutomaticVoucherGenerationAsync(int dealerId, int userId, int totalPoints);
    }
}
