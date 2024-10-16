using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteTaking.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                    @"
                    CREATE VIEW UserNoteSummaryView

                        AS

                        SELECT u.UserIdentificationNumber UserID, COUNT(*) AS Number_Of_Notes
                        FROM dbo.Users u
                        JOIN dbo.Notes n
                        ON u.UserIdentificationNumber = n.UserIdentificationNumber
                        GROUP BY u.UserIdentificationNumber
                        ORDER BY Number_Of_Notes
                    "

            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW UserNoteSummaryView");
        }
    }
}
