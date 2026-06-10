using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConserviceAccounting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounting_software",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    connection_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_software", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    user_password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    user_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_role = table.Column<int>(type: "integer", nullable: false),
                    user_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    user_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    user_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    user_last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "bills",
                columns: table => new
                {
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bill_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    bill_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bill_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    bill_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bill_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bill_paid_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bill_period_start = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bill_period_end = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bill_quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    bill_unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bill_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    bill_tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    bill_total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    bill_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    bill_notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    bill_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bill_external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    bill_attributes = table.Column<string>(type: "jsonb", nullable: true),
                    bill_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    bill_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    bill_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bills", x => x.bill_id);
                    table.ForeignKey(
                        name: "FK_bills_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_accounting_connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accounting_software_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    connection_string = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    database_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    database_server = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    database_username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    database_password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    api_endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    api_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    api_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sftp_host = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sftp_port = table.Column<int>(type: "integer", nullable: true),
                    sftp_username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sftp_password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sftp_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_tested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_test_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_accounting_connections", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_accounting_connections_accounting_software_accountin~",
                        column: x => x.accounting_software_id,
                        principalTable: "accounting_software",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_accounting_connections_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entity_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_entity_types_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_client_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    property_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    property_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    property_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    property_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    property_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    property_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    property_unit_count = table.Column<int>(type: "integer", nullable: true),
                    property_square_footage = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    property_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    property_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    property_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties", x => x.property_id);
                    table.ForeignKey(
                        name: "FK_properties_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                columns: table => new
                {
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    provider_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    provider_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    provider_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    provider_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    provider_website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    provider_contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    provider_account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_attributes = table.Column<string>(type: "jsonb", nullable: true),
                    provider_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    provider_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    provider_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_providers", x => x.provider_id);
                    table.ForeignKey(
                        name: "FK_providers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    configuration = table.Column<string>(type: "jsonb", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_shared = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_definitions", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_definitions_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_definitions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scope_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scope_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_scope_types_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "utilities",
                columns: table => new
                {
                    utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    utility_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    utility_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    utility_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    utility_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    utility_attributes = table.Column<string>(type: "jsonb", nullable: true),
                    utility_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    utility_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    utility_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilities", x => x.utility_id);
                    table.ForeignKey(
                        name: "FK_utilities_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_bills",
                columns: table => new
                {
                    user_bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_bill_role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_bill_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_bill_assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    user_bill_action_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_bill_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    user_bill_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    user_bill_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    user_bill_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_bills", x => x.user_bill_id);
                    table.ForeignKey(
                        name: "FK_user_bills_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "bill_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_bills_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    attributes = table.Column<string>(type: "jsonb", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entities", x => x.id);
                    table.ForeignKey(
                        name: "FK_entities_entity_types_entity_type_id",
                        column: x => x.entity_type_id,
                        principalTable: "entity_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entities_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_bills",
                columns: table => new
                {
                    property_bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_bill_allocation_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    property_bill_allocation_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    property_bill_allocation_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    property_bill_cost_center = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_bill_gl_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    property_bill_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    property_bill_is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    property_bill_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    property_bill_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    property_bill_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_bills", x => x.property_bill_id);
                    table.ForeignKey(
                        name: "FK_property_bills_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "bill_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_property_bills_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "property_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_properties",
                columns: table => new
                {
                    user_property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_property_role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_property_access_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_property_is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    user_property_assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    user_property_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_property_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    user_property_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    user_property_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    user_property_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_properties", x => x.user_property_id);
                    table.ForeignKey(
                        name: "FK_user_properties_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "property_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_properties_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bill_providers",
                columns: table => new
                {
                    bill_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_provider_account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bill_provider_invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bill_provider_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    bill_provider_is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    bill_provider_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    bill_provider_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    bill_provider_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_providers", x => x.bill_provider_id);
                    table.ForeignKey(
                        name: "FK_bill_providers_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "bill_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bill_providers_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "provider_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_providers",
                columns: table => new
                {
                    property_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_provider_account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_provider_service_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_provider_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    property_provider_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    property_provider_contract_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_provider_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    property_provider_is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    property_provider_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    property_provider_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    property_provider_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_providers", x => x.property_provider_id);
                    table.ForeignKey(
                        name: "FK_property_providers_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "property_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_property_providers_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "provider_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    frequency = table.Column<int>(type: "integer", nullable: false),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    time_of_day = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: true),
                    day_of_month = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    export_format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "csv"),
                    destination_table = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    destination_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_run_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    next_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_schedules_client_accounting_connections_connection_id",
                        column: x => x.connection_id,
                        principalTable: "client_accounting_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_schedules_report_definitions_report_definition_id",
                        column: x => x.report_definition_id,
                        principalTable: "report_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_schedules_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_schedules_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scopes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_scope_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scope_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    path = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scopes", x => x.id);
                    table.ForeignKey(
                        name: "FK_scopes_scope_types_scope_type_id",
                        column: x => x.scope_type_id,
                        principalTable: "scope_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_scopes_scopes_parent_scope_id",
                        column: x => x.parent_scope_id,
                        principalTable: "scopes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_scopes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bill_utilities",
                columns: table => new
                {
                    bill_utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_utility_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    bill_utility_quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    bill_utility_unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bill_utility_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    bill_utility_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    bill_utility_is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    bill_utility_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    bill_utility_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    bill_utility_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_utilities", x => x.bill_utility_id);
                    table.ForeignKey(
                        name: "FK_bill_utilities_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "bill_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bill_utilities_utilities_utility_id",
                        column: x => x.utility_id,
                        principalTable: "utilities",
                        principalColumn: "utility_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_utilities",
                columns: table => new
                {
                    property_utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_utility_account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_utility_meter_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    property_utility_service_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    property_utility_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    property_utility_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    property_utility_budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    property_utility_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    property_utility_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    property_utility_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    property_utility_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_utilities", x => x.property_utility_id);
                    table.ForeignKey(
                        name: "FK_property_utilities_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "property_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_property_utilities_utilities_utility_id",
                        column: x => x.utility_id,
                        principalTable: "utilities",
                        principalColumn: "utility_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_utilities",
                columns: table => new
                {
                    provider_utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_utility_service_area = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    provider_utility_default_rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    provider_utility_rate_unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_utility_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    provider_utility_is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    provider_utility_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    provider_utility_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    provider_utility_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_utilities", x => x.provider_utility_id);
                    table.ForeignKey(
                        name: "FK_provider_utilities_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "provider_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_provider_utilities_utilities_utility_id",
                        column: x => x.utility_id,
                        principalTable: "utilities",
                        principalColumn: "utility_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_pushes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pushed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    record_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_pushes", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_pushes_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_data_pushes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entity_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    attributes = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_entity_links_entities_source_entity_id",
                        column: x => x.source_entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entity_links_entities_target_entity_id",
                        column: x => x.target_entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entity_links_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_schedule_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    records_processed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    records_failed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    output_file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_schedule_runs", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_schedule_runs_report_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "report_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    attributes = table.Column<string>(type: "jsonb", nullable: true),
                    source = table.Column<int>(type: "integer", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_items_scopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "scopes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scope_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scope_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_scope_links_scopes_source_scope_id",
                        column: x => x.source_scope_id,
                        principalTable: "scopes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scope_links_scopes_target_scope_id",
                        column: x => x.target_scope_id,
                        principalTable: "scopes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_scope_links_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_utility_bills",
                columns: table => new
                {
                    property_utility_bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_utility_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_utility_bill_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    property_utility_bill_quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    property_utility_bill_period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    property_utility_bill_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    property_utility_bill_is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    property_utility_bill_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    property_utility_bill_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_utility_bills", x => x.property_utility_bill_id);
                    table.ForeignKey(
                        name: "FK_property_utility_bills_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "bill_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_property_utility_bills_property_utilities_property_utility_~",
                        column: x => x.property_utility_id,
                        principalTable: "property_utilities",
                        principalColumn: "property_utility_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_entities",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_entities", x => new { x.item_id, x.entity_id });
                    table.ForeignKey(
                        name: "FK_item_entities_entities_entity_id",
                        column: x => x.entity_id,
                        principalTable: "entities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_entities_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_scopes",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_scopes", x => new { x.item_id, x.scope_id });
                    table.ForeignKey(
                        name: "FK_item_scopes_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_scopes_scopes_scope_id",
                        column: x => x.scope_id,
                        principalTable: "scopes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accounting_software_code",
                table: "accounting_software",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bill_providers_bill_id_provider_id",
                table: "bill_providers",
                columns: new[] { "bill_id", "provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bill_providers_provider_id",
                table: "bill_providers",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_bill_utilities_bill_id_utility_id",
                table: "bill_utilities",
                columns: new[] { "bill_id", "utility_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bill_utilities_utility_id",
                table: "bill_utilities",
                column: "utility_id");

            migrationBuilder.CreateIndex(
                name: "IX_bills_user_id_bill_category",
                table: "bills",
                columns: new[] { "user_id", "bill_category" });

            migrationBuilder.CreateIndex(
                name: "IX_bills_user_id_bill_date",
                table: "bills",
                columns: new[] { "user_id", "bill_date" });

            migrationBuilder.CreateIndex(
                name: "IX_bills_user_id_bill_due_date",
                table: "bills",
                columns: new[] { "user_id", "bill_due_date" });

            migrationBuilder.CreateIndex(
                name: "IX_bills_user_id_bill_external_id",
                table: "bills",
                columns: new[] { "user_id", "bill_external_id" });

            migrationBuilder.CreateIndex(
                name: "IX_bills_user_id_bill_reference_id",
                table: "bills",
                columns: new[] { "user_id", "bill_reference_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bills_user_id_bill_status",
                table: "bills",
                columns: new[] { "user_id", "bill_status" });

            migrationBuilder.CreateIndex(
                name: "ix_bills_user_status_date",
                table: "bills",
                columns: new[] { "user_id", "bill_status", "bill_date" });

            migrationBuilder.CreateIndex(
                name: "IX_client_accounting_connections_accounting_software_id",
                table: "client_accounting_connections",
                column: "accounting_software_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_accounting_connections_user_id",
                table: "client_accounting_connections",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_pushes_batch_id",
                table: "data_pushes",
                column: "batch_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_data_pushes_entity_id",
                table: "data_pushes",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_pushes_user_id",
                table: "data_pushes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_entities_entity_type_id",
                table: "entities",
                column: "entity_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_entities_user_id_code",
                table: "entities",
                columns: new[] { "user_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entities_user_id_entity_type_id",
                table: "entities",
                columns: new[] { "user_id", "entity_type_id" });

            migrationBuilder.CreateIndex(
                name: "IX_entities_user_id_is_active",
                table: "entities",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_links_source_entity_id",
                table: "entity_links",
                column: "source_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_entity_links_target_entity_id",
                table: "entity_links",
                column: "target_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_entity_links_user_id_link_type",
                table: "entity_links",
                columns: new[] { "user_id", "link_type" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_links_user_id_source_entity_id",
                table: "entity_links",
                columns: new[] { "user_id", "source_entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_links_user_id_source_entity_id_target_entity_id_link~",
                table: "entity_links",
                columns: new[] { "user_id", "source_entity_id", "target_entity_id", "link_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entity_links_user_id_target_entity_id",
                table: "entity_links",
                columns: new[] { "user_id", "target_entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_entity_types_user_id_code",
                table: "entity_types",
                columns: new[] { "user_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_entities_entity_id",
                table: "item_entities",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_entities_item_id",
                table: "item_entities",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_scopes_item_id",
                table: "item_scopes",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_scopes_scope_id",
                table: "item_scopes",
                column: "scope_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_batch_id",
                table: "items",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_items_ScopeId",
                table: "items",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_items_user_id_date",
                table: "items",
                columns: new[] { "user_id", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_items_user_id_external_id",
                table: "items",
                columns: new[] { "user_id", "external_id" });

            migrationBuilder.CreateIndex(
                name: "ix_items_user_source",
                table: "items",
                columns: new[] { "user_id", "source" });

            migrationBuilder.CreateIndex(
                name: "ix_items_user_source_date",
                table: "items",
                columns: new[] { "user_id", "source", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_properties_user_id_property_client_id",
                table: "properties",
                columns: new[] { "user_id", "property_client_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_properties_user_id_property_is_active",
                table: "properties",
                columns: new[] { "user_id", "property_is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_properties_user_id_property_type",
                table: "properties",
                columns: new[] { "user_id", "property_type" });

            migrationBuilder.CreateIndex(
                name: "IX_property_bills_bill_id",
                table: "property_bills",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "IX_property_bills_property_id_bill_id",
                table: "property_bills",
                columns: new[] { "property_id", "bill_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_providers_property_id_provider_id",
                table: "property_providers",
                columns: new[] { "property_id", "provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_providers_provider_id",
                table: "property_providers",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_property_utilities_property_id_utility_id",
                table: "property_utilities",
                columns: new[] { "property_id", "utility_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_utilities_utility_id",
                table: "property_utilities",
                column: "utility_id");

            migrationBuilder.CreateIndex(
                name: "IX_property_utility_bills_bill_id",
                table: "property_utility_bills",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "IX_property_utility_bills_property_utility_id_bill_id",
                table: "property_utility_bills",
                columns: new[] { "property_utility_id", "bill_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_utilities_provider_id_utility_id",
                table: "provider_utilities",
                columns: new[] { "provider_id", "utility_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_utilities_utility_id",
                table: "provider_utilities",
                column: "utility_id");

            migrationBuilder.CreateIndex(
                name: "IX_providers_user_id_provider_account_id",
                table: "providers",
                columns: new[] { "user_id", "provider_account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_providers_user_id_provider_is_active",
                table: "providers",
                columns: new[] { "user_id", "provider_is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_providers_user_id_provider_type",
                table: "providers",
                columns: new[] { "user_id", "provider_type" });

            migrationBuilder.CreateIndex(
                name: "IX_report_definitions_created_by_id",
                table: "report_definitions",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_definitions_user_id",
                table: "report_definitions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedule_runs_schedule_id",
                table: "report_schedule_runs",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedule_runs_started_at",
                table: "report_schedule_runs",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_connection_id",
                table: "report_schedules",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_created_by_id",
                table: "report_schedules",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_is_active",
                table: "report_schedules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_next_run_at",
                table: "report_schedules",
                column: "next_run_at");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_report_definition_id",
                table: "report_schedules",
                column: "report_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_schedules_user_id",
                table: "report_schedules",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_scope_links_source_scope_id",
                table: "scope_links",
                column: "source_scope_id");

            migrationBuilder.CreateIndex(
                name: "IX_scope_links_target_scope_id",
                table: "scope_links",
                column: "target_scope_id");

            migrationBuilder.CreateIndex(
                name: "IX_scope_links_user_id_source_scope_id_target_scope_id_link_ty~",
                table: "scope_links",
                columns: new[] { "user_id", "source_scope_id", "target_scope_id", "link_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scope_types_user_id_code",
                table: "scope_types",
                columns: new[] { "user_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scopes_parent_scope_id",
                table: "scopes",
                column: "parent_scope_id");

            migrationBuilder.CreateIndex(
                name: "IX_scopes_scope_type_id",
                table: "scopes",
                column: "scope_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_scopes_user_id_code",
                table: "scopes",
                columns: new[] { "user_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scopes_user_id_path",
                table: "scopes",
                columns: new[] { "user_id", "path" });

            migrationBuilder.CreateIndex(
                name: "IX_user_bills_bill_id",
                table: "user_bills",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_bills_user_id_bill_id_user_bill_role",
                table: "user_bills",
                columns: new[] { "user_id", "bill_id", "user_bill_role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_properties_property_id",
                table: "user_properties",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_properties_user_id_property_id",
                table: "user_properties",
                columns: new[] { "user_id", "property_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_user_email",
                table: "users",
                column: "user_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utilities_user_id_utility_code",
                table: "utilities",
                columns: new[] { "user_id", "utility_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utilities_user_id_utility_is_active",
                table: "utilities",
                columns: new[] { "user_id", "utility_is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_utilities_user_id_utility_type",
                table: "utilities",
                columns: new[] { "user_id", "utility_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bill_providers");

            migrationBuilder.DropTable(
                name: "bill_utilities");

            migrationBuilder.DropTable(
                name: "data_pushes");

            migrationBuilder.DropTable(
                name: "entity_links");

            migrationBuilder.DropTable(
                name: "item_entities");

            migrationBuilder.DropTable(
                name: "item_scopes");

            migrationBuilder.DropTable(
                name: "property_bills");

            migrationBuilder.DropTable(
                name: "property_providers");

            migrationBuilder.DropTable(
                name: "property_utility_bills");

            migrationBuilder.DropTable(
                name: "provider_utilities");

            migrationBuilder.DropTable(
                name: "report_schedule_runs");

            migrationBuilder.DropTable(
                name: "scope_links");

            migrationBuilder.DropTable(
                name: "user_bills");

            migrationBuilder.DropTable(
                name: "user_properties");

            migrationBuilder.DropTable(
                name: "entities");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "property_utilities");

            migrationBuilder.DropTable(
                name: "providers");

            migrationBuilder.DropTable(
                name: "report_schedules");

            migrationBuilder.DropTable(
                name: "bills");

            migrationBuilder.DropTable(
                name: "entity_types");

            migrationBuilder.DropTable(
                name: "scopes");

            migrationBuilder.DropTable(
                name: "properties");

            migrationBuilder.DropTable(
                name: "utilities");

            migrationBuilder.DropTable(
                name: "client_accounting_connections");

            migrationBuilder.DropTable(
                name: "report_definitions");

            migrationBuilder.DropTable(
                name: "scope_types");

            migrationBuilder.DropTable(
                name: "accounting_software");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
