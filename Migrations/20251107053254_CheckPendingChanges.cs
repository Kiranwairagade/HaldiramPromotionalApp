using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class CheckPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Skip MaterialMaster table creation as it already exists
            
            // Check if tables exist before creating them
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
                BEGIN
                    CREATE TABLE [Products] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [ProductName] nvarchar(max) NOT NULL,
                        [Price] decimal(18,2) NOT NULL,
                        [Category] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PointsToCashCampaigns')
                BEGIN
                    CREATE TABLE [PointsToCashCampaigns] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [CampaignName] nvarchar(max) NOT NULL,
                        [MaterialType] nvarchar(max) NOT NULL,
                        [StartDate] datetime2 NOT NULL,
                        [EndDate] datetime2 NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [VoucherGenerationThreshold] int NOT NULL,
                        [VoucherValue] decimal(18,2) NOT NULL,
                        [VoucherValidity] int NOT NULL,
                        [Materials] nvarchar(max) NULL,
                        [MaterialPoints] nvarchar(max) NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_PointsToCashCampaigns] PRIMARY KEY ([Id])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PointsRewardCampaigns')
                BEGIN
                    CREATE TABLE [PointsRewardCampaigns] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [CampaignName] nvarchar(max) NOT NULL,
                        [MaterialType] nvarchar(max) NOT NULL,
                        [StartDate] datetime2 NOT NULL,
                        [EndDate] datetime2 NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [VoucherGenerationThreshold] int NOT NULL,
                        [VoucherValidity] int NOT NULL,
                        [Materials] nvarchar(max) NULL,
                        [MaterialPoints] nvarchar(max) NULL,
                        [RewardProductId] int NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_PointsRewardCampaigns] PRIMARY KEY ([Id])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FreeProductCampaigns')
                BEGIN
                    CREATE TABLE [FreeProductCampaigns] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [CampaignName] nvarchar(max) NOT NULL,
                        [MaterialType] nvarchar(max) NOT NULL,
                        [StartDate] datetime2 NOT NULL,
                        [EndDate] datetime2 NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [Materials] nvarchar(max) NULL,
                        [MaterialQuantities] nvarchar(max) NULL,
                        [FreeProducts] nvarchar(max) NULL,
                        [FreeQuantities] nvarchar(max) NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_FreeProductCampaigns] PRIMARY KEY ([Id])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AmountReachGoalCampaigns')
                BEGIN
                    CREATE TABLE [AmountReachGoalCampaigns] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [CampaignName] nvarchar(max) NOT NULL,
                        [StartDate] datetime2 NOT NULL,
                        [EndDate] datetime2 NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [TargetAmount] decimal(18,2) NOT NULL,
                        [VoucherValue] decimal(18,2) NOT NULL,
                        [VoucherValidity] int NOT NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_AmountReachGoalCampaigns] PRIMARY KEY ([Id])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SessionDurationRewardCampaigns')
                BEGIN
                    CREATE TABLE [SessionDurationRewardCampaigns] (
                        [Id] int IDENTITY(1,1) NOT NULL,
                        [CampaignName] nvarchar(max) NOT NULL,
                        [StartDate] datetime2 NOT NULL,
                        [EndDate] datetime2 NOT NULL,
                        [Description] nvarchar(max) NULL,
                        [SessionDuration] int NOT NULL,
                        [VoucherValue] decimal(18,2) NOT NULL,
                        [VoucherValidity] int NOT NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_SessionDurationRewardCampaigns] PRIMARY KEY ([Id])
                    );
                END

                -- Add foreign key constraint if tables exist
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PointsRewardCampaigns')
                   AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
                   AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_PointsRewardCampaigns_Products_RewardProductId')
                BEGIN
                    ALTER TABLE [PointsRewardCampaigns]
                    ADD CONSTRAINT [FK_PointsRewardCampaigns_Products_RewardProductId]
                    FOREIGN KEY ([RewardProductId]) REFERENCES [Products] ([Id]);
                END

                -- Add index if table exists
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PointsRewardCampaigns')
                   AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PointsRewardCampaigns_RewardProductId')
                BEGIN
                    CREATE INDEX [IX_PointsRewardCampaigns_RewardProductId] ON [PointsRewardCampaigns] ([RewardProductId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Safely drop tables in reverse order
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SessionDurationRewardCampaigns')
                BEGIN
                    DROP TABLE [SessionDurationRewardCampaigns];
                END

                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AmountReachGoalCampaigns')
                BEGIN
                    DROP TABLE [AmountReachGoalCampaigns];
                END

                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FreeProductCampaigns')
                BEGIN
                    DROP TABLE [FreeProductCampaigns];
                END

                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PointsRewardCampaigns')
                BEGIN
                    DROP TABLE [PointsRewardCampaigns];
                END

                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PointsToCashCampaigns')
                BEGIN
                    DROP TABLE [PointsToCashCampaigns];
                END

                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
                BEGIN
                    DROP TABLE [Products];
                END
            ");
        }
    }
}