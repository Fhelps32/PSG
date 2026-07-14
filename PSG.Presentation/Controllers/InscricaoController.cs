using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PSG.Application.Servicos.AlunoModulos;
using PSG.Application.Servicos.Csv;
using PSG.Application.Servicos.Cursos;
using PSG.Presentation.Models.Incricao;

namespace PSG.Presentation.Controllers
{
    public class InscricaoController : Controller
    {
        private readonly CursoService cursoService;
        private readonly CsvImporterService csvImporterService;
        private readonly AlunoModuloService alunoModuloService;

        public InscricaoController(CursoService cursoService, CsvImporterService csvImporterService, AlunoModuloService alunoModuloService)
        {
            this.cursoService = cursoService;
            this.csvImporterService = csvImporterService;
            this.alunoModuloService = alunoModuloService;
        }
        public IActionResult Index()
        {
            var cursos = cursoService.GetAllCursosAsync();
            var inscricoes = alunoModuloService.GetAllAlunoModuloAsync();
            var model = new InscricaoIndexViewModel
            {
                Inscricoes = new List<InscricaoItemViewModel>(),
                Cursos = cursos.Result.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.IdCurso.ToString(),
                    Text = c.Nome
                }).ToList(),
            };

            foreach (var inscricao in inscricoes.Result)
            {
                model.Inscricoes.Add(new InscricaoItemViewModel
                {
                    NomeAluno = inscricao.Aluno.Nome,
                    NomeCurso = inscricao.Modulo.Curso.Nome,
                    NomeModulo = inscricao.Modulo.Nome,
                    Nota = inscricao.Nota,
                    EnumStatus = inscricao.StatusInscricao,
                    DataInicio = inscricao.DataAcesso,
                    DataFim = inscricao.DataConclusao
                });
            }
            return View(model);
        }
    }
}
