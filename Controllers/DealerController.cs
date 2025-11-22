using HaldiramPromotionalApp.DTOs;
using HaldiramPromotionalApp.Extensions;
using HaldiramPromotionalApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HaldiramPromotionalApp.Controllers
{
    public class DealerController : Controller
    {
        private readonly ILogger<DealerController> _logger;
        private readonly IDealerService _dealerService;

        public DealerController(ILogger<DealerController> logger, IDealerService dealerService)
        {
            _logger = logger;
            _dealerService = dealerService;
        }

        // GET: Dealer or Dealer/Index
        [Route("Home/DealerHome")] // Backward compatibility
        [Route("Dealer")]
        [Route("Dealer/Index")]
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in and is a Dealer
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var dashboardData = await _dealerService.GetDashboardDataAsync(userName);

                if (dashboardData == null)
                {
                    _logger.LogWarning($"Dashboard data not found for user: {userName}");
                    return RedirectToAction("Login", "Auth");
                }

                // Process automatic voucher generation
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (userId > 0 && dashboardData.TotalPoints > 0)
                {
                    await _dealerService.ProcessAutomaticVoucherGenerationAsync(
                        dashboardData.DealerId, userId, dashboardData.TotalPoints);
                }

                var viewModel = dashboardData.ToViewModel();
                return View("~/Views/Dealer/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dealer dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return RedirectToAction("Login", "Auth");
            }
        }

        // GET: Dealer/PointsDetails
        [Route("Home/PointsDetails")] // Backward compatibility
        public async Task<IActionResult> PointsDetails()
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var pointsDetails = await _dealerService.GetPointsDetailsAsync(userName);

                if (pointsDetails == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var orderItems = pointsDetails.ToOrderItemsList();
                ViewData["TotalPointsEarned"] = pointsDetails.TotalPointsEarned;
                ViewData["AvailablePoints"] = pointsDetails.AvailablePoints;
                ViewData["PointsUsed"] = pointsDetails.PointsUsed;
                return View("~/Views/Dealer/PointsDetails.cshtml", orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading points details");
                TempData["ErrorMessage"] = "Error loading points details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Dealer/History
        [Route("Home/History")] // Backward compatibility
        public async Task<IActionResult> History()
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var history = await _dealerService.GetRedemptionHistoryAsync(userName);

                if (history == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var viewModel = history.ToViewModel();
                return View("~/Views/Dealer/History.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading history");
                TempData["ErrorMessage"] = "Error loading history. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Dealer/ViewCampaigns
        [Route("Home/ViewCampaigns")] // Backward compatibility
        public async Task<IActionResult> ViewCampaigns()
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var dashboardData = await _dealerService.GetDashboardDataAsync(userName);

                if (dashboardData == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var viewModel = dashboardData.ActiveCampaigns.ToViewCampaignsViewModel();
                return View("~/Views/Dealer/ViewCampaigns.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading campaigns");
                TempData["ErrorMessage"] = "Error loading campaigns. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Dealer/Vouchers
        [Route("Home/Vouchers")] // Backward compatibility
        public async Task<IActionResult> Vouchers()
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var voucherList = await _dealerService.GetVouchersAsync(userName);

                if (voucherList == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var viewModel = voucherList.ToViewModel();
                return View("~/Views/Dealer/Vouchers.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vouchers");
                TempData["ErrorMessage"] = "Error loading vouchers. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // POST: Dealer/GenerateVoucher
        [HttpPost]
        [Route("Home/GenerateVoucher")] // Backward compatibility
        public async Task<IActionResult> GenerateVoucher([FromForm] GenerateVoucherRequestDto request)
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var result = await _dealerService.GenerateVoucherAsync(userName, request);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Vouchers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating voucher");
                TempData["ErrorMessage"] = "Error generating voucher. Please try again.";
                return RedirectToAction("Vouchers");
            }
        }

        // GET: Dealer/GetVouchersPage (API)
        [HttpGet]
        [Route("Home/GetVouchersPage")] // Backward compatibility
        public async Task<IActionResult> GetVouchersPage(int page = 1, int pageSize = 10, string status = null, string campaignType = null)
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return Unauthorized();
            }

            try
            {
                var userName = HttpContext.Session.GetString("UserName");
                var result = await _dealerService.GetPaginatedVouchersAsync(userName, page, pageSize, status, campaignType);

                if (result == null)
                {
                    return Unauthorized();
                }

                return Ok(new
                {
                    vouchers = result.Vouchers,
                    hasMore = result.HasMore,
                    nextPage = result.NextPage,
                    totalCount = result.TotalCount,
                    totalPages = result.TotalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated vouchers");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // GET: Dealer/GetProductsPage (API)
        [HttpGet]
        [Route("Home/GetProductsPage")] // Backward compatibility
        public async Task<IActionResult> GetProductsPage(int page = 1, int pageSize = 10, string category = "__all__")
        {
            if (HttpContext.Session.GetString("UserName") == null || HttpContext.Session.GetString("role") != "Dealer")
            {
                return Unauthorized();
            }

            try
            {
                var result = await _dealerService.GetPaginatedProductsAsync(page, pageSize, category);

                return Ok(new
                {
                    products = result.Products,
                    hasMore = result.HasMore,
                    nextPage = result.NextPage,
                    totalCount = result.TotalCount,
                    totalPages = result.TotalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated products");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
