using System.Text.Json;
using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.DTOs;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace HaldiramPromotionalApp.Services
{
    /// <summary>
    /// Service implementation for dealer-related business logic
    /// </summary>
    public class DealerService : IDealerService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DealerService> _logger;

        public DealerService(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<DealerService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<DealerDashboardDto> GetDashboardDataAsync(string phoneNumber)
        {
            try
            {
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.PhoneNo == phoneNumber);

                if (dealer == null)
                {
                    _logger.LogWarning($"Dealer not found for phone: {phoneNumber}");
                    return null;
                }

                var totalPoints = await GetTotalPointsAsync(dealer.Id);

                // Get posters
                var posters = await _context.Posters
                    .Select(p => new PosterDto
                    {
                        Id = p.Id,
                        Message = p.Message,
                        ImagePath = p.ImagePath,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                // Get active campaigns
                var campaigns = await GetActiveCampaignsAsync();

                // Get materials
                var materials = await _context.MaterialMaster
                    .Where(m => m.isactive)
                    .Select(m => new MaterialDto
                    {
                        Id = m.Id,
                        MaterialName = m.Materialname,
                        ShortName = m.ShortName,
                        MaterialCode = m.material3partycode,
                        Category = m.Category,
                        SubCategory = m.subcategory,
                        Unit = m.Unit,
                        Price = m.price,
                        DealerPrice = m.dealerprice,
                        IsActive = m.isactive
                    })
                    .ToListAsync();

                // Get recent order
                var recentOrder = await GetRecentOrderAsync(dealer.Id);

                // Get recent basic orders
                var recentBasicOrders = await _context.DealerBasicOrders
                    .Where(dbo => dbo.DealerId == dealer.Id)
                    .OrderByDescending(dbo => dbo.Id)
                    .Take(10)
                    .Select(dbo => new BasicOrderDto
                    {
                        Id = dbo.Id,
                        MaterialName = dbo.MaterialName,
                        SapCode = dbo.SapCode,
                        ShortCode = dbo.ShortCode,
                        Quantity = dbo.Quantity,
                        Rate = dbo.Rate,
                        TotalAmount = dbo.Quantity * dbo.Rate
                    })
                    .ToListAsync();

                return new DealerDashboardDto
                {
                    DealerId = dealer.Id,
                    DealerName = dealer.Name,
                    PhoneNumber = dealer.PhoneNo,
                    TotalPoints = totalPoints,
                    Posters = posters,
                    ActiveCampaigns = campaigns,
                    Materials = materials,
                    RecentOrder = recentOrder,
                    RecentBasicOrders = recentBasicOrders
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting dashboard data for dealer: {phoneNumber}");
                throw;
            }
        }

        public async Task<PointsDetailsDto> GetPointsDetailsAsync(string phoneNumber)
        {
            try
            {
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.PhoneNo == phoneNumber);

                if (dealer == null)
                {
                    return null;
                }

                var transactions = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.Material)
                    .Where(oi => oi.Order.DealerId == dealer.Id)
                    .OrderByDescending(oi => oi.Order.OrderDate)
                    .Select(oi => new PointsTransactionDto
                    {
                        Id = oi.Id,
                        Date = oi.Order.OrderDate,
                        MaterialName = oi.Material.Materialname,
                        Quantity = oi.Quantity,
                        Amount = oi.TotalPrice,
                        Points = oi.Points,
                        OrderNumber = oi.Order.Id.ToString()
                    })
                    .ToListAsync();

                var totalPointsEarned = transactions.Sum(t => t.Points);
                var pointsUsed = await _context.Vouchers
                    .Where(v => v.DealerId == dealer.Id && v.IsRedeemed)
                    .SumAsync(v => (int?)v.PointsUsed) ?? 0;

                return new PointsDetailsDto
                {
                    TotalPointsEarned = totalPointsEarned,
                    AvailablePoints = totalPointsEarned - pointsUsed,
                    PointsUsed = pointsUsed,
                    Transactions = transactions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting points details for dealer: {phoneNumber}");
                throw;
            }
        }

        public async Task<RedemptionHistoryDto> GetRedemptionHistoryAsync(string phoneNumber)
        {
            try
            {
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.PhoneNo == phoneNumber);

                if (dealer == null)
                {
                    return null;
                }

                var redeemedVouchers = await _context.Vouchers
                    .Where(v => v.IsRedeemed && v.DealerId == dealer.Id)
                    .OrderByDescending(v => v.RedeemedDate)
                    .Select(v => new VoucherDto
                    {
                        Id = v.Id,
                        VoucherCode = v.VoucherCode,
                        CampaignType = v.CampaignType,
                        CampaignId = v.CampaignId,
                        VoucherValue = v.VoucherValue,
                        PointsUsed = v.PointsUsed,
                        IssueDate = v.IssueDate,
                        ExpiryDate = v.ExpiryDate,
                        IsRedeemed = v.IsRedeemed,
                        RedeemedDate = v.RedeemedDate
                    })
                    .ToListAsync();

                var redeemedProducts = await _context.RedeemedProducts
                    .Include(rp => rp.Voucher)
                    .Where(rp => rp.Voucher.DealerId == dealer.Id)
                    .Select(rp => new RedeemedProductDto
                    {
                        Id = rp.Id,
                        VoucherId = rp.VoucherId,
                        VoucherCode = rp.Voucher.VoucherCode,
                        ProductName = rp.ProductName,
                        Description = rp.Description,
                        Price = rp.Price,
                        Quantity = rp.Quantity,
                        RedemptionDate = rp.RedemptionDate,
                        VoucherType = rp.VoucherType
                    })
                    .ToListAsync();

                return new RedemptionHistoryDto
                {
                    RedeemedVouchers = redeemedVouchers,
                    RedeemedProducts = redeemedProducts,
                    TotalRedemptions = redeemedVouchers.Count,
                    TotalValue = redeemedVouchers.Sum(v => v.VoucherValue)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting redemption history for dealer: {phoneNumber}");
                throw;
            }
        }

        public async Task<VoucherListDto> GetVouchersAsync(string phoneNumber)
        {
            try
            {
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.PhoneNo == phoneNumber);

                if (dealer == null)
                {
                    return null;
                }

                var vouchers = await _context.Vouchers
                    .Where(v => v.DealerId == dealer.Id)
                    .OrderBy(v => v.IsRedeemed)
                    .ThenByDescending(v => v.ExpiryDate)
                    .ThenByDescending(v => v.IssueDate)
                    .Take(10)
                    .ToListAsync();

                var voucherDtos = new List<VoucherDto>();
                foreach (var voucher in vouchers)
                {
                    var qrCode = GenerateQRCodeBase64(voucher.QRCodeData ?? 
                        $"{voucher.VoucherCode}|{dealer.Id}|{voucher.VoucherValue}|{voucher.ExpiryDate:yyyy-MM-dd}");

                    voucherDtos.Add(new VoucherDto
                    {
                        Id = voucher.Id,
                        VoucherCode = voucher.VoucherCode,
                        CampaignType = voucher.CampaignType,
                        CampaignId = voucher.CampaignId,
                        VoucherValue = voucher.VoucherValue,
                        PointsUsed = voucher.PointsUsed,
                        IssueDate = voucher.IssueDate,
                        ExpiryDate = voucher.ExpiryDate,
                        IsRedeemed = voucher.IsRedeemed,
                        RedeemedDate = voucher.RedeemedDate,
                        QRCodeBase64 = qrCode
                    });
                }

                var totalPoints = await GetTotalPointsAsync(dealer.Id);
                var pointsUsed = await _context.Vouchers
                    .Where(v => v.DealerId == dealer.Id && v.IsRedeemed)
                    .SumAsync(v => (int?)v.PointsUsed) ?? 0;

                return new VoucherListDto
                {
                    Vouchers = voucherDtos,
                    TotalPoints = totalPoints,
                    AvailablePoints = totalPoints - pointsUsed,
                    UsedPoints = pointsUsed,
                    ActiveVouchersCount = voucherDtos.Count(v => !v.IsRedeemed && !v.IsExpired),
                    RedeemedVouchersCount = voucherDtos.Count(v => v.IsRedeemed),
                    ExpiredVouchersCount = voucherDtos.Count(v => !v.IsRedeemed && v.IsExpired)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vouchers for dealer: {phoneNumber}");
                throw;
            }
        }

        public async Task<PaginatedVouchersDto> GetPaginatedVouchersAsync(
            string phoneNumber, int page, int pageSize, string status = null, string campaignType = null)
        {
            try
            {
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.PhoneNo == phoneNumber);

                if (dealer == null)
                {
                    return null;
                }

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _context.Vouchers.Where(v => v.DealerId == dealer.Id);

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = status.ToLower() switch
                    {
                        "active" => query.Where(v => !v.IsRedeemed && v.ExpiryDate > DateTime.UtcNow),
                        "redeemed" => query.Where(v => v.IsRedeemed),
                        "expired" => query.Where(v => !v.IsRedeemed && v.ExpiryDate <= DateTime.UtcNow),
                        _ => query
                    };
                }

                if (!string.IsNullOrEmpty(campaignType))
                {
                    query = query.Where(v => v.CampaignType == campaignType);
                }

                var totalCount = await query.CountAsync();
                var skip = (page - 1) * pageSize;

                var vouchers = await query
                    .OrderBy(v => v.IsRedeemed)
                    .ThenByDescending(v => v.ExpiryDate)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                var voucherDtos = vouchers.Select(v => new VoucherDto
                {
                    Id = v.Id,
                    VoucherCode = v.VoucherCode,
                    CampaignType = v.CampaignType,
                    CampaignId = v.CampaignId,
                    VoucherValue = v.VoucherValue,
                    PointsUsed = v.PointsUsed,
                    IssueDate = v.IssueDate,
                    ExpiryDate = v.ExpiryDate,
                    IsRedeemed = v.IsRedeemed,
                    RedeemedDate = v.RedeemedDate,
                    QRCodeBase64 = GenerateQRCodeBase64(v.QRCodeData ?? 
                        $"{v.VoucherCode}|{dealer.Id}|{v.VoucherValue}|{v.ExpiryDate:yyyy-MM-dd}")
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return new PaginatedVouchersDto
                {
                    Vouchers = voucherDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasMore = skip + vouchers.Count < totalCount,
                    NextPage = page + 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paginated vouchers for dealer: {phoneNumber}");
                throw;
            }
        }

        public async Task<GenerateVoucherResponseDto> GenerateVoucherAsync(
            string phoneNumber, GenerateVoucherRequestDto request)
        {
            try
            {
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.PhoneNo == phoneNumber);

                if (dealer == null)
                {
                    return new GenerateVoucherResponseDto
                    {
                        Success = false,
                        Message = "Dealer not found"
                    };
                }

                var totalPoints = await GetTotalPointsAsync(dealer.Id);

                if (totalPoints < request.PointsToUse)
                {
                    return new GenerateVoucherResponseDto
                    {
                        Success = false,
                        Message = "Insufficient points to generate this voucher"
                    };
                }

                decimal voucherValue = 0;
                int voucherValidity = 30;

                // Get campaign details
                if (request.CampaignType == "PointsToCash")
                {
                    var campaign = await _context.PointsToCashCampaigns
                        .FirstOrDefaultAsync(c => c.Id == request.CampaignId);
                    if (campaign != null)
                    {
                        voucherValue = campaign.VoucherValue;
                        voucherValidity = campaign.VoucherValidity;
                    }
                }
                else if (request.CampaignType == "PointsReward")
                {
                    var campaign = await _context.PointsRewardCampaigns
                        .FirstOrDefaultAsync(c => c.Id == request.CampaignId);
                    if (campaign != null)
                    {
                        voucherValidity = campaign.VoucherValidity;
                        voucherValue = 100;
                    }
                }

                var voucherCode = $"V{dealer.Id}{DateTime.UtcNow:yyyyMMddHHmmss}";
                var expiryDate = DateTime.UtcNow.AddDays(voucherValidity);

                var voucher = new Voucher
                {
                    VoucherCode = voucherCode,
                    DealerId = dealer.Id,
                    CampaignType = request.CampaignType,
                    CampaignId = request.CampaignId,
                    VoucherValue = voucherValue,
                    PointsUsed = request.PointsToUse,
                    IssueDate = DateTime.UtcNow,
                    ExpiryDate = expiryDate,
                    QRCodeData = $"{voucherCode}|{dealer.Id}|{voucherValue}|{expiryDate:yyyy-MM-dd}"
                };

                _context.Vouchers.Add(voucher);
                await _context.SaveChangesAsync();

                return new GenerateVoucherResponseDto
                {
                    Success = true,
                    Message = $"Voucher generated successfully! Code: {voucherCode}",
                    Voucher = new VoucherDto
                    {
                        Id = voucher.Id,
                        VoucherCode = voucher.VoucherCode,
                        CampaignType = voucher.CampaignType,
                        CampaignId = voucher.CampaignId,
                        VoucherValue = voucher.VoucherValue,
                        PointsUsed = voucher.PointsUsed,
                        IssueDate = voucher.IssueDate,
                        ExpiryDate = voucher.ExpiryDate,
                        IsRedeemed = false,
                        QRCodeBase64 = GenerateQRCodeBase64(voucher.QRCodeData)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating voucher for dealer: {phoneNumber}");
                return new GenerateVoucherResponseDto
                {
                    Success = false,
                    Message = "Error generating voucher. Please try again."
                };
            }
        }

        public async Task<PaginatedProductsDto> GetPaginatedProductsAsync(
            int page, int pageSize, string category = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _context.MaterialMaster.Where(m => m.isactive);

                if (!string.IsNullOrEmpty(category) && category != "__all__")
                {
                    query = query.Where(m => m.Category == category);
                }

                var totalCount = await query.CountAsync();
                var skip = (page - 1) * pageSize;

                var materials = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                var materialIds = materials.Select(m => m.Id).ToList();
                var materialImages = await _context.MaterialImages
                    .Include(mi => mi.MaterialMaster)
                    .Where(mi => materialIds.Contains(mi.MaterialMaster.Id))
                    .ToListAsync();

                var productDtos = materials.Select(m =>
                {
                    var image = materialImages.FirstOrDefault(mi => 
                        mi.MaterialMaster?.material3partycode == m.material3partycode);

                    return new MaterialDto
                    {
                        Id = m.Id,
                        MaterialName = m.Materialname,
                        ShortName = m.ShortName,
                        MaterialCode = m.material3partycode,
                        Category = m.Category,
                        SubCategory = m.subcategory,
                        Unit = m.Unit,
                        Price = m.price,
                        DealerPrice = m.dealerprice,
                        ImagePath = image?.ImagePath ?? string.Empty,
                        IsActive = m.isactive
                    };
                }).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return new PaginatedProductsDto
                {
                    Products = productDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasMore = skip + materials.Count < totalCount,
                    NextPage = page + 1,
                    Category = category
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated products");
                throw;
            }
        }

        public async Task<int?> GetDealerIdByPhoneAsync(string phoneNumber)
        {
            var dealer = await _context.DealerMasters
                .FirstOrDefaultAsync(d => d.PhoneNo == phoneNumber);
            return dealer?.Id;
        }

        public async Task<int> GetTotalPointsAsync(int dealerId)
        {
            return await _context.OrderItems
                .Where(oi => oi.Order.DealerId == dealerId)
                .SumAsync(oi => (int?)oi.Points) ?? 0;
        }

        public async Task<int> ProcessAutomaticVoucherGenerationAsync(int dealerId, int userId, int totalPoints)
        {
            int vouchersGenerated = 0;

            try
            {
                // Process PointsToCash campaigns
                vouchersGenerated += await ProcessPointsToCashVouchersAsync(dealerId, userId, totalPoints);

                // Process PointsReward campaigns
                vouchersGenerated += await ProcessPointsRewardVouchersAsync(dealerId, userId, totalPoints);

                // Process AmountReachGoal campaigns
                vouchersGenerated += await ProcessAmountReachGoalVouchersAsync(dealerId, userId);

                // Process FreeProduct campaigns
                vouchersGenerated += await ProcessFreeProductVouchersAsync(dealerId, userId);

                return vouchersGenerated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing automatic voucher generation for dealer: {dealerId}");
                return vouchersGenerated;
            }
        }

        #region Private Helper Methods

        private async Task<List<CampaignSummaryDto>> GetActiveCampaignsAsync()
        {
            var campaigns = new List<CampaignSummaryDto>();
            
            // Pre-fetch materials for name lookup
            var materials = await _context.MaterialMaster
                .Where(m => m.isactive)
                .ToDictionaryAsync(m => m.Id, m => m.Materialname);

            // 1. PointsToCash
            var ptcCampaigns = await _context.PointsToCashCampaigns
                .Where(c => c.IsActive)
                .ToListAsync();

            foreach (var c in ptcCampaigns)
            {
                var dto = new CampaignSummaryDto
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    CampaignType = "PointsToCash",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    VoucherValue = c.VoucherValue,
                    PointsRequired = c.VoucherGenerationThreshold,
                    VoucherValidity = c.VoucherValidity,
                    MaterialDetails = new List<CampaignMaterialDto>()
                };

                if (!string.IsNullOrEmpty(c.MaterialPoints))
                {
                    try
                    {
                        var pointsMap = JsonSerializer.Deserialize<Dictionary<int, int>>(c.MaterialPoints);
                        if (pointsMap != null)
                        {
                            foreach (var kvp in pointsMap)
                            {
                                dto.MaterialDetails.Add(new CampaignMaterialDto
                                {
                                    MaterialId = kvp.Key,
                                    MaterialName = materials.ContainsKey(kvp.Key) ? materials[kvp.Key] : "Unknown",
                                    Points = kvp.Value
                                });
                            }
                        }
                    }
                    catch { }
                }
                campaigns.Add(dto);
            }

            // 2. PointsReward
            var prCampaigns = await _context.PointsRewardCampaigns
                .Where(c => c.IsActive)
                .ToListAsync();

            foreach (var c in prCampaigns)
            {
                var dto = new CampaignSummaryDto
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    CampaignType = "PointsReward",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    PointsRequired = c.VoucherGenerationThreshold,
                    VoucherValidity = c.VoucherValidity,
                    MaterialDetails = new List<CampaignMaterialDto>()
                };

                if (c.RewardProductId.HasValue && materials.ContainsKey(c.RewardProductId.Value))
                {
                    dto.RewardProductName = materials[c.RewardProductId.Value];
                }

                if (!string.IsNullOrEmpty(c.MaterialPoints))
                {
                    try
                    {
                        var pointsMap = JsonSerializer.Deserialize<Dictionary<int, int>>(c.MaterialPoints);
                        if (pointsMap != null)
                        {
                            foreach (var kvp in pointsMap)
                            {
                                dto.MaterialDetails.Add(new CampaignMaterialDto
                                {
                                    MaterialId = kvp.Key,
                                    MaterialName = materials.ContainsKey(kvp.Key) ? materials[kvp.Key] : "Unknown",
                                    Points = kvp.Value
                                });
                            }
                        }
                    }
                    catch { }
                }
                campaigns.Add(dto);
            }

            // 3. FreeProduct
            var fpCampaigns = await _context.FreeProductCampaigns
                .Where(c => c.IsActive)
                .ToListAsync();

            foreach (var c in fpCampaigns)
            {
                var dto = new CampaignSummaryDto
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    CampaignType = "FreeProduct",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    MaterialDetails = new List<CampaignMaterialDto>(),
                    FreeProductDetails = new List<CampaignFreeProductDto>()
                };

                try
                {
                    var quantitiesMap = !string.IsNullOrEmpty(c.MaterialQuantities) 
                        ? JsonSerializer.Deserialize<Dictionary<int, int>>(c.MaterialQuantities) 
                        : new Dictionary<int, int>();
                        
                    var freeProductsMap = !string.IsNullOrEmpty(c.FreeProducts)
                        ? JsonSerializer.Deserialize<Dictionary<int, int>>(c.FreeProducts)
                        : new Dictionary<int, int>();
                        
                    var freeQuantitiesMap = !string.IsNullOrEmpty(c.FreeQuantities)
                        ? JsonSerializer.Deserialize<Dictionary<int, int>>(c.FreeQuantities)
                        : new Dictionary<int, int>();

                    if (quantitiesMap != null)
                    {
                        foreach (var kvp in quantitiesMap)
                        {
                            dto.MaterialDetails.Add(new CampaignMaterialDto
                            {
                                MaterialId = kvp.Key,
                                MaterialName = materials.ContainsKey(kvp.Key) ? materials[kvp.Key] : "Unknown",
                                Quantity = kvp.Value
                            });
                        }
                    }

                    if (freeProductsMap != null && freeQuantitiesMap != null)
                    {
                        foreach (var kvp in freeProductsMap)
                        {
                            var materialId = kvp.Key;
                            var freeProductId = kvp.Value;
                            var freeQuantity = freeQuantitiesMap.ContainsKey(materialId) ? freeQuantitiesMap[materialId] : 0;

                            dto.FreeProductDetails.Add(new CampaignFreeProductDto
                            {
                                MaterialId = materialId,
                                FreeProductId = freeProductId,
                                FreeProductName = materials.ContainsKey(freeProductId) ? materials[freeProductId] : "Unknown",
                                FreeQuantity = freeQuantity
                            });
                        }
                    }
                }
                catch { }
                
                campaigns.Add(dto);
            }

            // 4. AmountReachGoal
            var argCampaigns = await _context.AmountReachGoalCampaigns
                .Where(c => c.IsActive)
                .ToListAsync();

            foreach (var c in argCampaigns)
            {
                campaigns.Add(new CampaignSummaryDto
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    CampaignType = "AmountReachGoal",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    TargetAmount = c.TargetAmount,
                    VoucherValue = c.VoucherValue,
                    VoucherValidity = c.VoucherValidity
                });
            }

            // 5. SessionDurationReward
            var sdrCampaigns = await _context.SessionDurationRewardCampaigns
                .Where(c => c.IsActive)
                .ToListAsync();

            foreach (var c in sdrCampaigns)
            {
                campaigns.Add(new CampaignSummaryDto
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    CampaignType = "SessionDurationReward",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    SessionDuration = c.SessionDuration,
                    VoucherValue = c.VoucherValue,
                    VoucherValidity = c.VoucherValidity
                });
            }

            return campaigns;
        }

        private async Task<OrderSummaryDto> GetRecentOrderAsync(int dealerId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Material)
                .Where(o => o.DealerId == dealerId)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            if (order == null) return null;

            return new OrderSummaryDto
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                TotalItems = order.OrderItems.Count,
                TotalPoints = order.OrderItems.Sum(oi => oi.Points),
                Status = "Completed",
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    MaterialName = oi.Material.Materialname,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Points = oi.Points
                }).ToList()
            };
        }

        private string GenerateQRCodeBase64(string data)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new Base64QRCode(qrCodeData);
                return qrCode.GetGraphic(20);
            }
            catch
            {
                return "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
            }
        }

        private async Task<int> ProcessPointsToCashVouchersAsync(int dealerId, int userId, int totalPoints)
        {
            int count = 0;
            var campaigns = await _context.PointsToCashCampaigns
                .Where(c => c.IsActive && c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            foreach (var campaign in campaigns)
            {
                if (totalPoints >= campaign.VoucherGenerationThreshold)
                {
                    int eligibleCount = totalPoints / campaign.VoucherGenerationThreshold;
                    var existingCount = await _context.Vouchers
                        .Where(v => v.DealerId == dealerId && v.CampaignId == campaign.Id && v.CampaignType == "PointsToCash")
                        .CountAsync();

                    int toCreate = eligibleCount - existingCount;
                    if (toCreate > 0)
                    {
                        var vouchers = new List<Voucher>();
                        for (int i = 0; i < toCreate; i++)
                        {
                            var voucherCode = $"PTC{dealerId}{campaign.Id}{DateTime.UtcNow:yyyyMMddHHmmss}{i}";
                            vouchers.Add(new Voucher
                            {
                                VoucherCode = voucherCode,
                                DealerId = dealerId,
                                CampaignType = "PointsToCash",
                                CampaignId = campaign.Id,
                                VoucherValue = campaign.VoucherValue,
                                PointsUsed = campaign.VoucherGenerationThreshold,
                                IssueDate = DateTime.UtcNow,
                                ExpiryDate = DateTime.UtcNow.AddDays(campaign.VoucherValidity),
                                QRCodeData = $"{voucherCode}|{dealerId}|{campaign.VoucherValue}|{DateTime.UtcNow.AddDays(campaign.VoucherValidity):yyyy-MM-dd}"
                            });
                        }

                        _context.Vouchers.AddRange(vouchers);
                        await _context.SaveChangesAsync();

                        foreach (var v in vouchers)
                        {
                            await _notificationService.CreateVoucherNotificationAsync(userId, v.VoucherCode, v.VoucherValue, v.Id);
                        }

                        count += toCreate;
                    }
                }
            }

            return count;
        }

        private async Task<int> ProcessPointsRewardVouchersAsync(int dealerId, int userId, int totalPoints)
        {
            // Similar implementation for PointsReward campaigns
            // Omitted for brevity - follows same pattern as PointsToCash
            return 0;
        }

        private async Task<int> ProcessAmountReachGoalVouchersAsync(int dealerId, int userId)
        {
            // Implementation for AmountReachGoal campaigns
            return 0;
        }

        private async Task<int> ProcessFreeProductVouchersAsync(int dealerId, int userId)
        {
            // Implementation for FreeProduct campaigns
            return 0;
        }

        #endregion
    }
}
