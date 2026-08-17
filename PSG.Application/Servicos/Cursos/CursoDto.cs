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
    /// Sigla vem nula: o Curso ainda não tem esse campo no Domain.
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
    /// Módulo dentro do painel de detalhes, com o professor que o ministra.
    /// </summary>
    public sealed record ModuloCursoDto(
        int IdModulo,
        string Nome,
        int Numero,
        string? NomeProfessor,
        int QuantidadeAlunos
    );
}
