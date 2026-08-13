using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSG.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class agrtemqir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Professores_CoordenadorIdProfessor",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Modulos_Professores_ProfessorIdProfessor",
                table: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_Modulos_ProfessorIdProfessor",
                table: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_CoordenadorIdProfessor",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "ProfessorIdProfessor",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "CoordenadorIdProfessor",
                table: "Cursos");

            migrationBuilder.RenameColumn(
                name: "idCoordenador",
                table: "Cursos",
                newName: "IdCoordenador");

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_IdProfessor",
                table: "Modulos",
                column: "IdProfessor");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdCoordenador",
                table: "Cursos",
                column: "IdCoordenador");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Professores_IdCoordenador",
                table: "Cursos",
                column: "IdCoordenador",
                principalTable: "Professores",
                principalColumn: "IdProfessor",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Modulos_Professores_IdProfessor",
                table: "Modulos",
                column: "IdProfessor",
                principalTable: "Professores",
                principalColumn: "IdProfessor",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Professores_IdCoordenador",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Modulos_Professores_IdProfessor",
                table: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_Modulos_IdProfessor",
                table: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdCoordenador",
                table: "Cursos");

            migrationBuilder.RenameColumn(
                name: "IdCoordenador",
                table: "Cursos",
                newName: "idCoordenador");

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
    }
}
