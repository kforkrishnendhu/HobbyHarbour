using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HobbyHarbour.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWalletTableWithHistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletHistory_Wallets_WalletID",
                table: "WalletHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletHistory",
                table: "WalletHistory");

            migrationBuilder.RenameTable(
                name: "WalletHistory",
                newName: "WalletHistories");

            migrationBuilder.RenameIndex(
                name: "IX_WalletHistory_WalletID",
                table: "WalletHistories",
                newName: "IX_WalletHistories_WalletID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletHistories",
                table: "WalletHistories",
                column: "WalletHistoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletHistories_Wallets_WalletID",
                table: "WalletHistories",
                column: "WalletID",
                principalTable: "Wallets",
                principalColumn: "WalletID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletHistories_Wallets_WalletID",
                table: "WalletHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletHistories",
                table: "WalletHistories");

            migrationBuilder.RenameTable(
                name: "WalletHistories",
                newName: "WalletHistory");

            migrationBuilder.RenameIndex(
                name: "IX_WalletHistories_WalletID",
                table: "WalletHistory",
                newName: "IX_WalletHistory_WalletID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletHistory",
                table: "WalletHistory",
                column: "WalletHistoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletHistory_Wallets_WalletID",
                table: "WalletHistory",
                column: "WalletID",
                principalTable: "Wallets",
                principalColumn: "WalletID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
