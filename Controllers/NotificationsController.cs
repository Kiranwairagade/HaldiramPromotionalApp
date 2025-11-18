using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;

namespace HaldiramPromotionalApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Notifications
        [HttpGet]
        public async Task<ActionResult> GetNotifications()
        {
            // Check if user is logged in
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
            {
                return Ok(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // Get the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
                if (user == null)
                {
                    return Ok(new { success = false, message = "User not found" });
                }

                // Get unread notifications for the user
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id && !n.IsRead)
                    .OrderByDescending(n => n.CreatedDate)
                    .Select(n => new
                    {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        type = n.Type,
                        time = n.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        badge = GetBadgeTextStatic(n.Type) // Use static method
                    })
                    .ToListAsync();

                return Ok(new { success = true, notifications });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Error retrieving notifications: " + ex.Message });
            }
        }

        // POST: api/Notifications/MarkAsRead
        [HttpPost("MarkAsRead")]
        public async Task<ActionResult> MarkAsRead([FromBody] int[] notificationIds)
        {
            // Check if user is logged in
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
            {
                return Ok(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // Get the user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
                if (user == null)
                {
                    return Ok(new { success = false, message = "User not found" });
                }

                // Mark notifications as read
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == user.Id && notificationIds.Contains(n.Id))
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Notifications marked as read" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Error marking notifications as read: " + ex.Message });
            }
        }

        // POST: api/Notifications/Delete
        [HttpPost("Delete")]
        public async Task<ActionResult> Delete([FromBody] int[] notificationIds)
        {
            // Check if user is logged in
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(userName) || userRole != "Dealer")
            {
                return Ok(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);
                if (user == null)
                {
                    return Ok(new { success = false, message = "User not found" });
                }

                IQueryable<Notification> query = _context.Notifications.Where(n => n.UserId == user.Id);

                // If specific IDs provided, filter them
                if (notificationIds != null && notificationIds.Length > 0)
                {
                    query = query.Where(n => notificationIds.Contains(n.Id));
                }

                var notificationsToDelete = await query.ToListAsync();
                if (notificationsToDelete.Any())
                {
                    _context.Notifications.RemoveRange(notificationsToDelete);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = "Notifications deleted" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Error deleting notifications: " + ex.Message });
            }
        }

        // Static helper method to get badge text based on notification type
        private static string GetBadgeTextStatic(string type)
        {
            return type.ToLower() switch
            {
                "campaign" => "Campaign",
                "order" => "Order",
                "voucher" => "Voucher",
                _ => "Notification"
            };
        }
    }
}