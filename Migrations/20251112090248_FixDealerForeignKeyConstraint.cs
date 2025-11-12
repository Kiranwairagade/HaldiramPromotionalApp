using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaldiramPromotionalApp.Migrations
{
    /// <inheritdoc />
    public partial class FixDealerForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove any Orders with DealerId that don't exist in DealerMasters
            migrationBuilder.Sql(@"
                DELETE FROM Orders 
                WHERE DealerId IS NOT NULL 
                AND DealerId NOT IN (SELECT Id FROM DealerMasters WHERE Id IS NOT NULL)
            ");

            // Check if the index already exists before creating it
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_DealerId' AND object_id = OBJECT_ID('Orders'))
                BEGIN
                    CREATE NONCLUSTERED INDEX [IX_Orders_DealerId] ON [Orders] ([DealerId]);
                END
            ");

            // Check if the foreign key constraint already exists before adding it
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Orders_DealerMasters_DealerId')
                BEGIN
                    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_DealerMasters_DealerId] FOREIGN KEY ([DealerId]) REFERENCES [DealerMasters] ([Id]) ON DELETE CASCADE;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Check if the foreign key constraint exists before dropping it
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Orders_DealerMasters_DealerId')
                BEGIN
                    ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_DealerMasters_DealerId];
                END
            ");

            // Check if the index exists before dropping it
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_DealerId' AND object_id = OBJECT_ID('Orders'))
                BEGIN
                    DROP INDEX [IX_Orders_DealerId] ON [Orders];
                END
            ");
        }
    }
}