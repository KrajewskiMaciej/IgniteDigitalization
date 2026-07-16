using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IgniteDigitalization.Server.Migrations
{
    /// <inheritdoc />
    public partial class MapGameStatusToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // int -> nazwa w lowercase (During=0, Paused=1, End=2)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Games"" ALTER COLUMN ""Game_Status"" TYPE text USING (
                    CASE ""Game_Status""
                        WHEN 0 THEN 'during'
                        WHEN 1 THEN 'paused'
                        WHEN 2 THEN 'end'
                        ELSE NULL
                    END);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Games"" ALTER COLUMN ""Game_Status"" TYPE integer USING (
                    CASE ""Game_Status""
                        WHEN 'during' THEN 0
                        WHEN 'paused' THEN 1
                        WHEN 'end' THEN 2
                        ELSE NULL
                    END);");
        }
    }
}
