using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PSG.Infra.Data.Migrations
{
    /// <summary>
    /// Cria a tabela Professores e liga o coordenador ao Curso e o professor ao Módulo.
    ///
    /// As duas FKs usam Restrict (ON DELETE NO ACTION) de propósito: com Cascade,
    /// apagar um professor chegaria em Modulos por dois caminhos — direto
    /// (Professores -> Modulos) e via curso (Professores -> Cursos -> Modulos, que
    /// já era cascade) — e o SQL Server recusa criar a constraint nesse cenário
    /// ("may cause cycles or multiple cascade paths").
    /// </summary>
    public partial class AdicionaProfessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<int>(
                name: "IdCoordenador",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdProfessor",
                table: "Modulos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Cursos e módulos que já existiam ficariam com IdCoordenador/IdProfessor = 0,
            // e a FK abaixo recusaria a alteração ("conflicted with the FOREIGN KEY
            // constraint"). Como o professor é obrigatório, eles são apontados para um
            // professor provisório — o mesmo que o CsvImporterService usa enquanto a API
            // de professores não existe. Só roda se houver linha para corrigir.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Cursos) OR EXISTS (SELECT 1 FROM Modulos)
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM Professores WHERE Matricula = 'A-DEFINIR')
                        INSERT INTO Professores (Matricula, Nome) VALUES ('A-DEFINIR', 'A definir');

                    DECLARE @IdProvisorio INT =
                        (SELECT TOP 1 IdProfessor FROM Professores WHERE Matricula = 'A-DEFINIR');

                    UPDATE Cursos  SET IdCoordenador = @IdProvisorio WHERE IdCoordenador = 0;
                    UPDATE Modulos SET IdProfessor   = @IdProvisorio WHERE IdProfessor   = 0;
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdCoordenador",
                table: "Cursos",
                column: "IdCoordenador");

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_IdProfessor",
                table: "Modulos",
                column: "IdProfessor");

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
                name: "IX_Cursos_IdCoordenador",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Modulos_IdProfessor",
                table: "Modulos");

            migrationBuilder.DropColumn(
                name: "IdCoordenador",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "IdProfessor",
                table: "Modulos");

            migrationBuilder.DropTable(
                name: "Professores");
        }
    }
}
