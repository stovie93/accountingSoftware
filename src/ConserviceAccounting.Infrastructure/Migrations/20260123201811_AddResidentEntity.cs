using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConserviceAccounting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "residents",
                columns: table => new
                {
                    resident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resident_external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resident_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resident_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    resident_middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resident_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    resident_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    resident_alternate_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    resident_unit_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    resident_lease_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resident_lease_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resident_monthly_rent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    resident_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resident_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resident_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    resident_zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    resident_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    resident_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    resident_attributes = table.Column<string>(type: "jsonb", nullable: true),
                    resident_batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resident_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    resident_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    resident_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_residents", x => x.resident_id);
                    table.ForeignKey(
                        name: "FK_residents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_residents",
                columns: table => new
                {
                    property_resident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resident_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_resident_unit_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    property_resident_move_in_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    property_resident_move_out_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    property_resident_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    property_resident_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    property_resident_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    property_resident_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    property_resident_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_residents", x => x.property_resident_id);
                    table.ForeignKey(
                        name: "FK_property_residents_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "property_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_property_residents_residents_resident_id",
                        column: x => x.resident_id,
                        principalTable: "residents",
                        principalColumn: "resident_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_property_residents_property_id_resident_id",
                table: "property_residents",
                columns: new[] { "property_id", "resident_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_residents_resident_id",
                table: "property_residents",
                column: "resident_id");

            migrationBuilder.CreateIndex(
                name: "IX_residents_resident_batch_id",
                table: "residents",
                column: "resident_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_residents_user_id_resident_external_id",
                table: "residents",
                columns: new[] { "user_id", "resident_external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_residents_user_id_resident_is_active",
                table: "residents",
                columns: new[] { "user_id", "resident_is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_residents_user_id_resident_status",
                table: "residents",
                columns: new[] { "user_id", "resident_status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "property_residents");

            migrationBuilder.DropTable(
                name: "residents");
        }
    }
}
