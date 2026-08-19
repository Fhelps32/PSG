namespace PSG.Presentation.Models.Relatorio
{
    /// <summary>
    /// A tabela pronta pra exibir na prévia (partial _PreviewRelatorioPartial).
    /// A exportação (xlsx/csv) não passa por esta VM — vai direto do
    /// RelatorioResultadoDto para o arquivo, então a prévia é só uma amostra.
    /// </summary>
    public class RelatorioResultadoVM
    {
        // Quando true, os filtros não formam uma combinação válida (ex.: "Alunos" +
        // "específico" sem escolher ninguém) — a partial mostra Mensagem em vez da tabela.
        public bool TemErro { get; set; }
        public string? Mensagem { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string? Subtitulo { get; set; }
        public string GeradoEmTexto { get; set; } = string.Empty;
        public string PeriodoTexto { get; set; } = string.Empty;

        public List<string> Colunas { get; set; } = new List<string>();
        public List<List<string>> Linhas { get; set; } = new List<List<string>>();

        // Total real de linhas do relatório — pode ser maior que Linhas.Count quando
        // a prévia é limitada; o arquivo exportado sempre traz todas.
        public int TotalLinhas { get; set; }
        public bool Truncado => TotalLinhas > Linhas.Count;

        public static RelatorioResultadoVM DeErro(string mensagem) =>
            new() { TemErro = true, Mensagem = mensagem };
    }
}
