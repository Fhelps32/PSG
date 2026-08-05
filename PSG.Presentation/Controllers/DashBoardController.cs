using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Servicos.Alunos;
using PSG.Application.Servicos.Cursos;
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

        public IActionResult Index(
            )
        {
            var cursos = _cursoService.ObterTodosOsCursosAsync().Result;

            //line graph
            var lineGraphValues = new List<ValueLineGraph>();
            for (int i = 1; i <= 6; i++)
            {
                var mes = DateTime.Now.AddMonths(-i);
                var info = _alunoService.ObterQuantidadeAlunosPorMes(mes).Result;
                lineGraphValues.Add(new ValueLineGraph
                {
                    QuantidadeAlunos = info.TotalAlunos,
                    Data = info.DataRegistro
                });
            }

            var lineGraphSection = new LineGraphSection
            {
                ValueLineGraph = lineGraphValues,
                Tempo = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Últimos 6 meses" },
                    new SelectListItem { Value = "2", Text = "Últimos 12 meses" },
                    new SelectListItem { Value = "3", Text = "Últimos 24 meses" }
                },
                Cursos = cursos.Select(c => new SelectListItem { Value = c.IdCurso.ToString(), Text = c.Nome }).ToList()
            };

            var viewmodel = new DashBoardVM
            {
                TotalAlunos = _alunoService.ObterQuantidadeTotalAlunosAsync().Result,
                TotalCursos = _cursoService.ObterQuantidadeTotalCursosAsync().Result,
                LineGraphSection = lineGraphSection,
            };
            return View(viewmodel);
        }

        [HttpGet]
        public IActionResult GetPieGraphData(int cursoId)
        {
            //ajeita essa merda aq
            return Json(_alunoService.ObterQuantidadeAlunosPorCursoAsync(cursoId).Result);
        }
    }
}
