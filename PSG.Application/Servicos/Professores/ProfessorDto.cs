namespace PSG.Application.Servicos.Professores
{
    /// <summary>Professor como aparece nos dropdowns e listagens.</summary>
    public sealed record ProfessorDto(
        int IdProfessor,
        string Matricula,
        string Nome
    );
}
