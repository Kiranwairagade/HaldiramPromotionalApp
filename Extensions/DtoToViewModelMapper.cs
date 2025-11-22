using HaldiramPromotionalApp.DTOs;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;

namespace HaldiramPromotionalApp.Extensions
{
    /// <summary>
    /// Extension methods to map DTOs to ViewModels
    /// This allows us to use clean DTOs in the service layer while maintaining compatibility with existing views
    /// </summary>
    public static class DtoToViewModelMapper
    {
        /// <summary>
        /// Map DealerDashboardDto to DealerDashboardViewModel
        /// </summary>
        public static DealerDashboardViewModel ToViewModel(this DealerDashboardDto dto)
        {
            if (dto == null) return null;

            return new DealerDashboardViewModel
            {
                TotalPoints = dto.TotalPoints,
                Posters = dto.Posters?.Select(p => new Poster
                {
                    Id = p.Id,
                    Message = p.Message,
                    ImagePath = p.ImagePath,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? new List<Poster>(),
                
                Materials = dto.Materials?.Select(m => new MaterialMaster
                {
                    Id = m.Id,
                    Materialname = m.MaterialName,
                    ShortName = m.ShortName,
                    material3partycode = m.MaterialCode,
                    Category = m.Category,
                    subcategory = m.SubCategory,
                    Unit = m.Unit,
                    price = m.Price,
                    dealerprice = m.DealerPrice,
                    isactive = m.IsActive
                }).ToList() ?? new List<MaterialMaster>(),
                
                MaterialImages = new List<MaterialImage>(), // Can be populated if needed
                
                RecentOrder = dto.RecentOrder != null ? new Order
                {
                    Id = dto.RecentOrder.OrderId,
                    OrderDate = dto.RecentOrder.OrderDate,
                    TotalAmount = dto.RecentOrder.TotalAmount,
                    OrderItems = dto.RecentOrder.Items?.Select(i => new OrderItem
                    {
                        Id = i.Id,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice,
                        Points = i.Points,
                        Material = new MaterialMaster { Materialname = i.MaterialName }
                    }).ToList() ?? new List<OrderItem>()
                } : null,
                
                RecentDealerBasicOrders = dto.RecentBasicOrders?.Select(o => new DealerBasicOrder
                {
                    Id = o.Id,
                    MaterialName = o.MaterialName,
                    SapCode = o.SapCode,
                    ShortCode = o.ShortCode,
                    Quantity = o.Quantity,
                    Rate = o.Rate
                }).ToList() ?? new List<DealerBasicOrder>(),
                
                Campaigns = dto.ActiveCampaigns.ToViewCampaignsViewModel()
            };
        }

        /// <summary>
        /// Map VoucherListDto to VoucherViewModel
        /// </summary>
        public static VoucherViewModel ToViewModel(this VoucherListDto dto)
        {
            if (dto == null) return null;

            var voucherQRCodeData = new Dictionary<int, string>();
            var vouchers = new List<Voucher>();

            foreach (var vDto in dto.Vouchers ?? new List<VoucherDto>())
            {
                vouchers.Add(new Voucher
                {
                    Id = vDto.Id,
                    VoucherCode = vDto.VoucherCode,
                    CampaignType = vDto.CampaignType,
                    CampaignId = vDto.CampaignId,
                    VoucherValue = vDto.VoucherValue,
                    PointsUsed = vDto.PointsUsed,
                    IssueDate = vDto.IssueDate,
                    ExpiryDate = vDto.ExpiryDate,
                    IsRedeemed = vDto.IsRedeemed,
                    RedeemedDate = vDto.RedeemedDate,
                    QRCodeData = vDto.QRCodeBase64
                });

                if (!string.IsNullOrEmpty(vDto.QRCodeBase64))
                {
                    voucherQRCodeData[vDto.Id] = vDto.QRCodeBase64;
                }
            }

            return new VoucherViewModel
            {
                Vouchers = vouchers,
                TotalPoints = dto.TotalPoints,
                VoucherQRCodeData = voucherQRCodeData
            };
        }

        /// <summary>
        /// Map RedemptionHistoryDto to VoucherViewModel
        /// </summary>
        public static VoucherViewModel ToViewModel(this RedemptionHistoryDto dto)
        {
            if (dto == null) return null;

            return new VoucherViewModel
            {
                RedemptionHistory = dto.RedeemedVouchers?.Select(v => new Voucher
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
                }).ToList() ?? new List<Voucher>(),
                
                RedeemedProducts = dto.RedeemedProducts?.Select(p => new RedeemedProduct
                {
                    Id = p.Id,
                    VoucherId = p.VoucherId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    RedemptionDate = p.RedemptionDate,
                    VoucherType = p.VoucherType,
                    Voucher = new Voucher { VoucherCode = p.VoucherCode }
                }).ToList() ?? new List<RedeemedProduct>()
            };
        }

        /// <summary>
        /// Map PointsDetailsDto to view data
        /// </summary>
        public static List<OrderItem> ToOrderItemsList(this PointsDetailsDto dto)
        {
            if (dto == null || dto.Transactions == null) return new List<OrderItem>();

            return dto.Transactions.Select(t => new OrderItem
            {
                Id = t.Id,
                Quantity = t.Quantity,
                TotalPrice = t.Amount,
                Points = t.Points,
                Material = new MaterialMaster { Materialname = t.MaterialName },
                Order = new Order
                {
                    OrderDate = t.Date,
                    Id = int.TryParse(t.OrderNumber, out var orderId) ? orderId : 0
                }
            }).ToList();
        }
        /// <summary>
        /// Map list of CampaignSummaryDto to ViewCampaignsViewModel
        /// </summary>
        public static ViewCampaignsViewModel ToViewCampaignsViewModel(this List<CampaignSummaryDto> dtos)
        {
            var viewModel = new ViewCampaignsViewModel();
            if (dtos == null) return viewModel;

            // 1. PointsToCash
            viewModel.DetailedPointsToCashCampaigns = dtos
                .Where(c => c.CampaignType == "PointsToCash")
                .Select(c => new DetailedPointsToCashCampaign
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    VoucherValue = c.VoucherValue ?? 0,
                    VoucherGenerationThreshold = c.PointsRequired ?? 0,
                    VoucherValidity = c.VoucherValidity,
                    MaterialDetails = c.MaterialDetails?.Select(m => new MaterialDetail
                    {
                        MaterialId = m.MaterialId,
                        MaterialName = m.MaterialName,
                        Points = m.Points
                    }).ToList() ?? new List<MaterialDetail>()
                }).ToList();

            // 2. PointsReward
            viewModel.DetailedPointsRewardCampaigns = dtos
                .Where(c => c.CampaignType == "PointsReward")
                .Select(c => new DetailedPointsRewardCampaign
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    VoucherGenerationThreshold = c.PointsRequired ?? 0,
                    VoucherValidity = c.VoucherValidity,
                    RewardProduct = !string.IsNullOrEmpty(c.RewardProductName) 
                        ? new Product { ProductName = c.RewardProductName } 
                        : null,
                    MaterialDetails = c.MaterialDetails?.Select(m => new MaterialDetail
                    {
                        MaterialId = m.MaterialId,
                        MaterialName = m.MaterialName,
                        Points = m.Points
                    }).ToList() ?? new List<MaterialDetail>()
                }).ToList();

            // 3. FreeProduct
            viewModel.DetailedFreeProductCampaigns = dtos
                .Where(c => c.CampaignType == "FreeProduct")
                .Select(c => new DetailedFreeProductCampaign
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    MaterialDetails = c.MaterialDetails?.Select(m => new MaterialDetail
                    {
                        MaterialId = m.MaterialId,
                        MaterialName = m.MaterialName,
                        Quantity = m.Quantity
                    }).ToList() ?? new List<MaterialDetail>(),
                    FreeProductDetails = c.FreeProductDetails?.ToDictionary(
                        fp => fp.MaterialId,
                        fp => new MaterialFreeProductDetail
                        {
                            FreeProductId = fp.FreeProductId,
                            FreeProductName = fp.FreeProductName,
                            FreeQuantity = fp.FreeQuantity
                        }) ?? new Dictionary<int, MaterialFreeProductDetail>()
                }).ToList();

            // 4. AmountReachGoal
            viewModel.AmountReachGoalCampaigns = dtos
                .Where(c => c.CampaignType == "AmountReachGoal")
                .Select(c => new AmountReachGoalCampaign
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    TargetAmount = c.TargetAmount,
                    VoucherValue = c.VoucherValue ?? 0,
                    VoucherValidity = c.VoucherValidity
                }).ToList();

            // 5. SessionDurationReward
            viewModel.SessionDurationRewardCampaigns = dtos
                .Where(c => c.CampaignType == "SessionDurationReward")
                .Select(c => new SessionDurationRewardCampaign
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    ImagePath = c.ImagePath,
                    IsActive = c.IsActive,
                    SessionDuration = c.SessionDuration,
                    VoucherValue = c.VoucherValue ?? 0,
                    VoucherValidity = c.VoucherValidity
                }).ToList();

            return viewModel;
        }
    }
}
