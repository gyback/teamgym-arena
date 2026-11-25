using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamGym.Arena.Modules.Organisations.Persistence.Migrations
{
    /// <inheritdoc />
    internal partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "organisations");

            migrationBuilder.CreateTable(
                name: "organisations",
                schema: "organisations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    short_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    country_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    parent_organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    default_rule_set_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisations_organisations_parent_organisation_id",
                        column: x => x.parent_organisation_id,
                        principalSchema: "organisations",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gymnasts",
                schema: "organisations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    given_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    family_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    competitive_gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gymnasts", x => x.id);
                    table.ForeignKey(
                        name: "fk_gymnasts_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "organisations",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organisation_memberships",
                schema: "organisations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_memberships", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_memberships_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "organisations",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                schema: "organisations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    section = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teams", x => x.id);
                    table.ForeignKey(
                        name: "fk_teams_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "organisations",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coach_team_assignments",
                schema: "organisations",
                columns: table => new
                {
                    organisation_membership_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coach_team_assignments", x => new { x.organisation_membership_id, x.team_id });
                    table.ForeignKey(
                        name: "fk_coach_team_assignments_organisation_memberships_organisatio",
                        column: x => x.organisation_membership_id,
                        principalSchema: "organisations",
                        principalTable: "organisation_memberships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_coach_team_assignments_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "organisations",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_members",
                schema: "organisations",
                columns: table => new
                {
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gymnast_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_team_members", x => new { x.team_id, x.gymnast_id });
                    table.ForeignKey(
                        name: "fk_team_members_gymnasts_gymnast_id",
                        column: x => x.gymnast_id,
                        principalSchema: "organisations",
                        principalTable: "gymnasts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_team_members_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "organisations",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_coach_team_assignments_team_id",
                schema: "organisations",
                table: "coach_team_assignments",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_gymnasts_organisation_id",
                schema: "organisations",
                table: "gymnasts",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_memberships_organisation_id_user_id_role",
                schema: "organisations",
                table: "organisation_memberships",
                columns: new[] { "organisation_id", "user_id", "role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_memberships_user_id",
                schema: "organisations",
                table: "organisation_memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisations_parent_organisation_id",
                schema: "organisations",
                table: "organisations",
                column: "parent_organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_team_members_gymnast_id",
                schema: "organisations",
                table: "team_members",
                column: "gymnast_id");

            migrationBuilder.CreateIndex(
                name: "ix_teams_organisation_id",
                schema: "organisations",
                table: "teams",
                column: "organisation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coach_team_assignments",
                schema: "organisations");

            migrationBuilder.DropTable(
                name: "team_members",
                schema: "organisations");

            migrationBuilder.DropTable(
                name: "organisation_memberships",
                schema: "organisations");

            migrationBuilder.DropTable(
                name: "gymnasts",
                schema: "organisations");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "organisations");

            migrationBuilder.DropTable(
                name: "organisations",
                schema: "organisations");
        }
    }
}
