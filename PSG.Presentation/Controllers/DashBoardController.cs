using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Servicos.Alunos;
using PSG.Application.Servicos.Cursos;
using PSG.Domain.Enum;
using PSG.Presentation.Models.DashBoard;

namespace PSG.Presentation.Controllers
{
    public class DashBoardController(
        AlunoService alunoService,
        CursoService cursoService
        ) : Controller
    {
        private const int MesesPadrao = 6;

        private readonly AlunoService _alunoService = alunoService;
        private readonly CursoService _cursoService = cursoService;

        public async Task<IActionResult> Index()
        {
            var cursos = await _cursoService.ObterTodosOsCursosAsync();
            var cursosSelectList = cursos
                .Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome })
                .ToList();

            // Line graph
            var lineGraphValues = new List<ValueLineGraph>();
            foreach (var mes in UltimosMeses(MesesPadrao))
            {
                var info = await _alunoService.ObterQuantidadeAlunosPorMes(mes, null);
                lineGraphValues.Add(new ValueLineGraph { QuantidadeAlunos = info.TotalAlunos, Data = info.DataRegistro });
            }

            var lineGraphSection = new LineGraphSection
            {
                ValueLineGraph = lineGraphValues,
                Tempo = new List<SelectListItem>
                {
                    new() { Value = "6", Text = "Últimos 6 meses", Selected = true },
                    new() { Value = "12", Text = "Últimos 12 meses" },
                    new() { Value = "24", Text = "Últimos 24 meses" }
                },
                Cursos = cursosSelectList
            };

            // Sector (pie) graph: uma fatia por curso.
            var valuesSectorGraph = (await _alunoService.ObterQuantidadeAlunosPorCursoAsync())
                .Select(v => new ValueSectorGraph { QuantidadeAlunos = v.TotalAlunos, Curso = v.NomeCurso })
                .ToList();

            var sectorGraphSection = new SectorGraphSection
            {
                ValueSectorGraph = valuesSectorGraph,
                Cursos = cursosSelectList
            };

            var cursoMaiorCancelamento = await _cursoService.ObterCursoComMaiorTaxaAsync(EnumStatus.Cancelado);
            var cursoMaiorReprovacao = await _cursoService.ObterCursoComMaiorTaxaAsync(EnumStatus.Reprovado);
            var alunosCancelados = await _alunoService.ObterAlunosCanceladosAsync();

            var viewmodel = new DashBoardVM
            {
                TotalAlunos = await _alunoService.ObterQuantidadeTotalAlunosAsync(),
                TotalCursos = await _cursoService.ObterQuantidadeTotalCursosAsync(),
                LineGraphSection = lineGraphSection,
                SectorGraphSection = sectorGraphSection,
                CursoMaiorCancelamento = MapearImportancia(cursoMaiorCancelamento),
                CursoMaiorReprovacao = MapearImportancia(cursoMaiorReprovacao),
                AlunosCancelados = alunosCancelados
                    .Select(a => new AlunoCanceladoItem
                    {
                        Nome = a.Nome,
                        Curso = a.NomeCurso,
                        Modulo = a.NomeModulo,
                        DataCancelamento = a.DataCancelamento
                    })
                    .ToList()
            };
            return View(viewmodel);
        }

        [HttpGet]
        public async Task<IActionResult> GetPieGraphData(int? cursoId)
        {
            // Sem curso selecionado a pizza mostra a distribuição entre os cursos;
            // com um curso selecionado ela detalha os módulos daquele curso.
            if (cursoId is null or 0)
            {
                var porCurso = await _alunoService.ObterQuantidadeAlunosPorCursoAsync();
                return Json(porCurso.Select(c => new
                {
                    curso = c.NomeCurso,
                    modulo = (string?)null,
                    quantidadeAlunos = c.TotalAlunos
                }));
            }

            var porModulo = await _alunoService.ObterQuantidadeAlunosPorModuloAsync(cursoId.Value);
            return Json(porModulo.Select(m => new
            {
                curso = m.NomeCurso,
                modulo = m.NomeModulo,
                quantidadeAlunos = m.TotalAlunos
            }));
        }

        [HttpGet]
        public async Task<IActionResult> GetLineGraphData(int tempo, int? cursoId)
        {
            var meses = tempo is > 0 and <= 60 ? tempo : MesesPadrao;

            var lineGraphValues = new List<AlunoQuantidadeDto>();
            foreach (var mes in UltimosMeses(meses))
            {
                lineGraphValues.Add(await _alunoService.ObterQuantidadeAlunosPorMes(mes, cursoId));
            }
            return Json(lineGraphValues);
        }

        /// <summary>
        /// Meses em ordem cronológica (mais antigo primeiro), terminando no mês corrente.
        /// </summary>
        private static IEnumerable<DateTime> UltimosMeses(int quantidade)
        {
            var mesAtual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            for (var i = quantidade - 1; i >= 0; i--)
            {
                yield return mesAtual.AddMonths(-i);
            }
        }

        private static CursoImportanciaItem? MapearImportancia(CursoTaxaDto? dto) =>
            dto is null
                ? null
                : new CursoImportanciaItem
                {
                    Curso = dto.NomeCurso,
                    Taxa = dto.Taxa,
                    QuantidadeAlunosImportancia = dto.QuantidadeImportancia,
                    QuantidadeAlunosTotal = dto.QuantidadeTotal
                };
    }
}
