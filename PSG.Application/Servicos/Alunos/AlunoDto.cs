using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Alunos
{
    public sealed record AlunoDto(
        int IdAluno,
        int IdCurso,
        string Matricula,
        string Nome
    );

    public sealed record AlunoDtoDetalhado( 
        int IdAluno,
        int IdCurso,
        string Matricula,
        string Nome,
        DateTime DataCadastro,
        IEnumerable<AlunoModuloDto> Modulos
    );

    public sealed record AlunoDtoCriar(
        int IdCurso,
        string Matricula,
        string Nome
    );

    public sealed record AlunoModuloDto(    
        int IdAlunoModulo,
        int IdAluno,
        int IdModulo,
        decimal Nota,
        EnumStatus Status
    );

    public sealed record AlunoQuantidadeDto(
        int TotalAlunos,
        DateTime DataRegistro
    );

    public sealed record AlunoQuantidadeCursoDto(
        int TotalAlunos,
        string NomeCurso,
        DateTime DataRegistro
    );

    public sealed record AlunoQuantidadeModuloDto(
        int TotalAlunos,
        string NomeCurso,
        string NomeModulo
    );

    public sealed record AlunoCanceladoDto(
        string Nome,
        string NomeCurso,
        string NomeModulo,
        DateTime DataCancelamento
    );

    /// <summary>
    /// Situação do aluno no curso. Não é persistida: sai da combinação das
    /// inscrições dele (ver AlunoService.CalcularStatusAluno).
    /// </summary>
    public enum EnumStatusAluno
    {
        Cursando,
        Finalizado,
        EmEspera
    }

    /// <summary>Uma linha da listagem de alunos.</summary>
    public sealed record AlunoListagemDto(
        int IdAluno,
        string Nome,
        string? Matricula,
        string NomeCurso,
        // Módulo em andamento de maior número; null quando o aluno não está cursando nada.
        string? ModuloAtual,
        EnumStatusAluno Status
    );

    /// <summary>Painel de detalhes do aluno selecionado.</summary>
    public sealed record AlunoDetalhesDto(
        int IdAluno,
        string Nome,
        string? Matricula,
        string NomeCurso,
        EnumStatusAluno Status,
        int ModulosConcluidos,
        int TotalModulosCurso,
        List<AlunoModuloDetalheDto> Modulos,
        List<AlunoModuloDetalheDto> ModulosReprovados
    );

    /// <summary>Uma inscrição do aluno, como aparece no painel de detalhes.</summary>
    public sealed record AlunoModuloDetalheDto(
        int IdModulo,
        string NomeModulo,
        int NumeroModulo,
        DateTime? DataInscricao,
        DateTime? DataFim,
        decimal Nota,
        EnumStatus Status
    );
}
