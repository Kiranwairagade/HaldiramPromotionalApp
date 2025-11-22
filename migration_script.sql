Build started...
Build succeeded.
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AmountReachGoalCampaigns] (
    [Id] int NOT NULL IDENTITY,
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

CREATE TABLE [FreeProductCampaigns] (
    [Id] int NOT NULL IDENTITY,
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

CREATE TABLE [PointsToCashCampaigns] (
    [Id] int NOT NULL IDENTITY,
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

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [ProductName] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);

CREATE TABLE [SessionDurationRewardCampaigns] (
    [Id] int NOT NULL IDENTITY,
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

CREATE TABLE [PointsRewardCampaigns] (
    [Id] int NOT NULL IDENTITY,
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
    CONSTRAINT [PK_PointsRewardCampaigns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PointsRewardCampaigns_Products_RewardProductId] FOREIGN KEY ([RewardProductId]) REFERENCES [Products] ([Id])
);

CREATE INDEX [IX_PointsRewardCampaigns_RewardProductId] ON [PointsRewardCampaigns] ([RewardProductId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107051109_InitialCreateWithoutMaterialMaster', N'9.0.10');


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
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107052929_AddMissingTables', N'9.0.10');


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
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107053254_CheckPendingChanges', N'9.0.10');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FreeProductCampaigns]') AND [c].[name] = N'MaterialType');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [FreeProductCampaigns] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [FreeProductCampaigns] DROP COLUMN [MaterialType];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107061826_RemoveMaterialTypeFromFreeProductCampaign', N'9.0.10');

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PointsToCashCampaigns]') AND [c].[name] = N'MaterialType');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PointsToCashCampaigns] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PointsToCashCampaigns] DROP COLUMN [MaterialType];

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PointsRewardCampaigns]') AND [c].[name] = N'MaterialType');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [PointsRewardCampaigns] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [PointsRewardCampaigns] DROP COLUMN [MaterialType];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107063213_RemoveMaterialTypeFromCampaigns', N'9.0.10');

CREATE TABLE [Posters] (
    [Id] int NOT NULL IDENTITY,
    [ImagePath] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [ShowFrom] datetime2 NOT NULL,
    [ShowUntil] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Posters] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107114927_AddPostersTable', N'9.0.10');

CREATE TABLE [MaterialImages] (
    [Id] int NOT NULL IDENTITY,
    [MaterialMasterId] int NOT NULL,
    [ImagePath] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_MaterialImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MaterialImages_MaterialMaster_MaterialMasterId] FOREIGN KEY ([MaterialMasterId]) REFERENCES [MaterialMaster] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_MaterialImages_MaterialMasterId] ON [MaterialImages] ([MaterialMasterId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251107131030_AddMaterialImagesTable', N'9.0.10');

ALTER TABLE [SessionDurationRewardCampaigns] ADD [ImagePath] nvarchar(max) NULL;

ALTER TABLE [PointsToCashCampaigns] ADD [ImagePath] nvarchar(max) NULL;

ALTER TABLE [PointsRewardCampaigns] ADD [ImagePath] nvarchar(max) NULL;

ALTER TABLE [FreeProductCampaigns] ADD [ImagePath] nvarchar(max) NULL;

ALTER TABLE [AmountReachGoalCampaigns] ADD [ImagePath] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251108102945_AddImagePathToCampaigns', N'9.0.10');

CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [OrderDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [UserId] int NOT NULL,
    [OrderStatus] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [OrderItems] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [MaterialId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderItems_MaterialMaster_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [MaterialMaster] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_OrderItems_MaterialId] ON [OrderItems] ([MaterialId]);

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);

CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251110090158_AddOrdersWithUserReference', N'9.0.10');

ALTER TABLE [Orders] ADD [DealerId] int NOT NULL DEFAULT 0;

CREATE TABLE [DealerMaster] (
    [Id] int NOT NULL IDENTITY,
    [DistributorId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [ContactPerson] nvarchar(200) NOT NULL,
    [RouteCode] nvarchar(50) NOT NULL,
    [Address] nvarchar(500) NOT NULL,
    [PhoneNo] nvarchar(10) NOT NULL,
    [Email] nvarchar(100) NULL,
    CONSTRAINT [PK_DealerMaster] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251111121551_AddDealerIdToOrders', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251111121606_AddDealerMasterTable', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251111123959_MakeDealerIdRequiredInOrder', N'9.0.10');


                DELETE FROM Orders 
                WHERE DealerId IS NOT NULL 
                AND DealerId NOT IN (SELECT Id FROM DealerMasters WHERE Id IS NOT NULL)
            

CREATE INDEX [IX_Orders_DealerId] ON [Orders] ([DealerId]);

ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_DealerMasters_DealerId] FOREIGN KEY ([DealerId]) REFERENCES [DealerMasters] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251111125522_AddForeignKeyToOrdersDealerId', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251112083113_AddPointsToMaterialMaster', N'9.0.10');

CREATE TABLE [MaterialPoints] (
    [Id] int NOT NULL IDENTITY,
    [MaterialId] int NOT NULL,
    [Points] int NOT NULL,
    CONSTRAINT [PK_MaterialPoints] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MaterialPoints_MaterialMaster_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [MaterialMaster] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_MaterialPoints_MaterialId] ON [MaterialPoints] ([MaterialId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251112084716_RemovePointsFromMaterialMasterAndAddMaterialPointsTable', N'9.0.10');


                DELETE FROM Orders 
                WHERE DealerId IS NOT NULL 
                AND DealerId NOT IN (SELECT Id FROM DealerMasters WHERE Id IS NOT NULL)
            


                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_DealerId' AND object_id = OBJECT_ID('Orders'))
                BEGIN
                    CREATE NONCLUSTERED INDEX [IX_Orders_DealerId] ON [Orders] ([DealerId]);
                END
            


                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Orders_DealerMasters_DealerId')
                BEGIN
                    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_DealerMasters_DealerId] FOREIGN KEY ([DealerId]) REFERENCES [DealerMasters] ([Id]) ON DELETE CASCADE;
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251112090248_FixDealerForeignKeyConstraint', N'9.0.10');

DROP TABLE [MaterialPoints];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251112102717_RemoveMaterialPointsTable', N'9.0.10');

ALTER TABLE [OrderItems] ADD [Points] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251112104348_AddPointsToOrderItem', N'9.0.10');

CREATE TABLE [ShopkeeperMasters] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [StoreLocation] nvarchar(500) NOT NULL,
    [StoreType] nvarchar(max) NOT NULL,
    [Username] nvarchar(100) NOT NULL,
    [Password] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(10) NOT NULL,
    CONSTRAINT [PK_ShopkeeperMasters] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113071639_AddShopkeeperMasterTable', N'9.0.10');

CREATE TABLE [Vouchers] (
    [Id] int NOT NULL IDENTITY,
    [VoucherCode] nvarchar(max) NOT NULL,
    [DealerId] int NOT NULL,
    [CampaignType] nvarchar(max) NOT NULL,
    [CampaignId] int NOT NULL,
    [VoucherValue] decimal(18,2) NOT NULL,
    [PointsUsed] int NOT NULL,
    [IssueDate] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [IsRedeemed] bit NOT NULL,
    [RedeemedDate] datetime2 NULL,
    [QRCodeData] nvarchar(max) NULL,
    CONSTRAINT [PK_Vouchers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Vouchers_DealerMasters_DealerId] FOREIGN KEY ([DealerId]) REFERENCES [DealerMasters] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Vouchers_DealerId] ON [Vouchers] ([DealerId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113091504_AddVoucherTable', N'9.0.10');

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [IsRead] bit NOT NULL,
    [RelatedEntityId] int NULL,
    [RelatedEntityType] nvarchar(max) NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251114110128_AddNotificationTable', N'9.0.10');

ALTER TABLE [SessionDurationRewardCampaigns] ADD [DistributorVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [SessionDurationRewardCampaigns] ADD [SalesVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [PointsToCashCampaigns] ADD [DistributorVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [PointsToCashCampaigns] ADD [SalesVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [PointsRewardCampaigns] ADD [DistributorVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [PointsRewardCampaigns] ADD [SalesVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [FreeProductCampaigns] ADD [DistributorVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [FreeProductCampaigns] ADD [SalesVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [AmountReachGoalCampaigns] ADD [DistributorVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [AmountReachGoalCampaigns] ADD [SalesVoucherValue] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251120124355_AddVoucherValueFieldsToCampaigns', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251121050107_RenameShopkeeperVoucherValueToSalesVoucherValue', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251121051548_RenameShopkeeperToSalesVoucherValueColumns', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251121054356_addedcolumn', N'9.0.10');

ALTER TABLE [Vouchers] DROP CONSTRAINT [FK_Vouchers_DealerMasters_DealerId];

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vouchers]') AND [c].[name] = N'DealerId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Vouchers] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Vouchers] ALTER COLUMN [DealerId] int NULL;

ALTER TABLE [Vouchers] ADD [EntityIdentifier] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Vouchers] ADD [EntityType] nvarchar(max) NOT NULL DEFAULT N'';

CREATE TABLE [Customer_Masters] (
    [Id] int NOT NULL IDENTITY,
    [shortname] nvarchar(max) NOT NULL,
    [Name] nvarchar(500) NOT NULL,
    [ContactPerson] nvarchar(500) NOT NULL,
    [Division] nvarchar(max) NOT NULL,
    [accounttype] nvarchar(max) NOT NULL,
    [route] nvarchar(max) NOT NULL,
    [address] nvarchar(max) NOT NULL,
    [phoneno] nvarchar(10) NOT NULL,
    [email] nvarchar(max) NOT NULL,
    [city] nvarchar(max) NOT NULL,
    [PostalCode] nvarchar(6) NOT NULL,
    [country] nvarchar(max) NOT NULL,
    [state] nvarchar(max) NOT NULL,
    [Sequence] int NOT NULL,
    CONSTRAINT [PK_Customer_Masters] PRIMARY KEY ([Id])
);

CREATE TABLE [EmpToCustMaps] (
    [id] int NOT NULL IDENTITY,
    [empl] nvarchar(50) NOT NULL,
    [phoneno] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_EmpToCustMaps] PRIMARY KEY ([id])
);

CREATE TABLE [Cust2EmpMaps] (
    [id] int NOT NULL IDENTITY,
    [empt2custid] int NOT NULL,
    [customer] nvarchar(50) NOT NULL,
    [phone] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Cust2EmpMaps] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Cust2EmpMaps_EmpToCustMaps_empt2custid] FOREIGN KEY ([empt2custid]) REFERENCES [EmpToCustMaps] ([id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Cust2EmpMaps_empt2custid] ON [Cust2EmpMaps] ([empt2custid]);

ALTER TABLE [Vouchers] ADD CONSTRAINT [FK_Vouchers_DealerMasters_DealerId] FOREIGN KEY ([DealerId]) REFERENCES [DealerMasters] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251121101712_UpdateVoucherModelForEntityTypes', N'9.0.10');

CREATE TABLE [EmployeeMasters] (
    [Id] int NOT NULL IDENTITY,
    [EmpCode] nvarchar(max) NOT NULL,
    [FirstName] nvarchar(500) NOT NULL,
    [MiddileName] nvarchar(500) NOT NULL,
    [LastName] nvarchar(500) NOT NULL,
    [Department] nvarchar(500) NOT NULL,
    [Route] nvarchar(500) NOT NULL,
    [Segment] nvarchar(500) NOT NULL,
    [Grade] nvarchar(500) NOT NULL,
    [UserType] nvarchar(500) NOT NULL,
    [Designation] nvarchar(500) NOT NULL,
    [EmployeeType] nvarchar(500) NOT NULL,
    [PhoneNumber] nvarchar(10) NOT NULL,
    [isActive] bit NOT NULL,
    CONSTRAINT [PK_EmployeeMasters] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251121122554_AddEntityTypesToVoucher', N'9.0.10');

ALTER TABLE [Vouchers] DROP CONSTRAINT [FK_Vouchers_DealerMasters_DealerId];

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vouchers]') AND [c].[name] = N'EntityIdentifier');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Vouchers] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Vouchers] DROP COLUMN [EntityIdentifier];

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vouchers]') AND [c].[name] = N'EntityType');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Vouchers] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Vouchers] DROP COLUMN [EntityType];

DROP INDEX [IX_Vouchers_DealerId] ON [Vouchers];
DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vouchers]') AND [c].[name] = N'DealerId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Vouchers] DROP CONSTRAINT [' + @var6 + '];');
UPDATE [Vouchers] SET [DealerId] = 0 WHERE [DealerId] IS NULL;
ALTER TABLE [Vouchers] ALTER COLUMN [DealerId] int NOT NULL;
ALTER TABLE [Vouchers] ADD DEFAULT 0 FOR [DealerId];
CREATE INDEX [IX_Vouchers_DealerId] ON [Vouchers] ([DealerId]);

CREATE TABLE [RedeemedProducts] (
    [Id] int NOT NULL IDENTITY,
    [VoucherId] int NOT NULL,
    [ProductName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    [RedemptionDate] datetime2 NOT NULL,
    CONSTRAINT [PK_RedeemedProducts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RedeemedProducts_Vouchers_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [Vouchers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_RedeemedProducts_VoucherId] ON [RedeemedProducts] ([VoucherId]);

ALTER TABLE [Vouchers] ADD CONSTRAINT [FK_Vouchers_DealerMasters_DealerId] FOREIGN KEY ([DealerId]) REFERENCES [DealerMasters] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251122012027_AddRedeemedProductsTableOnly', N'9.0.10');

COMMIT;
GO


