namespace PSG.Application.Servicos.Cursos
{
    public sealed record CursoTaxaDto(
        string NomeCurso,
        int Taxa,
        int QuantidadeImportancia,
        int QuantidadeTotal
    );
}
