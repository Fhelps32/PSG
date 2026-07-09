using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PSG.Application.Servicos.Csv
{
    internal static class CsvParserHelper
    {
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

        public static DateTime? ObterData(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            valor = valor.Trim();

            if (DateTime.TryParseExact(
                    valor,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var data))
            {
                return data;
            }

            if (double.TryParse(valor,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var excelDate))
            {
                try
                {
                    return DateTime.FromOADate(excelDate);
                }
                catch
                {
                }
            }

            if (DateTime.TryParse(valor, out data))
                return data;

            return null;
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

            return numeros.Length >= 10 && numeros.Length <= 11;
        }
    }
}