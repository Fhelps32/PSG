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
                status: status,
                idModulo: moduloId
            );

            var model = new InscricaoIndexViewModel
            {
                // Projeta as entidades para o item da tabela
                Inscricoes = resultado.Items.Select(am => new InscricaoItemViewModel
                {
                    IdAlunoModulo = am.IdAlunoModulo,
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

                Cursos = (await cursoService.ObterTodosOsCursosAsync())
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

                // Módulos do curso filtrado (vazio se nenhum curso selecionado).
                // Mesmo formato "00 - Nome" usado no cascata AJAX, para não haver diferença visual.
                Modulos = (await moduloService.ObterModulosPorCursoAsync(cursoId ?? 0))
                    .Select(m => new SelectListItem
                    {
                        Value = m.IdModulo.ToString(),
                        Text = $"{m.Numero:00} - {m.Nome}"
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
            var cursos = await cursoService.ObterTodosOsCursosAsync();
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

        [HttpPost]
        public async Task<IActionResult> Create(InscricaoCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Devolve a modal de novo, mas com status 400 — o JS trata 400 como
                // "erros no formulário" e re-renderiza a modal com as mensagens de validação.
                // (Antes retornava 200, então o front dava reload e perdia os erros.)
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return PartialView("_CreateInscricaoModalPartial", viewModel);
            }

            var dto = new AlunoModuloDtoCriar(
                await alunoService.ObterAlunoPorIdAsync(viewModel.AlunoId),
                await moduloService.ObterModuloPorIdAsync(viewModel.ModuloId),
                viewModel.StatusInscricao,
                viewModel.DataAcesso,
                null
            );

            await alunoModuloService.CreateAlunoModuloAsync(dto);

            // Sucesso: 200 OK. O front faz location.reload() para exibir a nova inscrição.
            // (Antes fazia RedirectToAction("IndexAsync"); porém, por convenção, o nome da
            //  action é "Index" — o fetch seguia o redirect para /Inscricao/IndexAsync, que
            //  retorna 404, gerando o "falso erro" mesmo com a inscrição já criada.)
            return Ok();
        }

        /// <summary>
        /// Devolve a modal de edição da inscrição já preenchida. Chamada por fetch a
        /// partir da tabela — por isso PartialView, e não uma página nova.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarModal(int idAlunoModulo)
        {
            var inscricao = await alunoModuloService.ObterInscricaoParaEdicaoAsync(idAlunoModulo);
            if (inscricao is null)
            {
                return NotFound();
            }

            var viewModel = new InscricaoEditViewModel
            {
                IdAlunoModulo = inscricao.IdAlunoModulo,
                NomeAluno = inscricao.NomeAluno,
                NomeCurso = inscricao.NomeCurso,
                NomeModulo = inscricao.NomeModulo,
                NumeroModulo = inscricao.NumeroModulo,
                DataInicio = inscricao.DataInicio,
                DataFim = inscricao.DataFim,
                DataMatricula = inscricao.DataMatricula,
                Nota = inscricao.Nota,
                StatusInscricao = inscricao.EnumStatus,
                ObsGeral = inscricao.ObsGeral
            };

            return PartialView("_EditInscricaoModalPartial", viewModel);
        }

        /// <summary>
        /// Salva a edição da inscrição. Mesmo contrato da modal de criação: 400 + a
        /// modal re-renderizada quando há erro de validação, 200 quando salva (o front
        /// recarrega a tabela).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(InscricaoEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return PartialView("_EditInscricaoModalPartial", viewModel);
            }

            await alunoModuloService.AtualizarInscricaoAsync(
                viewModel.IdAlunoModulo,
                new AlunoModuloDtoAtualizar(
                    viewModel.DataInicio,
                    viewModel.DataFim,
                    viewModel.DataMatricula,
                    viewModel.Nota,
                    viewModel.StatusInscricao,
                    viewModel.ObsGeral));

            return Ok();
        }

        /// <summary>
        /// Exclusão lógica da inscrição (Status = false no service).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int idAlunoModulo)
        {
            await alunoModuloService.ExcluirInscricaoAsync(idAlunoModulo);
            return Ok();
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
