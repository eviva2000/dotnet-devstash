using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DevStash.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabaseFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "devstash_dotnet");

            migrationBuilder.CreateTable(
                name: "application_users",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_roles",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "collections",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collections", x => x.id);
                    table.ForeignKey(
                        name: "fk_collections_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_claims",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_identity_user_claims_application_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_logins",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "FK_identity_user_logins_application_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_tokens",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "FK_identity_user_tokens_application_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_types",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_types", x => x.id);
                    table.CheckConstraint("ck_item_types_owner", "(is_system = TRUE AND user_id IS NULL) OR (is_system = FALSE AND user_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_item_types_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_tags_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_role_claims",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_identity_role_claims_identity_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_roles",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_identity_user_roles_application_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_identity_user_roles_identity_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "items",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    language = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    is_favorite = table.Column<bool>(type: "boolean", nullable: false),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_items_item_types_item_type_id",
                        column: x => x.item_type_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "item_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_items_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "application_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_collections",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_collections", x => new { x.item_id, x.collection_id });
                    table.ForeignKey(
                        name: "fk_item_collections_collections_collection_id",
                        column: x => x.collection_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_collections_items_item_id",
                        column: x => x.item_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_tags",
                schema: "devstash_dotnet",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_tags", x => new { x.item_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_item_tags_items_item_id",
                        column: x => x.item_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "devstash_dotnet",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "devstash_dotnet",
                table: "item_types",
                columns: new[] { "id", "color", "created_at", "icon", "is_system", "name", "slug", "updated_at", "user_id" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), null, new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "code", true, "Snippet", "snippet", new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), null, new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "sparkles", true, "Prompt", "prompt", new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), null, new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "note", true, "Note", "note", new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("d0000000-0000-0000-0000-000000000004"), null, new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "terminal", true, "Command", "command", new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("d0000000-0000-0000-0000-000000000005"), null, new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "file", true, "File", "file", new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("d0000000-0000-0000-0000-000000000006"), null, new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "image", true, "Image", "image", new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("d0000000-0000-0000-0000-000000000007"), null, new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "link", true, "Link", "link", new DateTimeOffset(new DateTime(2026, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_application_users_normalized_email",
                schema: "devstash_dotnet",
                table: "application_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ux_application_users_normalized_user_name",
                schema: "devstash_dotnet",
                table: "application_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_collections_user_updated_at",
                schema: "devstash_dotnet",
                table: "collections",
                columns: new[] { "user_id", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "ux_collections_user_slug",
                schema: "devstash_dotnet",
                table: "collections",
                columns: new[] { "user_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_role_claims_role_id",
                schema: "devstash_dotnet",
                table: "identity_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ux_identity_roles_normalized_name",
                schema: "devstash_dotnet",
                table: "identity_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_claims_user_id",
                schema: "devstash_dotnet",
                table: "identity_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_logins_user_id",
                schema: "devstash_dotnet",
                table: "identity_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_identity_user_roles_role_id",
                schema: "devstash_dotnet",
                table: "identity_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_collections_collection_id",
                schema: "devstash_dotnet",
                table: "item_collections",
                column: "collection_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_tags_tag_id",
                schema: "devstash_dotnet",
                table: "item_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_types_user_updated_at",
                schema: "devstash_dotnet",
                table: "item_types",
                columns: new[] { "user_id", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "ux_item_types_system_slug",
                schema: "devstash_dotnet",
                table: "item_types",
                column: "slug",
                unique: true,
                filter: "\"is_system\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "ux_item_types_user_slug",
                schema: "devstash_dotnet",
                table: "item_types",
                columns: new[] { "user_id", "slug" },
                unique: true,
                filter: "\"user_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_items_item_type_id",
                schema: "devstash_dotnet",
                table: "items",
                column: "item_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_items_user_updated_at",
                schema: "devstash_dotnet",
                table: "items",
                columns: new[] { "user_id", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tags_user_updated_at",
                schema: "devstash_dotnet",
                table: "tags",
                columns: new[] { "user_id", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "ux_tags_user_slug",
                schema: "devstash_dotnet",
                table: "tags",
                columns: new[] { "user_id", "slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identity_role_claims",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "identity_user_claims",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "identity_user_logins",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "identity_user_roles",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "identity_user_tokens",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "item_collections",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "item_tags",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "identity_roles",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "collections",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "items",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "item_types",
                schema: "devstash_dotnet");

            migrationBuilder.DropTable(
                name: "application_users",
                schema: "devstash_dotnet");
        }
    }
}
