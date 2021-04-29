using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace NBB.Payments.Migrations.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payables",
                columns: table => new
                {
                    PayableId = table.Column<Guid>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    ClientId = table.Column<Guid>(nullable: false),
                    InvoiceId = table.Column<Guid>(nullable: true),
                    ContractId = table.Column<Guid>(nullable: true),
                    Version = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payables", x => x.PayableId);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(nullable: false),
                    PayableId = table.Column<Guid>(nullable: false),
                    PaymentDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Payables_PayableId",
                        column: x => x.PayableId,
                        principalTable: "Payables",
                        principalColumn: "PayableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PayableId",
                table: "Payments",
                column: "PayableId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Payables");
        }
    }
}
