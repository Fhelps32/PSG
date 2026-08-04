using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            var vm = new DashBoardVM
            {
                TotalAlunos = _alunoService.(),
            };
            return View(vm);
        }
    }
}
