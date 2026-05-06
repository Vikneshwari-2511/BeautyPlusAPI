using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautyPlusParlour.Migrations
{
    /// <inheritdoc />
    public partial class Module4_CustomerManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    profile_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer_addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address_line1 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    address_line2 = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pin_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    landmark = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_addresses_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favourite_services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favourite_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_favourite_services_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_favourite_services_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_addresses_customer_id_is_active",
                table: "customer_addresses",
                columns: new[] { "customer_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_customer_addresses_customer_id_is_default",
                table: "customer_addresses",
                columns: new[] { "customer_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_customer_profiles_is_active",
                table: "customer_profiles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_customer_profiles_user_id",
                table: "customer_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_favourite_services_customer_id_service_id",
                table: "favourite_services",
                columns: new[] { "customer_id", "service_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_favourite_services_service_id",
                table: "favourite_services",
                column: "service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_addresses");

            migrationBuilder.DropTable(
                name: "favourite_services");

            migrationBuilder.DropTable(
                name: "customer_profiles");
        }
    }
}
