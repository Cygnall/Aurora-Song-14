using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddCharacterRecordCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profile_preference_preference_id",
                table: "profile");

            migrationBuilder.AlterColumn<int>(
                name: "preference_id",
                table: "profile",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "record_character",
                columns: table => new
                {
                    record_character_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_edit_id = table.Column<int>(type: "INTEGER", nullable: true),
                    round_id = table.Column<int>(type: "INTEGER", nullable: true),
                    record_type = table.Column<int>(type: "INTEGER", nullable: false),
                    target_character_id = table.Column<int>(type: "INTEGER", nullable: true),
                    author_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    author_character_id = table.Column<int>(type: "INTEGER", nullable: true),
                    hidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    deleted_by_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_record_character", x => x.record_character_id);
                    table.ForeignKey(
                        name: "FK_record_character_player_player_id",
                        column: x => x.author_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_record_character_player_player_id1",
                        column: x => x.deleted_by_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_record_character_profile_profile_id",
                        column: x => x.author_character_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_record_character_profile_profile_id1",
                        column: x => x.target_character_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_edit",
                columns: table => new
                {
                    record_edit_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    record_character_id = table.Column<int>(type: "INTEGER", nullable: false),
                    field = table.Column<string>(type: "TEXT", nullable: false),
                    old_value = table.Column<string>(type: "TEXT", nullable: true),
                    new_value = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    author_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    author_character_id = table.Column<int>(type: "INTEGER", nullable: true),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    deleted_by_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_record_edit", x => x.record_edit_id);
                    table.ForeignKey(
                        name: "FK_record_edit_player_player_id",
                        column: x => x.author_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_record_edit_player_player_id1",
                        column: x => x.deleted_by_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_record_edit_profile_profile_id",
                        column: x => x.author_character_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_record_edit_record_character_record_character_id",
                        column: x => x.record_character_id,
                        principalTable: "record_character",
                        principalColumn: "record_character_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_record_character_author_character_id",
                table: "record_character",
                column: "author_character_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_character_author_user_id",
                table: "record_character",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_character_deleted_by_id",
                table: "record_character",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_character_last_edit_id",
                table: "record_character",
                column: "last_edit_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_record_character_record_type_target_character_id_created_at",
                table: "record_character",
                columns: new[] { "record_type", "target_character_id", "created_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_record_character_target_character_id",
                table: "record_character",
                column: "target_character_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_edit_author_character_id",
                table: "record_edit",
                column: "author_character_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_edit_author_user_id",
                table: "record_edit",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_edit_deleted_by_id",
                table: "record_edit",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_record_edit_record_character_id_created_at",
                table: "record_edit",
                columns: new[] { "record_character_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.AddForeignKey(
                name: "FK_profile_preference_preference_id",
                table: "profile",
                column: "preference_id",
                principalTable: "preference",
                principalColumn: "preference_id");

            migrationBuilder.AddForeignKey(
                name: "FK_record_character_record_edit_last_edit_id1",
                table: "record_character",
                column: "last_edit_id",
                principalTable: "record_edit",
                principalColumn: "record_edit_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profile_preference_preference_id",
                table: "profile");

            migrationBuilder.DropForeignKey(
                name: "FK_record_character_record_edit_last_edit_id1",
                table: "record_character");

            migrationBuilder.DropTable(
                name: "record_edit");

            migrationBuilder.DropTable(
                name: "record_character");

            migrationBuilder.AlterColumn<int>(
                name: "preference_id",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_profile_preference_preference_id",
                table: "profile",
                column: "preference_id",
                principalTable: "preference",
                principalColumn: "preference_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
