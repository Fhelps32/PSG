using ClosedXML.Excel;
using PSG.Application.Interfaces;
using PSG.Application.Servicos.Relatorios;
using System.Text;

namespace PSG.Infra.Data.Relatorios
{
    public class RelatorioExportService : IRelatorioExportService
    {
        /// <summary>
        /// CSV com ; como separador (não ,): é o que o Excel em português espera —
        /// com vírgula ele abre tudo numa coluna só. As duas primeiras linhas trazem
        /// o título e "gerado em/período" como metadado; ferramentas que só querem a
        /// tabela pura devem pular as 2 primeiras linhas antes do cabeçalho.
        /// </summary>
        public byte[] GerarCsv(RelatorioResultadoDto resultado)
        {
            var sb = new StringBuilder();

            sb.AppendLine(CampoCsv(resultado.Titulo));
            sb.AppendLine(CampoCsv(TextoCabecalho(resultado)));
            sb.AppendLine();
            sb.AppendLine(string.Join(';', resultado.Colunas.Select(CampoCsv)));

            foreach (var linha in resultado.Linhas)
            {
                sb.AppendLine(string.Join(';', linha.Select(CampoCsv)));
            }

            // BOM UTF-8: sem ele o Excel abre acentos como caracteres errados.
            var preamble = Encoding.UTF8.GetPreamble();
            var conteudo = Encoding.UTF8.GetBytes(sb.ToString());
            return preamble.Concat(conteudo).ToArray();
        }

        public byte[] GerarXlsx(RelatorioResultadoDto resultado)
        {
            using var workbook = new XLWorkbook();
            var planilha = workbook.Worksheets.Add(NomeAbaValido(resultado.Titulo));

            planilha.Cell(1, 1).Value = resultado.Titulo;
            planilha.Cell(1, 1).Style.Font.Bold = true;
            planilha.Cell(1, 1).Style.Font.FontSize = 13;

            planilha.Cell(2, 1).Value = TextoCabecalho(resultado);
            planilha.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#64748b"); // slate-500

            const int linhaCabecalhoTabela = 4;
            for (var i = 0; i < resultado.Colunas.Count; i++)
            {
                var celula = planilha.Cell(linhaCabecalhoTabela, i + 1);
                celula.Value = resultado.Colunas[i];
                celula.Style.Font.Bold = true;
                celula.Style.Fill.BackgroundColor = XLColor.FromHtml("#eff4ff"); // brand-light
            }

            for (var l = 0; l < resultado.Linhas.Count; l++)
            {
                var linha = resultado.Linhas[l];
                for (var c = 0; c < linha.Count; c++)
                {
                    planilha.Cell(linhaCabecalhoTabela + 1 + l, c + 1).Value = linha[c];
                }
            }

            if (resultado.Colunas.Count > 0)
            {
                planilha.Range(linhaCabecalhoTabela, 1, linhaCabecalhoTabela, resultado.Colunas.Count)
                    .SetAutoFilter();
                planilha.Columns(1, resultado.Colunas.Count).AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static string TextoCabecalho(RelatorioResultadoDto resultado)
        {
            var periodo = resultado.PeriodoInicio.HasValue
                ? $"Período: {resultado.PeriodoInicio:dd/MM/yyyy} a {(resultado.PeriodoFim ?? DateTime.Today):dd/MM/yyyy}"
                : "Período: sem filtro (todos os registros)";

            var texto = $"Gerado em {resultado.DataGeracao:dd/MM/yyyy HH:mm} · {periodo}";
            return string.IsNullOrWhiteSpace(resultado.Subtitulo) ? texto : $"{resultado.Subtitulo} · {texto}";
        }

        private static string CampoCsv(string valor)
        {
            // Aspas quando o campo tem ;, aspas ou quebra de linha — regra padrão de CSV.
            if (valor.IndexOfAny(new[] { ';', '"', '\n', '\r' }) < 0)
            {
                return valor;
            }
            return $"\"{valor.Replace("\"", "\"\"")}\"";
        }

        // Nome de aba do Excel: no máx. 31 caracteres e sem os símbolos que o Excel rejeita.
        private static string NomeAbaValido(string titulo)
        {
            var limpo = new string(titulo.Where(c => !"[]:*?/\\".Contains(c)).ToArray());
            return limpo.Length > 31 ? limpo[..31] : limpo;
        }
    }
}
