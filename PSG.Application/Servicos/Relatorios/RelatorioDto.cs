using PSG.Domain.Enum;

namespace PSG.Application.Servicos.Relatorios
{
    /// <summary>Assunto principal do relatório — define qual seção de filtros a tela mostra.</summary>
    public enum EnumRelatorioTipo
    {
        Alunos,
        Modulos,
        Cursos
    }

    /// <summary>Só usado quando Tipo = Alunos: um aluno específico ou vários por filtro.</summary>
    public enum EnumRelatorioEscopoAluno
    {
        Especifico,
        Filtros
    }

    /// <summary>Comparação da nota, usada quando o filtro de status é Aprovado ou Reprovado.</summary>
    public enum EnumOperadorNota
    {
        MaiorOuIgual,
        MenorOuIgual,
        IgualA
    }

    /// <summary>Presets do filtro de período; Personalizado usa DataInicio/DataFim informadas.</summary>
    public enum EnumRelatorioPeriodo
    {
        SemFiltro,
        UltimaSemana,
        UltimoMes,
        UltimoSemestre,
        Personalizado
    }

    /// <summary>
    /// Todos os filtros da tela de relatórios num único objeto — a mesma combinação
    /// alimenta tanto a prévia quanto o arquivo final, para as duas baterem sempre.
    /// </summary>
    public sealed record RelatorioFiltroDto(
        EnumRelatorioTipo Tipo,
        EnumRelatorioEscopoAluno EscopoAluno,
        int? IdAlunoEspecifico,
        int? IdCurso,
        EnumStatus? Status,
        EnumOperadorNota OperadorNota,
        decimal? Nota,
        bool MostrarAlunosNosModulos,
        EnumRelatorioPeriodo Periodo,
        DateTime? DataInicioPersonalizada,
        DateTime? DataFimPersonalizada
    );

    /// <summary>
    /// Resultado já pronto para exibir (prévia) ou exportar (xlsx/csv): título, o texto do
    /// período usado e a tabela em si, como colunas + linhas de texto — formato simples
    /// o bastante para alimentar HTML, CSV e planilha sem lógica extra em cada exportador.
    /// </summary>
    public sealed record RelatorioResultadoDto(
        string Titulo,
        string? Subtitulo,
        DateTime DataGeracao,
        DateTime? PeriodoInicio,
        DateTime? PeriodoFim,
        List<string> Colunas,
        List<List<string>> Linhas
    );
}
