using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Application.Interfaces;
using PSG.Domain;
using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Csv
{
    public class CsvImporterService
    {
        private readonly ICsvReaderService _csvReaderService;
        private readonly IPSGDbContext _context;

        public CsvImporterService(ICsvReaderService csvReaderService, IPSGDbContext context)
        {
            _csvReaderService = csvReaderService;
            _context = context;
        }

        public async Task<ResultadoImportacaoCsv> ImportarAsync(Stream stream)
        {
            var linhas = _csvReaderService.LerCsv(stream);

            // Corrige, quando dá, problemas específicos deste CSV (coluna deslocada,
            // texto solto na coluna errada) ANTES da normalização genérica, pra não
            // perder informação que seria descartada depois.
            foreach (var linha in linhas)
                PreProcessarLinha(linha);

            // Normalização genérica já existente (limpeza de texto, número do módulo, etc.)
            var errosNormalizacao = CsvNormalizer.Normalizar(linhas);
            var linhasComErro = new HashSet<LinhaCsvDto>(errosNormalizacao.Select(e => e.LinhaOriginal));

            // Carrega o que já existe no banco. O Include garante que a navegação
            // Curso venha populada, já que usamos a referência do objeto Curso
            // (e não o Id) como parte da chave de deduplicação nos dicionários abaixo -
            // isso funciona tanto pra cursos já existentes quanto pra cursos novos
            // criados durante este mesmo import (que ainda não têm Id, pois só
            // recebem um após o SaveChangesAsync no final).
            var cursos = await _context.Cursos.ToListAsync();
            var alunos = await _context.Alunos.Include(a => a.Curso).ToListAsync();
            var modulos = await _context.Modulos.Include(m => m.Curso).ToListAsync();

            var cursosPorNome = cursos.ToDictionary(c => c.Nome.Trim().ToUpperInvariant());
            var alunosPorCursoENome = alunos.ToDictionary(a => (a.Curso, Nome: a.Nome.Trim().ToUpperInvariant()));
            var modulosPorCursoENumero = modulos.ToDictionary(m => (m.Curso, m.Numero));

            var errosDominio = new List<LinhaCsvInvalida>();

            var cursosCriados = 0;
            var alunosCriados = 0;
            var modulosCriados = 0;
            var registrosCriados = 0;

            for (var i = 0; i < linhas.Count; i++)
            {
                var linha = linhas[i];
                var numeroLinha = i + 2; // +1 pelo header, +1 porque a lista é 0-based

                if (linhasComErro.Contains(linha))
                    continue; // já reportada pelo CsvNormalizer, não processa de novo

                try
                {
                    var alunoModulo = ProcessarLinha(
                        linha,
                        cursosPorNome, alunosPorCursoENome, modulosPorCursoENumero,
                        ref cursosCriados, ref alunosCriados, ref modulosCriados);

                    _context.AlunoModulos.Add(alunoModulo);
                    registrosCriados++;
                }
                catch (Exception ex)
                {
                    errosDominio.Add(new LinhaCsvInvalida
                    {
                        NumeroLinha = numeroLinha,
                        Motivo = ex.Message,
                        LinhaOriginal = linha
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new ResultadoImportacaoCsv
            {
                TotalLinhas = linhas.Count,
                CursosCriados = cursosCriados,
                AlunosCriados = alunosCriados,
                ModulosCriados = modulosCriados,
                RegistrosCriados = registrosCriados,
                LinhasInvalidas = errosNormalizacao
                    .Concat(errosDominio)
                    .OrderBy(e => e.NumeroLinha)
                    .ToList()
            };
        }

        private AlunoModulo ProcessarLinha(
            LinhaCsvDto linha,
            Dictionary<string, Curso> cursosPorNome,
            Dictionary<(Curso Curso, string Nome), Aluno> alunosPorCursoENome,
            Dictionary<(Curso Curso, int Numero), Modulo> modulosPorCursoENumero,
            ref int cursosCriados, ref int alunosCriados, ref int modulosCriados)
        {
            if (string.IsNullOrWhiteSpace(linha.Curso))
                throw new Exception("Curso não informado.");

            if (string.IsNullOrWhiteSpace(linha.Aluno))
                throw new Exception("Nome do aluno não informado.");

            if (string.IsNullOrWhiteSpace(linha.NomeModulo))
                throw new Exception("Nome do módulo não informado.");

            if (!int.TryParse(linha.NumeroModulo, out var numeroModulo))
                throw new Exception($"Número do módulo inválido: '{linha.NumeroModulo}'.");

            var dataAcesso = CsvParserHelper.ObterData(linha.DataAcesso);
            if (dataAcesso == null)
                throw new Exception($"Data de acesso inválida ou ausente: '{linha.DataAcesso}'.");

            var dataFim = CsvParserHelper.ObterData(linha.DataFim);

            // "Data matríc." no CSV original: às vezes é uma data de fato, às vezes é
            // texto (erro de preenchimento na planilha, ex.: "Recesso até 20/01/19",
            // "2º TENTATIVA"). Quando não dá pra interpretar como data, preserva o
            // texto original como observação em vez de simplesmente descartar.
            var dataMatricula = CsvParserHelper.ObterData(linha.DataMatricula);
            string? obsDataMatricula = null;
            if (dataMatricula == null && !string.IsNullOrWhiteSpace(linha.DataMatricula))
                obsDataMatricula = $"Data matríc. (original, não reconhecida como data): {linha.DataMatricula.Trim()}";

            var curso = ObterOuCriarCurso(linha.Curso, cursosPorNome, ref cursosCriados);

            var aluno = ObterOuCriarAluno(linha.Aluno, linha.Celular, curso, alunosPorCursoENome, ref alunosCriados);

            var modulo = ObterOuCriarModulo(linha.NomeModulo, numeroModulo, curso, modulosPorCursoENumero, ref modulosCriados);

            var (nota, status, obsNota) = ResolverNotaEStatus(linha);

            var alunoModulo = new AlunoModulo(aluno, modulo, dataAcesso.Value, status, dataFim)
            {
                Nota = nota,
                ObsNota = obsNota,
                ObsGeral = string.IsNullOrWhiteSpace(linha.Observacao) ? null : linha.Observacao,
                ObsTempo = string.IsNullOrWhiteSpace(linha.Recuperacao) ? null : linha.Recuperacao,
                DataMatricula = dataMatricula
            };

            if (!string.IsNullOrWhiteSpace(obsDataMatricula))
            {
                alunoModulo.ObsGeral = string.IsNullOrWhiteSpace(alunoModulo.ObsGeral)
                    ? obsDataMatricula
                    : $"{alunoModulo.ObsGeral} | {obsDataMatricula}";
            }

            return alunoModulo;
        }

        private Curso ObterOuCriarCurso(
            string nomeCurso,
            Dictionary<string, Curso> cursosPorNome,
            ref int cursosCriados)
        {
            var chave = nomeCurso.Trim().ToUpperInvariant();

            if (cursosPorNome.TryGetValue(chave, out var curso))
                return curso;

            curso = new Curso(nomeCurso.Trim());
            _context.Cursos.Add(curso);
            cursosPorNome[chave] = curso;
            cursosCriados++;

            return curso;
        }

        private Aluno ObterOuCriarAluno(
            string nomeAluno,
            string? celular,
            Curso curso,
            Dictionary<(Curso Curso, string Nome), Aluno> alunosPorCursoENome,
            ref int alunosCriados)
        {
            var chave = (curso, Nome: nomeAluno.Trim().ToUpperInvariant());

            if (!alunosPorCursoENome.TryGetValue(chave, out var aluno))
            {
                aluno = new Aluno(curso, nomeAluno.Trim());
                _context.Alunos.Add(aluno);
                alunosPorCursoENome[chave] = aluno;
                alunosCriados++;
            }

            // Aproveita pra completar o celular se o aluno ainda não tinha um
            // e esta linha trouxe um valor que realmente parece telefone.
            if (string.IsNullOrWhiteSpace(aluno.Celular) && CsvParserHelper.PareceTelefone(celular))
                aluno.Celular = Regex.Replace(celular!, @"\D", "");

            return aluno;
        }

        private Modulo ObterOuCriarModulo(
            string nomeModulo,
            int numeroModulo,
            Curso curso,
            Dictionary<(Curso Curso, int Numero), Modulo> modulosPorCursoENumero,
            ref int modulosCriados)
        {
            var chave = (curso, numeroModulo);

            if (modulosPorCursoENumero.TryGetValue(chave, out var modulo))
                return modulo;

            modulo = new Modulo(curso, nomeModulo.Trim(), numeroModulo);
            _context.Modulos.Add(modulo);
            modulosPorCursoENumero[chave] = modulo;
            modulosCriados++;

            return modulo;
        }

        /// <summary>
        /// Neste CSV a coluna "Nota" às vezes traz a nota numérica e às vezes traz
        /// texto de status (ex.: "Reprovação por Inatividade", "Aguardando Template").
        /// Junta os sinais textuais espalhados pela linha (Nota-como-texto, Aprovação,
        /// Observação) pra inferir o status via palavras-chave; na ausência de
        /// palavra-chave, cai pro critério numérico (nota >= 7) e por fim, na
        /// ausência total de sinal, assume Em Andamento se não há data de conclusão.
        /// </summary>
        private static (decimal Nota, EnumStatus Status, string? ObsNota) ResolverNotaEStatus(LinhaCsvDto linha)
        {
            var (notaExtraida, textoNota) = CsvParserHelper.ObterNotaOuTexto(linha.Nota);
            var notaFinal = notaExtraida ?? 0m;

            var textoParaAnalise = string.Join(" | ", new[] { textoNota, linha.Aprovacao, linha.Observacao }
                .Where(t => !string.IsNullOrWhiteSpace(t)));

            EnumStatus status;

            if (CsvParserHelper.ContemAlgumaPalavra(textoParaAnalise, "cancel", "desist", "suspens", "renovada", "exclu"))
                status = EnumStatus.Cancelado;
            else if (CsvParserHelper.ContemAlgumaPalavra(textoParaAnalise, "reprov", "inativ"))
                status = EnumStatus.Reprovado;
            else if (CsvParserHelper.ContemAlgumaPalavra(textoParaAnalise, "aprov", "conclu"))
                status = EnumStatus.Aprovado;
            else if (notaExtraida.HasValue)
                status = notaExtraida.Value >= 7 ? EnumStatus.Aprovado : EnumStatus.Reprovado;
            else
                status = string.IsNullOrWhiteSpace(linha.DataFim) ? EnumStatus.EmAndamento : EnumStatus.Aprovado;

            return (notaFinal, status, textoNota);
        }

        /// <summary>
        /// Corrige padrões de sujeira identificados neste CSV específico, ANTES da
        /// normalização genérica (pra não perder texto que seria descartado depois):
        ///   1) Módulo com anotação de "ordem invertida" junto do número.
        ///   2) Nota que "vazou" uma coluna pra direita (Nota vazia + Aprovação com número).
        ///   3) Texto solto na coluna Celular que não é telefone.
        /// </summary>
        private static void PreProcessarLinha(LinhaCsvDto linha)
        {
            if (!string.IsNullOrWhiteSpace(linha.NumeroModulo) &&
                linha.NumeroModulo.Contains("invertido", StringComparison.OrdinalIgnoreCase))
            {
                AdicionarObservacao(linha, "Ordem do módulo invertida (indicado na planilha original).");
            }

            var notaEstaVazia = string.IsNullOrWhiteSpace(linha.Nota);
            var aprovacaoPareceNumero = decimal.TryParse(
                linha.Aprovacao?.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out _);

            if (notaEstaVazia && aprovacaoPareceNumero)
            {
                linha.Nota = linha.Aprovacao;
                linha.Aprovacao = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(linha.Celular) && !CsvParserHelper.PareceTelefone(linha.Celular))
            {
                AdicionarObservacao(linha, $"[Texto originalmente na coluna Celular] {linha.Celular.Trim()}");
                linha.Celular = string.Empty;
            }
        }

        private static void AdicionarObservacao(LinhaCsvDto linha, string texto)
        {
            linha.Observacao = string.IsNullOrWhiteSpace(linha.Observacao)
                ? texto
                : $"{linha.Observacao} | {texto}";
        }
    }
}