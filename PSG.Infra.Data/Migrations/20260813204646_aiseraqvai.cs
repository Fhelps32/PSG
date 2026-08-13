using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSG.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class aiseraqvai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdProfessor",
                table: "Modulos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfessorIdProfessor",
                table: "Modulos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CoordenadorIdProfessor",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "idCoordenador",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Professores",
                columns: table => new
                {
                    IdProfessor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricula = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professores", x => x.IdProfessor);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_ProfessorIdProfessor",
                table: "Modulos",
                column: "ProfessorIdProfessor");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_CoordenadorIdProfessor",
                table: "Cursos",
                column: "CoordenadorIdProfessor");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Professores_CoordenadorIdProfessor",
                table: "Cursos",
                column: "CoordenadorIdProfessor",
                principalTable: "Professores",
                principalColumn: "IdProfessor",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Modulos_Professores_ProfessorIdProfessor",
                table: "Modulos",
                column: "ProfessorIdProfessor",
                principalTable: "Professores",
                principalColumn: "IdProfessor",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Professores_CoordenadorIdProfessor",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Modulos_Professores_ProfessorIdProfessor",
                table: "Modulos");

            migrationBuilder.DropTable(
                name: "Professores");

            migrationBuilder.DropIndex(
                name: "IX_Modulos_ProfessorIdProfessor",
                table: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_CoordenadorIdProfessor",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "IdProfessor",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "ProfessorIdProfessor",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "CoordenadorIdProfessor",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "idCoordenador",
                table: "Cursos");
        }
    }
}
