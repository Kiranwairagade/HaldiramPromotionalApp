using System;
using System.Threading.Tasks;
using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateCampaignNotificationAsync(int userId, string campaignName, string campaignType, int campaignId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "New Campaign Started",
                Message = $"{campaignType} campaign \"{campaignName}\" has started!",
                Type = "campaign",
                RelatedEntityId = campaignId,
                RelatedEntityType = "Campaign"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateOrderNotificationAsync(int userId, int orderId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Order Placed",
                Message = $"Your order #{orderId} has been placed successfully!",
                Type = "order",
                RelatedEntityId = orderId,
                RelatedEntityType = "Order"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateVoucherNotificationAsync(int userId, string voucherCode, decimal voucherValue, int voucherId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Voucher Generated",
                Message = $"Voucher {voucherCode} worth ₹{voucherValue} has been generated!",
                Type = "voucher",
                RelatedEntityId = voucherId,
                RelatedEntityType = "Voucher"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateCampaignExpiryNotificationAsync(int userId, string campaignName, string campaignType, int campaignId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Campaign Expiring Soon",
                Message = $"{campaignType} campaign \"{campaignName}\" expires soon!",
                Type = "campaign",
                RelatedEntityId = campaignId,
                RelatedEntityType = "Campaign"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateVoucherExpiryNotificationAsync(int userId, string voucherCode, int voucherId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = "Voucher Expiring Soon",
                Message = $"Voucher {voucherCode} expires soon!",
                Type = "voucher",
                RelatedEntityId = voucherId,
                RelatedEntityType = "Voucher"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}