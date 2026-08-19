using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Interfaces;
using PSG.Application.Servicos.Alunos;
using PSG.Application.Servicos.Cursos;
using PSG.Application.Servicos.Relatorios;
using PSG.Domain.Enum;
using PSG.Presentation.Models.Relatorio;

namespace PSG.Presentation.Controllers
{
    public class RelatorioController(
        RelatorioService relatorioService,
        IRelatorioExportService exportService,
        CursoService cursoService,
        AlunoService alunoService
        ) : Controller
    {
        private readonly RelatorioService _relatorioService = relatorioService;
        private readonly IRelatorioExportService _exportService = exportService;
        private readonly CursoService _cursoService = cursoService;
        private readonly AlunoService _alunoService = alunoService;

        // Quantas linhas a prévia mostra no máximo. O arquivo exportado (xlsx/csv)
        // nunca passa por este limite — usa sempre o relatório completo.
        private const int LinhasNaPreVia = 50;

        public async Task<IActionResult> Index()
        {
            var cursos = await _cursoService.ObterTodosOsCursosAsync();
            var nomeCursoPorId = cursos.ToDictionary(c => c.IdCurso, c => c.Nome);

            var alunos = await _alunoService.ObterTodosAlunosAsync();

            // Estado inicial dos filtros — precisa bater com os valores marcados por
            // padrão nos campos da view, senão a prévia da primeira carga mostraria
            // algo diferente do que os filtros na tela dizem estar selecionados.
            var (_, filtroInicial) = ValidarEMontarFiltro(
                EnumRelatorioTipo.Alunos, EnumRelatorioEscopoAluno.Filtros,
                null, null, null, EnumOperadorNota.MaiorOuIgual, null, false,
                EnumRelatorioPeriodo.SemFiltro, null, null);
            var resultadoInicial = await _relatorioService.GerarRelatorioAsync(filtroInicial!);

            var viewModel = new RelatorioIndexVM
            {
                Cursos = cursos
                    .OrderBy(c => c.Nome)
                    .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome })
                    .ToList(),

                // "Nome (Matrícula) — Curso": o mesmo aluno pode ter homônimos, e o
                // curso ajuda a identificar de cara sem abrir mais nada.
                Alunos = alunos
                    .OrderBy(a => a.Nome)
                    .Select(a => new SelectListItem
                    {
                        Value = a.IdAluno.ToString(),
                        Text = $"{a.Nome} ({a.Matricula}) — {nomeCursoPorId.GetValueOrDefault(a.IdCurso, "curso não encontrado")}"
                    })
                    .ToList(),

                Preview = MapearParaVM(resultadoInicial)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Prévia: monta o relatório com os filtros atuais e devolve só as primeiras
        /// linhas, para o usuário conferir antes de exportar. Chamada por fetch a
        /// cada mudança de filtro na tela — por isso GET com parâmetros soltos (a
        /// mesma forma como o resto do app já faz filtro via query string) em vez de
        /// receber um objeto de filtro só, que exigiria um binder à parte.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Preview(
            EnumRelatorioTipo tipo = EnumRelatorioTipo.Alunos,
            EnumRelatorioEscopoAluno escopo = EnumRelatorioEscopoAluno.Filtros,
            int? idAluno = null,
            int? idCurso = null,
            EnumStatus? status = null,
            EnumOperadorNota operadorNota = EnumOperadorNota.MaiorOuIgual,
            decimal? nota = null,
            bool mostrarAlunos = false,
            EnumRelatorioPeriodo periodo = EnumRelatorioPeriodo.SemFiltro,
            DateTime? dataInicio = null,
            DateTime? dataFim = null)
        {
            var (erro, filtro) = ValidarEMontarFiltro(tipo, escopo, idAluno, idCurso, status,
                operadorNota, nota, mostrarAlunos, periodo, dataInicio, dataFim);

            if (erro is not null)
            {
                return PartialView("_PreviewRelatorioPartial", RelatorioResultadoVM.DeErro(erro));
            }

            var resultado = await _relatorioService.GerarRelatorioAsync(filtro!);
            return PartialView("_PreviewRelatorioPartial", MapearParaVM(resultado));
        }

        /// <summary>
        /// Gera o arquivo (xlsx ou csv) com o relatório completo — mesma combinação de
        /// filtros da prévia, sem o limite de linhas. Chamada por fetch (não por um
        /// submit de formulário comum) para poder mostrar erro de validação sem sair
        /// da página; o download em si é disparado pelo JS a partir do blob da resposta.
        /// </summary>
        /// <param name="modoRapido">
        /// Botão "Gerar relatório rápido": gera com os filtros já configurados na tela,
        /// mas sempre em xlsx — ignora o rádio de formato, então o usuário nem precisa
        /// tê-lo marcado.
        /// </param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Gerar(
            EnumRelatorioTipo tipo,
            EnumRelatorioEscopoAluno escopo,
            int? idAluno,
            int? idCurso,
            EnumStatus? status,
            EnumOperadorNota operadorNota,
            decimal? nota,
            bool mostrarAlunos,
            EnumRelatorioPeriodo periodo,
            DateTime? dataInicio,
            DateTime? dataFim,
            string formato = "xlsx",
            bool modoRapido = false)
        {
            var (erro, filtro) = ValidarEMontarFiltro(tipo, escopo, idAluno, idCurso, status,
                operadorNota, nota, mostrarAlunos, periodo, dataInicio, dataFim);

            if (erro is not null)
            {
                // ContentResult com texto puro, não BadRequest(erro): esse último devolve a
                // mensagem como JSON (com aspas em volta), e o JS só lê resp.text() puro.
                return new ContentResult
                {
                    Content = erro,
                    ContentType = "text/plain; charset=utf-8",
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var resultado = await _relatorioService.GerarRelatorioAsync(filtro!);
            var formatoFinal = modoRapido ? "xlsx" : formato;

            var (bytes, contentType, extensao) = formatoFinal == "csv"
                ? (_exportService.GerarCsv(resultado), "text/csv", "csv")
                : (_exportService.GerarXlsx(resultado),
                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx");

            var nomeArquivo = $"{Slug(resultado.Titulo)}_{DateTime.Now:yyyyMMdd_HHmm}.{extensao}";
            return File(bytes, contentType, nomeArquivo);
        }

        /// <summary>
        /// Confere se a combinação de filtros faz sentido antes de gerar o relatório
        /// (aluno específico sem aluno escolhido, período personalizado sem data de
        /// início, data final antes da inicial) e, quando faz, monta o DTO que o
        /// RelatorioService usa. Compartilhada por Preview e Gerar — as duas telas
        /// nunca podem divergir sobre o que é ou não um filtro válido.
        /// </summary>
        private static (string? Erro, RelatorioFiltroDto? Filtro) ValidarEMontarFiltro(
            EnumRelatorioTipo tipo,
            EnumRelatorioEscopoAluno escopo,
            int? idAluno,
            int? idCurso,
            EnumStatus? status,
            EnumOperadorNota operadorNota,
            decimal? nota,
            bool mostrarAlunos,
            EnumRelatorioPeriodo periodo,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            if (tipo == EnumRelatorioTipo.Alunos && escopo == EnumRelatorioEscopoAluno.Especifico && !idAluno.HasValue)
            {
                return ("Selecione o aluno.", null);
            }

            if (periodo == EnumRelatorioPeriodo.Personalizado && !dataInicio.HasValue)
            {
                return ("Informe a data de início do período personalizado.", null);
            }

            if (dataInicio.HasValue && dataFim.HasValue && dataFim.Value.Date < dataInicio.Value.Date)
            {
                return ("A data final não pode ser anterior à data inicial.", null);
            }

            var filtro = new RelatorioFiltroDto(
                tipo, escopo, idAluno, idCurso, status, operadorNota, nota,
                mostrarAlunos, periodo, dataInicio, dataFim);

            return (null, filtro);
        }

        private static RelatorioResultadoVM MapearParaVM(RelatorioResultadoDto resultado)
        {
            var periodoTexto = resultado.PeriodoInicio.HasValue
                ? $"{resultado.PeriodoInicio:dd/MM/yyyy} a {(resultado.PeriodoFim ?? DateTime.Today):dd/MM/yyyy}"
                : "sem filtro (todos os registros)";

            return new RelatorioResultadoVM
            {
                Titulo = resultado.Titulo,
                Subtitulo = resultado.Subtitulo,
                GeradoEmTexto = resultado.DataGeracao.ToString("dd/MM/yyyy HH:mm"),
                PeriodoTexto = periodoTexto,
                Colunas = resultado.Colunas,
                Linhas = resultado.Linhas.Take(LinhasNaPreVia).ToList(),
                TotalLinhas = resultado.Linhas.Count
            };
        }

        // Nome de arquivo amigável a partir do título ("Relatório de alunos" -> "relatorio-de-alunos").
        private static string Slug(string texto)
        {
            var normalizado = texto.Normalize(System.Text.NormalizationForm.FormD);
            var semAcento = new string(normalizado
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray());

            var slug = System.Text.RegularExpressions.Regex.Replace(semAcento.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
            return string.IsNullOrEmpty(slug) ? "relatorio" : slug;
        }
    }
}
