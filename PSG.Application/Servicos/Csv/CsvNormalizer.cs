using System.Text.RegularExpressions;

namespace PSG.Application.Servicos.Csv
{

    internal static class CsvNormalizer
    {
        public static List<LinhaCsvInvalida> Normalizar(List<LinhaCsvDto> linhas)
        {
            var erros = new List<LinhaCsvInvalida>();

            for (int i = 0; i < linhas.Count; i++)
            {
                var linha = linhas[i];

                try
                {
                    NormalizarLinha(linha);
                }
                catch (Exception ex)
                {
                    erros.Add(new LinhaCsvInvalida
                    {
                        NumeroLinha = i + 2,
                        Motivo = ex.Message,
                        LinhaOriginal = linha
                    });
                }
            }

            return erros;
        }

        private static void NormalizarLinha(LinhaCsvDto linha)
        {
            linha.Curso = CsvParserHelper.LimparTexto(linha.Curso);

            linha.NomeModulo = CsvParserHelper.LimparTexto(linha.NomeModulo);

            linha.Aluno = CsvParserHelper.LimparTexto(linha.Aluno);

            linha.Observacao = CsvParserHelper.LimparTexto(linha.Observacao);

            linha.Recuperacao = CsvParserHelper.LimparTexto(linha.Recuperacao);

            linha.Aprovacao = CsvParserHelper.LimparTexto(linha.Aprovacao);

            linha.NumeroModulo = NormalizarModulo(linha.NumeroModulo);

            linha.Nota = NormalizarNota(linha.Nota);

            linha.Celular = NormalizarCelular(linha.Celular);
        }

        private static string NormalizarModulo(string modulo)
        {
            if (string.IsNullOrWhiteSpace(modulo))
                return "";

            var numero = CsvParserHelper.ObterNumeroModulo(modulo);

            if (numero == null)
                throw new Exception($"Número do módulo inválido: {modulo}");

            return numero.ToString()!;
        }

        private static string NormalizarNota(string nota)
        {
            if (string.IsNullOrWhiteSpace(nota))
                return "";

            nota = nota.Replace(",", ".");

            return nota;
        }

        private static string? NormalizarCelular(string? celular)
        {
            if (string.IsNullOrWhiteSpace(celular))
                return null;

            // Remove espaços
            celular = celular.Trim();

            // Algumas linhas do seu CSV possuem textos nessa coluna
            // ("Reprovado", "Cancelado", etc.)
            if (!CsvParserHelper.PareceTelefone(celular))
                return null;

            // Mantém apenas os números
            celular = Regex.Replace(celular, @"\D", "");

            return celular;
        }
    }
}