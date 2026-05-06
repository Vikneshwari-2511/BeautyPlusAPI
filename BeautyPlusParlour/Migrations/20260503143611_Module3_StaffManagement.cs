using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeautyPlusParlour.Migrations
{
    /// <inheritdoc />
    public partial class Module3_StaffManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "staff_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    alternate_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    profile_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bio = table.Column<string>(type: "text", nullable: true),
                    experience_years = table.Column<int>(type: "integer", nullable: false),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_available_for_on_site = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    joined_at = table.Column<DateOnly>(type: "date", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_staff_profiles_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_staff_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "staff_leaves",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leave_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    leave_from_date = table.Column<DateOnly>(type: "date", nullable: false),
                    leave_to_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_days = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_leaves", x => x.id);
                    table.ForeignKey(
                        name: "fk_staff_leaves_staff_profiles_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_staff_leaves_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "staff_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_working_day = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_staff_schedules_staff_profiles_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proficiency_level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_skills", x => x.id);
                    table.ForeignKey(
                        name: "fk_staff_skills_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_staff_skills_staff_profiles_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_staff_skills_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_staff_leaves_leave_from_date_leave_to_date",
                table: "staff_leaves",
                columns: new[] { "leave_from_date", "leave_to_date" });

            migrationBuilder.CreateIndex(
                name: "ix_staff_leaves_reviewed_by",
                table: "staff_leaves",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "ix_staff_leaves_staff_id_status",
                table: "staff_leaves",
                columns: new[] { "staff_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_staff_profiles_created_by",
                table: "staff_profiles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_staff_profiles_employee_code",
                table: "staff_profiles",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staff_profiles_is_active",
                table: "staff_profiles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_staff_profiles_user_id",
                table: "staff_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staff_schedules_staff_id_day_of_week",
                table: "staff_schedules",
                columns: new[] { "staff_id", "day_of_week" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_staff_skills_created_by",
                table: "staff_skills",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_staff_skills_service_id",
                table: "staff_skills",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_staff_skills_staff_id_service_id",
                table: "staff_skills",
                columns: new[] { "staff_id", "service_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "staff_leaves");

            migrationBuilder.DropTable(
                name: "staff_schedules");

            migrationBuilder.DropTable(
                name: "staff_skills");

            migrationBuilder.DropTable(
                name: "staff_profiles");
        }
    }
}
