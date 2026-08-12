namespace PSG.Application.Servicos.Cursos
{
    public sealed record CursoTaxaDto(
        string NomeCurso,
        int Taxa,
        int QuantidadeImportancia,
        int QuantidadeTotal
    );

    /// <summary>
    /// Uma linha da listagem de cursos (coluna da esquerda da tela de Cursos).
    /// Sigla e NomeCoordenador vêm nulos: nenhum dos dois existe no Domain hoje
    /// (Curso não tem sigla e não há entidade Professor/Coordenador).
    /// </summary>
    public sealed record CursoListagemDto(
        int IdCurso,
        string Nome,
        string? Sigla,
        string? NomeCoordenador,
        int QuantidadeAlunos,
        int TaxaCancelamento
    );

    /// <summary>
    /// Painel de detalhes do curso selecionado, com os módulos dele.
    /// </summary>
    public sealed record CursoDetalhesDto(
        int IdCurso,
        string Nome,
        string? Sigla,
        int QuantidadeAlunos,
        int QuantidadeAlunosCancelados,
        int QuantidadeAlunosReprovados,
        int TaxaCancelamento,
        int TaxaReprovacao,
        List<ModuloCursoDto> Modulos
    );

    /// <summary>
    /// Módulo dentro do painel de detalhes. NomeProfessor vem nulo pelo mesmo
    /// motivo do coordenador: não existe entidade Professor no Domain.
    /// </summary>
    public sealed record ModuloCursoDto(
        int IdModulo,
        string Nome,
        int Numero,
        string? NomeProfessor,
        int QuantidadeAlunos
    );
}
