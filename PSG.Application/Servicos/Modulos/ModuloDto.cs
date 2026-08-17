using PSG.Application.Servicos.Alunos;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Modulos
{
    public sealed record ModuloDto(
        int IdModulo,
        int IdCurso,
        string Nome,
        int Numero
    );

    public sealed record ModuloDtoDetalhado(
        int IdModulo,
        int IdCurso,
        string Nome,
        int Numero,
        DateTime DataCadastro,
        Curso Curso,
        IEnumerable<AlunoModuloDto> Alunos
    );

    public sealed record ModuloDtoCriar(
        Curso Curso,
        string Nome,
        int Numero
    );

    /// <summary>Campos editáveis de um módulo (usado pela modal de edição).</summary>
    public sealed record ModuloDtoAtualizar(
        string Nome,
        int Numero,
        int IdProfessor
    );

    /// <summary>
    /// Dados que a modal de edição precisa exibir: os campos do módulo mais o
    /// nome do curso, que aparece como contexto (não editável).
    /// </summary>
    public sealed record ModuloEdicaoDto(
        int IdModulo,
        int IdCurso,
        string NomeCurso,
        string Nome,
        int Numero,
        int IdProfessor,
        string NomeProfessor
    );
}
