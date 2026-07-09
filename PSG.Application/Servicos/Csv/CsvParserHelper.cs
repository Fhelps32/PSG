using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PSG.Application.Servicos.Csv
{
    internal static class CsvParserHelper
    {
        private static readonly Dictionary<string, int> MesesAbreviados = new()
        {
            { "jan", 1 }, { "fev", 2 }, { "mar", 3 }, { "abr", 4 },
            { "mai", 5 }, { "jun", 6 }, { "jul", 7 }, { "ago", 8 },
            { "set", 9 }, { "out", 10 }, { "nov", 11 }, { "dez", 12 }
        };

        public static int? ObterNumeroModulo(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            var match = Regex.Match(valor, @"\d+");

            if (!match.Success)
                return null;

            return int.Parse(match.Value);
        }

        public static decimal? ObterNota(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            valor = valor.Replace(",", ".");

            if (decimal.TryParse(
                valor,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var nota))
            {
                return nota;
            }

            return null;
        }

        /// <summary>
        /// Tenta interpretar o valor como uma nota numérica. Quando não é possível
        /// (ex.: "Reprovação por Inatividade", "Aguardando Template"), devolve o
        /// texto original em TextoNaoNumerico para que não seja perdido — o chamador
        /// decide o que fazer com ele (normalmente vira observação e ajuda a
        /// resolver o status da matrícula no módulo).
        /// </summary>
        public static (decimal? Nota, string? TextoNaoNumerico) ObterNotaOuTexto(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return (null, null);

            var nota = ObterNota(valor);

            return nota != null ? (nota, null) : (null, valor.Trim());
        }

        public static DateTime? ObterData(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            valor = valor.Trim();

            // 1) formato "padrão" da planilha
            if (DateTime.TryParseExact(
                    valor, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                return data;

            // 2) variações com traço em vez de barra, e ano com 2 dígitos
            if (DateTime.TryParseExact(
                    valor, new[] { "dd-MM-yyyy", "dd-MM-yy", "d-M-yyyy", "d-M-yy" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
                return data;

            // 3) dia/mês com 1 dígito, ano com 2 dígitos
            if (DateTime.TryParseExact(
                    valor, new[] { "d/M/yyyy", "d/M/yy", "dd/MM/yy" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
                return data;

            // 4) mês abreviado em português (ex.: "10/abr/24")
            var comMesAbreviado = TentarParseMesAbreviado(valor);
            if (comMesAbreviado != null)
                return comMesAbreviado;

            // 5) número serial de data do Excel (planilha exportada errado).
            //    Faixa aproximada 2009-2036, suficiente pro período do CSV,
            //    evitando interpretar qualquer número solto como data.
            if (double.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var serial)
                && serial is > 40000 and < 50000)
            {
                try
                {
                    return DateTime.FromOADate(serial);
                }
                catch
                {
                    // serial fora de um intervalo válido pro DateTime - ignora
                }
            }

            // 6) extrai a primeira data dd/mm/yyyy (ou dd-mm-yyyy) de dentro de um
            //    texto maior - cobre intervalos ("28/04/21 - 11/05/21") e frases
            //    ("entre 06 e 15/06/2022", "13/01/2023 18/01/2023").
            var match = Regex.Match(valor, @"\b(\d{1,2})[/-](\d{1,2})[/-](\d{2,4})\b");
            if (match.Success)
            {
                var trecho = match.Value.Replace('-', '/');
                if (DateTime.TryParse(trecho, CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
                    return data;
            }

            // 7) último recurso: deixa o .NET tentar interpretar sozinho
            if (DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
                return data;

            return null;
        }

        private static DateTime? TentarParseMesAbreviado(string valor)
        {
            var match = Regex.Match(valor, @"(\d{1,2})/([A-Za-zçÇ]{3})/(\d{2,4})");
            if (!match.Success)
                return null;

            var mesTexto = RemoverAcentos(match.Groups[2].Value).ToLowerInvariant();
            if (!MesesAbreviados.TryGetValue(mesTexto, out var mes))
                return null;

            if (!int.TryParse(match.Groups[1].Value, out var dia))
                return null;

            if (!int.TryParse(match.Groups[3].Value, out var ano))
                return null;

            if (ano < 100)
                ano += 2000;

            try
            {
                return new DateTime(ano, mes, dia);
            }
            catch
            {
                return null;
            }
        }

        public static string LimparTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            return Regex.Replace(texto.Trim(), @"\s+", " ");
        }

        public static bool PareceTelefone(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            var numeros = Regex.Replace(valor, @"\D", "");

            return numeros.Length is >= 10 and <= 11;
        }

        public static string RemoverAcentos(string? texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalizado)
            {
                var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Verifica, sem acentos e sem diferenciar maiúsculas/minúsculas, se o texto
        /// contém alguma das palavras-chave informadas. Usado para inferir o status
        /// da matrícula a partir de anotações livres espalhadas pelo CSV
        /// (ex.: "REPROV.", "Cancelou matrícula", "Desistência").
        /// </summary>
        public static bool ContemAlgumaPalavra(string? texto, params string[] palavras)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            var normalizado = RemoverAcentos(texto).ToLowerInvariant();

            return palavras.Any(p => normalizado.Contains(p));
        }
    }
}