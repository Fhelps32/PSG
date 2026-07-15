using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Servicos.AlunoModulos;
using PSG.Application.Servicos.Alunos;
using PSG.Application.Servicos.Csv;
using PSG.Application.Servicos.Cursos;
using PSG.Application.Servicos.Modulos;
using PSG.Domain;
using PSG.Domain.Enum;
using PSG.Presentation.Models.Incricao;

namespace PSG.Presentation.Controllers
{
    public class InscricaoController : Controller
    {
        private readonly CursoService cursoService;
        private readonly CsvImporterService csvImporterService;
        private readonly AlunoModuloService alunoModuloService;
        private readonly ModuloService moduloService;
        private readonly AlunoService alunoService;

        public InscricaoController(
            CursoService cursoService, 
            CsvImporterService csvImporterService, 
            AlunoModuloService alunoModuloService,
            ModuloService moduloService,
            AlunoService alunoService
        )
        {
            this.cursoService = cursoService;
            this.csvImporterService = csvImporterService;
            this.alunoModuloService = alunoModuloService;
            this.moduloService = moduloService;
            this.alunoService = alunoService;
        }

        public async Task<IActionResult> IndexAsync(
            int pagina = 1,
            string? nomeAluno = null,
            int? cursoId = null,
            int? moduloId = null,
            int? statusId = null)
        {
            EnumStatus? status = statusId.HasValue ? (EnumStatus)statusId.Value : null;

            var resultado = await alunoModuloService.ObterInscricoesFiltradasPaginadoAsync(
                pagina,
                nomeAluno: nomeAluno,
                nomeModulo: null,
                idCurso: cursoId,
                status: status
            );

            var model = new InscricaoIndexViewModel
            {
                // Projeta as entidades para o item da tabela
                Inscricoes = resultado.Items.Select(am => new InscricaoItemViewModel
                {
                    NomeAluno = am.Aluno.Nome,
                    NomeCurso = am.Modulo.Curso.Nome,
                    NomeModulo = am.Modulo.Nome,
                    NumeroModulo = am.Modulo.Numero,
                    Nota = am.Nota,
                    EnumStatus = am.StatusInscricao,
                    DataInicio = am.DataAcesso,
                    DataFim = am.DataConclusao
                }).ToList(),

                // Mantém os filtros escolhidos (para preencher o form e os links de página)
                NomeAluno = nomeAluno,
                CursoId = cursoId,
                ModuloId = moduloId,
                StatusId = statusId,

                Cursos = (await cursoService.GetAllCursosAsync())
                    .Select(c => new SelectListItem
                    {
                        Value = c.IdCurso.ToString(),
                        Text = c.Nome
                    }).ToList(),

                Status = System.Enum.GetValues<EnumStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = TraduzirStatus(s)
                    }).ToList(),

                Modulos = (await moduloService.ObterModulosPorCursoAsync(cursoId ?? 0))
                    .Select(m => new SelectListItem
                    {
                        Value = m.IdModulo.ToString(),
                        Text = m.Nome
                    }).ToList(),

                PaginaAtual = resultado.PaginaAtual,
                TotalPaginas = resultado.TotalPaginas,
                TotalItems = resultado.TotalItems,
                TemPaginaAnterior = resultado.TemPaginaAnterior,
                TemProximaPagina = resultado.TemProximaPagina
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateModal()
        {
            var alunos = await alunoService.ObterTodosAlunosAsync();
            var cursos = await cursoService.GetAllCursosAsync();
            var model = new InscricaoCreateViewModel
            {
                Alunos = alunos.Select(a => new SelectListItem
                {
                    Value = a.IdAluno.ToString(),
                    Text = a.Nome
                }).ToList(),

                Cursos = cursos.Select(c => new SelectListItem
                {
                    Value = c.IdCurso.ToString(),
                    Text = c.Nome
                }).ToList()
            };
            return PartialView("_CreateInscricaoModalPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetModulosPorCursoModal(int cursoId)
        {
            var modulos = await moduloService.ObterModulosPorCursoAsync(cursoId);
            var lista = modulos.Select(m => new
            {
                value = m.IdModulo,
                text = $"{m.Numero:00} - {m.Nome}"
            });
            return Json(lista);
        }

        // Texto amigável do status para o dropdown de filtro
        private static string TraduzirStatus(EnumStatus status) => status switch
        {
            EnumStatus.Aprovado => "Aprovado",
            EnumStatus.Reprovado => "Reprovado",
            EnumStatus.EmAndamento => "Em Andamento",
            EnumStatus.Cancelado => "Cancelado",
            _ => status.ToString()
        };
    }
}
