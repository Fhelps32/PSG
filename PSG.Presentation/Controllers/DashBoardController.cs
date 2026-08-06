using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Servicos.Alunos;
using PSG.Application.Servicos.Cursos;
using PSG.Domain;
using PSG.Presentation.Models.DashBoard;

namespace PSG.Presentation.Controllers
{
    public class DashBoardController(
        AlunoService alunoService,
        CursoService cursoService
        ) : Controller
    {
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
            for (int i = 1; i <= 6; i++)
            {
                var mes = DateTime.Now.AddMonths(-i);
                var info = await _alunoService.ObterQuantidadeAlunosPorMes(mes, null);
                lineGraphValues.Add(new ValueLineGraph { QuantidadeAlunos = info.TotalAlunos, Data = info.DataRegistro });
            }

            var lineGraphSection = new LineGraphSection
            {
                ValueLineGraph = lineGraphValues,
                Tempo = new List<SelectListItem>
                {
                    new() { Value = "6", Text = "Últimos 6 meses" },
                    new() { Value = "12", Text = "Últimos 12 meses" },
                    new() { Value = "24", Text = "Últimos 24 meses" }
                },
                Cursos = cursosSelectList
            };

            // Sector (pie) graph — FALTAVA ISSO
            var valuesSectorGraph = new List<ValueSectorGraph>();
            foreach (var curso in cursos)
            {
                var value = await _alunoService.ObterQuantidadeAlunosPorCursoAsync(curso.IdCurso);
                valuesSectorGraph.Add(new ValueSectorGraph { QuantidadeAlunos = value.TotalAlunos, Curso = value.NomeCurso, Modulo = value.NomeCurso });
            } 
            // ajuste conforme sua service
            var sectorGraphSection = new SectorGraphSection
            {
                ValueSectorGraph = valuesSectorGraph,
                Cursos = cursosSelectList
            };

            var viewmodel = new DashBoardVM
            {
                TotalAlunos = await _alunoService.ObterQuantidadeTotalAlunosAsync(),
                TotalCursos = await _cursoService.ObterQuantidadeTotalCursosAsync(),
                LineGraphSection = lineGraphSection,
                SectorGraphSection = sectorGraphSection,
                CursoMaiorCancelamento = null,
                CursoMaiorReprovacao = null,
                AlunosCancelados = new List<AlunoCanceladoItem>()
            };
            return View(viewmodel);
        }

        [HttpGet]
        public IActionResult GetPieGraphData(int cursoId)
        {
            //ajeita essa merda aq
            return Json(_alunoService.ObterQuantidadeAlunosPorCursoAsync(cursoId));

        }

        [HttpGet("GetLineGraphData")]
        public IActionResult GetLineGraphData(int tempo, int? cursoId)
        {
            var lineGraphValues = new List<AlunoQuantidadeDto>();
            for (int i = 1; i <= tempo; i++)
            {
                var mes = DateTime.Now.AddMonths(-i);
                var info = _alunoService.ObterQuantidadeAlunosPorMes(mes, cursoId).Result;
                lineGraphValues.Add(info);
            }
            return Json(lineGraphValues);
        }
    }
}