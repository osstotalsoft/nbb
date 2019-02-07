using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.Migrations.ReadModelMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    ClientId = table.Column<Guid>(nullable: false),
                    IsValidated = table.Column<bool>(nullable: false),
                    Version = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractId);
                });

            migrationBuilder.CreateTable(
                name: "ContractLines",
                columns: table => new
                {
                    ContractLineId = table.Column<Guid>(nullable: false),
                    ContractId = table.Column<Guid>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    Product = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractLines", x => x.ContractLineId);
                    table.ForeignKey(
                        name: "FK_ContractLines_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "ContractId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractLines_ContractId",
                table: "ContractLines",
                column: "ContractId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractLines");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
