using Microsoft.AspNetCore.Mvc;
using PSG.Application.Servicos.Cursos;
using PSG.Application.Servicos.Modulos;
using PSG.Presentation.Models.Curso;

namespace PSG.Presentation.Controllers
{
    public class CursoController(
        CursoService cursoService,
        ModuloService moduloService
        ) : Controller
    {
        private readonly CursoService _cursoService = cursoService;
        private readonly ModuloService _moduloService = moduloService;

        /// <summary>
        /// Tela de cursos: listagem à esquerda e, à direita, os detalhes do curso
        /// selecionado com os módulos dele. A busca de cursos e a de módulos são
        /// feitas no cliente, então a action só recebe o curso selecionado.
        /// Sem cursoId na URL, seleciona o primeiro curso da listagem.
        /// </summary>
        public async Task<IActionResult> Index(int? cursoId = null)
        {
            var cursos = await _cursoService.ObterCursosParaListagemAsync();

            var viewModel = new CursoIndexVM
            {
                Cursos = cursos
                    .Select(c => new CursoItem
                    {
                        IdCurso = c.IdCurso,
                        Nome = c.Nome,
                        Sigla = c.Sigla,
                        NomeCoordenador = c.NomeCoordenador,
                        QuantidadeAlunos = c.QuantidadeAlunos,
                        TaxaCancelamento = c.TaxaCancelamento
                    })
                    .ToList()
            };

            // Curso inexistente na URL cai no primeiro da lista, em vez de deixar
            // o painel da direita vazio.
            var idSelecionado = cursos.Any(c => c.IdCurso == cursoId)
                ? cursoId
                : cursos.FirstOrDefault()?.IdCurso;

            if (idSelecionado.HasValue)
            {
                var detalhes = await _cursoService.ObterDetalhesDoCursoAsync(idSelecionado.Value);
                if (detalhes is not null)
                {
                    viewModel.CursoDetalhes = new CursoDetalhes
                    {
                        IdCurso = detalhes.IdCurso,
                        Nome = detalhes.Nome,
                        Sigla = detalhes.Sigla,
                        QuantidadeAlunos = detalhes.QuantidadeAlunos,
                        QuantidadeAlunosCancelados = detalhes.QuantidadeAlunosCancelados,
                        QuantidadeAlunosReprovados = detalhes.QuantidadeAlunosReprovados,
                        TaxaCancelamento = detalhes.TaxaCancelamento,
                        TaxaReprovacao = detalhes.TaxaReprovacao,
                        Modulos = detalhes.Modulos
                            .Select(m => new ModuloCursoItem
                            {
                                IdModulo = m.IdModulo,
                                Nome = m.Nome,
                                Numero = m.Numero,
                                NomeProfessor = m.NomeProfessor,
                                QuantidadeAlunos = m.QuantidadeAlunos
                            })
                            .ToList()
                    };
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Devolve a modal de edição do módulo já preenchida. Chamada por fetch a
        /// partir da tela de Cursos — por isso PartialView, e não uma página nova.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarModuloModal(int idModulo)
        {
            var modulo = await _moduloService.ObterModuloParaEdicaoAsync(idModulo);
            if (modulo is null)
            {
                return NotFound();
            }

            var viewModel = new ModuloEditVM
            {
                IdModulo = modulo.IdModulo,
                IdCurso = modulo.IdCurso,
                NomeCurso = modulo.NomeCurso,
                Nome = modulo.Nome,
                Numero = modulo.Numero
            };

            return PartialView("_EditModuloModalPartial", viewModel);
        }

        /// <summary>
        /// Salva a edição do módulo. Segue o mesmo contrato da modal de inscrição:
        /// 400 + a modal re-renderizada quando há erro de validação, 200 quando salva
        /// (o front recarrega a tela).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarModulo(ModuloEditVM viewModel)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return PartialView("_EditModuloModalPartial", viewModel);
            }

            await _moduloService.AtualizarModuloAsync(
                viewModel.IdModulo,
                new ModuloDtoAtualizar(viewModel.Nome, viewModel.Numero));

            return Ok();
        }

        /// <summary>
        /// Exclusão lógica do módulo (Status = false no service).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirModulo(int idModulo)
        {
            await _moduloService.ExcluirModuloAsync(idModulo);
            return Ok();
        }
    }
}
