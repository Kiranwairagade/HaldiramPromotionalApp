CREATE TABLE [DistributorVouchers] (
    [Id] int NOT NULL IDENTITY,
    [VoucherCode] nvarchar(max) NOT NULL,
    [DistributorId] int NOT NULL,
    [CampaignType] nvarchar(max) NOT NULL,
    [CampaignId] int NOT NULL,
    [VoucherValue] decimal(18,2) NOT NULL,
    [IssueDate] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [IsRedeemed] bit NOT NULL,
    [RedeemedDate] datetime2 NULL,
    [QRCodeData] nvarchar(max) NULL,
    CONSTRAINT [PK_DistributorVouchers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DistributorVouchers_DealerMasters_DistributorId] FOREIGN KEY ([DistributorId]) REFERENCES [DealerMasters] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_DistributorVouchers_DistributorId] ON [DistributorVouchers] ([DistributorId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251122101500_CreateDistributorVoucherTable', N'9.0.10');