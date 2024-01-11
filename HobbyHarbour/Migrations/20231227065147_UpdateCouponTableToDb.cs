using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HobbyHarbour.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCouponTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaximumCartAmount",
                table: "Coupons",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumCartAmount",
                table: "Coupons");
        }
    }
}
