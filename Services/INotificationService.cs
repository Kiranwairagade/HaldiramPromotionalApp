using System.Threading.Tasks;
using HaldiramPromotionalApp.Models;

namespace HaldiramPromotionalApp.Services
{
    public interface INotificationService
    {
        Task CreateCampaignNotificationAsync(int userId, string campaignName, string campaignType, int campaignId);
        Task CreateOrderNotificationAsync(int userId, int orderId);
        Task CreateVoucherNotificationAsync(int userId, string voucherCode, decimal voucherValue, int voucherId);
        Task CreateCampaignExpiryNotificationAsync(int userId, string campaignName, string campaignType, int campaignId);
        Task CreateVoucherExpiryNotificationAsync(int userId, string voucherCode, int voucherId);
    }
}