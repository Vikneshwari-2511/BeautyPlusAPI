using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautyPlusParlour.Migrations
{
    /// <inheritdoc />
    public partial class Module7_ReviewsAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_rating = table.Column<int>(type: "integer", nullable: false),
                    staff_rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    hidden_by = table.Column<Guid>(type: "uuid", nullable: true),
                    hidden_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hide_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviews", x => x.id);
                    table.ForeignKey(
                        name: "fk_reviews_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reviews_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reviews_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reviews_staff_profiles_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reviews_users_hidden_by",
                        column: x => x.hidden_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_booking_id",
                table: "reviews",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reviews_customer_id_created_at",
                table: "reviews",
                columns: new[] { "customer_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_hidden_by",
                table: "reviews",
                column: "hidden_by");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_service_id_is_visible",
                table: "reviews",
                columns: new[] { "service_id", "is_visible" });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_staff_id_is_visible",
                table: "reviews",
                columns: new[] { "staff_id", "is_visible" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reviews");
        }
    }
}
