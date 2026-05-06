using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautyPlusParlour.Migrations
{
    /// <inheritdoc />
    public partial class Module5_BookingManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: true),
                    booking_date = table.Column<DateOnly>(type: "date", nullable: false),
                    booking_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    booking_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    travel_charge = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    final_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    advance_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    advance_paid = table.Column<bool>(type: "boolean", nullable: false),
                    coupon_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    loyalty_points_used = table.Column<int>(type: "integer", nullable: false),
                    loyalty_points_earned = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancelled_by = table.Column<Guid>(type: "uuid", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requires_consultation = table.Column<bool>(type: "boolean", nullable: false),
                    consultation_scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    consultation_done_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_customer_addresses_address_id",
                        column: x => x.address_id,
                        principalTable: "customer_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bookings_staff_profiles_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    coupon_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    value = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    min_order_amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    max_discount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    usage_limit = table.Column<int>(type: "integer", nullable: true),
                    per_user_limit = table.Column<int>(type: "integer", nullable: false),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coupons", x => x.id);
                    table.ForeignKey(
                        name: "fk_coupons_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer_loyalty_points",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_points = table.Column<int>(type: "integer", nullable: false),
                    total_earned = table.Column<int>(type: "integer", nullable: false),
                    total_redeemed = table.Column<int>(type: "integer", nullable: false),
                    total_expired = table.Column<int>(type: "integer", nullable: false),
                    tier = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_loyalty_points", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_loyalty_points_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    buffer_minutes = table.Column<int>(type: "integer", nullable: false),
                    loyalty_points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_booking_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_booking_items_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_booking_items_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loyalty_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    balance_after = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_loyalty_transactions_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_loyalty_transactions_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    payment_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coupon_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    coupon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_applied = table.Column<decimal>(type: "numeric", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coupon_usages", x => x.id);
                    table.ForeignKey(
                        name: "fk_coupon_usages_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_coupon_usages_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_coupon_usages_customer_profiles_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_booking_id",
                table: "booking_items",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_service_id",
                table: "booking_items",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_address_id",
                table: "bookings",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_booking_code",
                table: "bookings",
                column: "booking_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bookings_booking_date_status",
                table: "bookings",
                columns: new[] { "booking_date", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_customer_id_status",
                table: "bookings",
                columns: new[] { "customer_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_staff_id_booking_date_status",
                table: "bookings",
                columns: new[] { "staff_id", "booking_date", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_coupon_usages_booking_id",
                table: "coupon_usages",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_coupon_usages_coupon_id",
                table: "coupon_usages",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "ix_coupon_usages_customer_id",
                table: "coupon_usages",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_coupons_code",
                table: "coupons",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_coupons_created_by",
                table: "coupons",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_coupons_is_active",
                table: "coupons",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_coupons_valid_from_valid_to",
                table: "coupons",
                columns: new[] { "valid_from", "valid_to" });

            migrationBuilder.CreateIndex(
                name: "ix_customer_loyalty_points_customer_id",
                table: "customer_loyalty_points",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_loyalty_points_tier",
                table: "customer_loyalty_points",
                column: "tier");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_transactions_booking_id",
                table: "loyalty_transactions",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_transactions_customer_id_created_at",
                table: "loyalty_transactions",
                columns: new[] { "customer_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_transactions_transaction_type_expires_at",
                table: "loyalty_transactions",
                columns: new[] { "transaction_type", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id",
                table: "payments",
                column: "booking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_items");

            migrationBuilder.DropTable(
                name: "coupon_usages");

            migrationBuilder.DropTable(
                name: "customer_loyalty_points");

            migrationBuilder.DropTable(
                name: "loyalty_transactions");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "coupons");

            migrationBuilder.DropTable(
                name: "bookings");
        }
    }
}
