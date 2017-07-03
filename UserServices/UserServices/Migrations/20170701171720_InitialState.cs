using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace UserServices.Migrations
{
    public partial class InitialState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CpfCnpj = table.Column<string>(maxLength: 14, nullable: true),
                    Email = table.Column<string>(maxLength: 100, nullable: false),
                    FacebookId = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(maxLength: 100, nullable: false),
                    GoogleId = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(maxLength: 100, nullable: false),
                    Password = table.Column<string>(maxLength: 100, nullable: true),
                    PictureUrl = table.Column<string>(nullable: true),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CpfCnpj",
                table: "Users",
                column: "CpfCnpj",
                unique: true,
                filter: "[CpfCnpj] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
