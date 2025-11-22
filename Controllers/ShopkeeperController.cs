using System.Text.RegularExpressions;
using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace HaldiramPromotionalApp.Controllers
{
    public class ShopkeeperController : Controller
    {
        private readonly ILogger<ShopkeeperController> _logger;
        private readonly ApplicationDbContext _context;

        public ShopkeeperController(ILogger<ShopkeeperController> _logger, ApplicationDbContext context)
        {
            this._logger = _logger;
            _context = context;
        }

        // Helper method to generate QR codes
        private string GenerateQRCodeBase64(string data)
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new Base64QRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
            catch
            {
                return "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
            }
        }

        // GET: Shopkeeper or Shopkeeper/Index
        [Route("Home/ShopkeeperHome")] // Backward compatibility
        [Route("Shopkeeper")]
        [Route("Shopkeeper/Index")]
        public async Task<IActionResult> Index(bool showProductForm = false, int voucherId = 0)
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Shopkeeper")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewData["HideLayoutBottomNav"] = "true";

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);

                ShopkeeperMaster shopkeeper = null;
                if (user != null)
                {
                    shopkeeper = await _context.ShopkeeperMasters.FirstOrDefaultAsync(s => s.PhoneNumber == user.phoneno);
                }

                if (shopkeeper == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var vouchers = await _context.Vouchers
                    .Where(v => !v.IsRedeemed && v.ExpiryDate > DateTime.UtcNow)
                    .OrderByDescending(v => v.IssueDate)
                    .ToListAsync();

                var redemptionHistory = await _context.Vouchers
                    .Include(v => v.Dealer)
                    .Where(v => v.IsRedeemed)
                    .OrderByDescending(v => v.RedeemedDate)
                    .Take(20)
                    .ToListAsync();

                var redeemedProducts = await _context.RedeemedProducts.ToListAsync();

                var voucherQRCodeData = new Dictionary<int, string>();
                foreach (var voucher in vouchers)
                {
                    if (string.IsNullOrEmpty(voucher.QRCodeData))
                    {
                        voucher.QRCodeData = $"{voucher.VoucherCode}|{voucher.DealerId}|{voucher.VoucherValue}|{voucher.ExpiryDate:yyyy-MM-dd}";
                        _context.Vouchers.Update(voucher);
                    }
                    voucherQRCodeData[voucher.Id] = GenerateQRCodeBase64(voucher.QRCodeData);
                }

                await _context.SaveChangesAsync();

                var viewModel = new VoucherViewModel
                {
                    Vouchers = vouchers,
                    VoucherQRCodeData = voucherQRCodeData,
                    Shopkeeper = shopkeeper,
                    RedemptionHistory = redemptionHistory,
                    RedeemedProducts = redeemedProducts,
                    ShowProductForm = showProductForm,
                    VoucherId = voucherId
                };

                return View("~/Views/Shopkeeper/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShopkeeperHome action: {ex.Message}");
                return RedirectToAction("Login", "Auth");
            }
        }

        // POST: Shopkeeper/RedeemVoucher
        [HttpPost]
        [Route("Home/RedeemVoucher")] // Backward compatibility
        public async Task<IActionResult> RedeemVoucher(string voucherCode)
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Shopkeeper")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneno == userName);

                ShopkeeperMaster shopkeeper = null;
                if (user != null)
                {
                    shopkeeper = await _context.ShopkeeperMasters.FirstOrDefaultAsync(s => s.PhoneNumber == user.phoneno);
                }

                if (shopkeeper == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == voucherCode);

                if (voucher == null)
                {
                    TempData["ErrorMessage"] = "Voucher not found.";
                    return RedirectToAction("Index");
                }

                if (voucher.IsRedeemed)
                {
                    TempData["ErrorMessage"] = "This voucher has already been redeemed.";
                    return RedirectToAction("Index");
                }

                if (DateTime.UtcNow > voucher.ExpiryDate)
                {
                    TempData["ErrorMessage"] = "This voucher has expired.";
                    return RedirectToAction("Index");
                }

                voucher.IsRedeemed = true;
                voucher.RedeemedDate = DateTime.UtcNow;

                _context.Vouchers.Update(voucher);
                await _context.SaveChangesAsync();

                if (voucher.CampaignType == "PointsToCash")
                {
                    TempData["SuccessMessage"] = $"Voucher {voucher.VoucherCode} redeemed successfully! Please enter product details.";
                    return RedirectToAction("Index", new { showProductForm = true, voucherId = voucher.Id });
                }
                else
                {
                    TempData["SuccessMessage"] = $"Voucher {voucher.VoucherCode} redeemed successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RedeemVoucher action: {ex.Message}");
                TempData["ErrorMessage"] = "Error redeeming voucher. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Shopkeeper/RedeemVoucherByQR
        [HttpPost]
        [Route("Home/RedeemVoucherByQR")] // Backward compatibility
        public async Task<IActionResult> RedeemVoucherByQR([FromBody] QRScanModel model)
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return Json(new { success = false, message = "You must be logged in to redeem vouchers." });
            }

            var userRole = HttpContext.Session.GetString("role");
            if (userRole != "Shopkeeper")
            {
                return Json(new { success = false, message = "Only shopkeepers can redeem vouchers." });
            }

            try
            {
                var raw = (model?.QrData ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(raw))
                {
                    return Json(new { success = false, message = "Invalid QR code data." });
                }

                string voucherCode = null;
                var qrDataParts = raw.Split('|');
                
                if (qrDataParts.Length >= 4)
                {
                    voucherCode = qrDataParts[0].Trim();
                }
                else if (qrDataParts.Length == 1)
                {
                    var match = Regex.Match(raw, @"(?:code|voucherCode|voucher)=([^&\s]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        voucherCode = Uri.UnescapeDataString(match.Groups[1].Value);
                    }
                    else if (raw.Contains('/'))
                    {
                        var segs = raw.TrimEnd('/').Split('/');
                        voucherCode = Uri.UnescapeDataString(segs.Last());
                    }
                    else
                    {
                        voucherCode = Uri.UnescapeDataString(raw);
                    }
                }
                else
                {
                    voucherCode = qrDataParts[0].Trim();
                }

                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.VoucherCode == voucherCode);

                if (voucher == null)
                {
                    return Json(new { success = false, message = "Voucher not found." });
                }

                if (voucher.IsRedeemed)
                {
                    return Json(new { success = false, message = "This voucher has already been redeemed." });
                }

                if (DateTime.UtcNow > voucher.ExpiryDate)
                {
                    return Json(new { success = false, message = "This voucher has expired." });
                }

                voucher.IsRedeemed = true;
                voucher.RedeemedDate = DateTime.UtcNow;

                _context.Vouchers.Update(voucher);
                await _context.SaveChangesAsync();

                if (voucher.CampaignType == "PointsToCash")
                {
                    return Json(new { success = true, message = $"Voucher {voucher.VoucherCode} redeemed successfully!", requiresProductDetails = true, voucherId = voucher.Id });
                }
                else
                {
                    return Json(new { success = true, message = $"Voucher {voucher.VoucherCode} redeemed successfully!" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RedeemVoucherByQR action: {ex.Message}");
                return Json(new { success = false, message = "Error redeeming voucher. Please try again." });
            }
        }

        // POST: Shopkeeper/SaveProductDetails
        [HttpPost]
        [Route("Home/SaveProductDetails")] // Backward compatibility
        public async Task<IActionResult> SaveProductDetails(int voucherId, string ProductName, string ProductDescription, decimal ProductPrice, int Quantity)
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Shopkeeper")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var voucher = await _context.Vouchers
                    .Include(v => v.Dealer)
                    .FirstOrDefaultAsync(v => v.Id == voucherId);

                if (voucher == null)
                {
                    TempData["ErrorMessage"] = "Voucher not found.";
                    return RedirectToAction("Index");
                }

                var productDetails = new RedeemedProduct
                {
                    VoucherId = voucherId,
                    ProductName = ProductName,
                    Description = ProductDescription,
                    Price = ProductPrice,
                    Quantity = Quantity,
                    RedemptionDate = DateTime.UtcNow,
                    VoucherType = voucher.CampaignType
                };

                if (voucher.CampaignType == "PointsReward")
                {
                    var pointsRewardCampaign = await _context.PointsRewardCampaigns
                        .Include(c => c.RewardProduct)
                        .FirstOrDefaultAsync(c => c.Id == voucher.CampaignId);

                    if (pointsRewardCampaign?.RewardProduct != null)
                    {
                        productDetails.RewardProductId = pointsRewardCampaign.RewardProductId;
                        if (string.IsNullOrEmpty(ProductName))
                        {
                            productDetails.ProductName = pointsRewardCampaign.RewardProduct.ProductName;
                        }
                        if (ProductPrice == 0)
                        {
                            productDetails.Price = pointsRewardCampaign.RewardProduct.Price;
                        }
                    }
                }

                _context.RedeemedProducts.Add(productDetails);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Voucher {voucher.VoucherCode} redeemed successfully! Product details saved.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving product details: {ex.Message}");
                TempData["ErrorMessage"] = "Error saving product details. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}
