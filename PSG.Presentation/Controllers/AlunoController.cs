using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Servicos.Alunos;
using PSG.Application.Servicos.Cursos;
using PSG.Application.Servicos.Modulos;
using PSG.Presentation.Models.Aluno;

namespace PSG.Presentation.Controllers
{
    public class AlunoController(
        AlunoService alunoService,
        CursoService cursoService,
        ModuloService moduloService
        ) : Controller
    {
        private readonly AlunoService _alunoService = alunoService;
        private readonly CursoService _cursoService = cursoService;
        private readonly ModuloService _moduloService = moduloService;

        /// <summary>
        /// Tela de alunos: listagem paginada com busca e filtros à esquerda e, à
        /// direita, os detalhes do aluno selecionado com os módulos dele.
        /// Sem alunoId na URL, seleciona o primeiro aluno da página.
        /// </summary>
        public async Task<IActionResult> Index(
            int? alunoId = null,
            string? busca = null,
            int? cursoId = null,
            int? moduloId = null,
            int pagina = 1)
        {
            var resultado = await _alunoService.ObterAlunosParaListagemAsync(pagina, busca, cursoId, moduloId);

            var viewModel = new AlunoIndexVM
            {
                Busca = busca,
                CursoId = cursoId,
                ModuloId = moduloId,

                Alunos = resultado.Items
                    .Select(a => new AlunoItem
                    {
                        IdAluno = a.IdAluno,
                        Nome = a.Nome,
                        Matricula = a.Matricula,
                        NomeCurso = a.NomeCurso,
                        ModuloAtual = a.ModuloAtual,
                        Status = a.Status
                    })
                    .ToList(),

                Cursos = (await _cursoService.ObterTodosOsCursosAsync())
                    .Select(c => new SelectListItem
                    {
                        Value = c.IdCurso.ToString(),
                        Text = c.Nome
                    })
                    .ToList(),

                // Módulos do curso filtrado (vazio quando nenhum curso está selecionado).
                // Mesmo formato "00 - Nome" usado na cascata AJAX, para não haver
                // diferença visual entre o que vem do servidor e o que vem do fetch.
                Modulos = (await _moduloService.ObterModulosPorCursoAsync(cursoId ?? 0))
                    .OrderBy(m => m.Numero)
                    .Select(m => new SelectListItem
                    {
                        Value = m.IdModulo.ToString(),
                        Text = $"{m.Numero:00} - {m.Nome}"
                    })
                    .ToList(),

                PaginaAtual = resultado.PaginaAtual,
                TotalPaginas = resultado.TotalPaginas,
                TotalItems = resultado.TotalItems,
                TemPaginaAnterior = resultado.TemPaginaAnterior,
                TemProximaPagina = resultado.TemProximaPagina
            };

            // Aluno que não está na página atual cai no primeiro da lista, senão o
            // painel da direita mostraria alguém que o filtro deixou de fora.
            var idSelecionado = viewModel.Alunos.Any(a => a.IdAluno == alunoId)
                ? alunoId
                : viewModel.Alunos.FirstOrDefault()?.IdAluno;

            if (idSelecionado.HasValue)
            {
                var detalhes = await _alunoService.ObterDetalhesDoAlunoAsync(idSelecionado.Value);
                if (detalhes is not null)
                {
                    viewModel.AlunoDetalhes = new AlunoDetalhes
                    {
                        IdAluno = detalhes.IdAluno,
                        Nome = detalhes.Nome,
                        Matricula = detalhes.Matricula,
                        NomeCurso = detalhes.NomeCurso,
                        Status = detalhes.Status,
                        ModulosConcluidos = detalhes.ModulosConcluidos,
                        TotalModulosCurso = detalhes.TotalModulosCurso,
                        Modulos = detalhes.Modulos.Select(MapearModulo).ToList(),
                        ModulosReprovados = detalhes.ModulosReprovados.Select(MapearModulo).ToList()
                    };
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Módulos de um curso, para a cascata Curso -> Módulo do filtro (mesma ideia
        /// do GetModulosPorCursoModal da tela de Inscrições).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetModulosPorCurso(int cursoId)
        {
            var modulos = await _moduloService.ObterModulosPorCursoAsync(cursoId);
            return Json(modulos
                .OrderBy(m => m.Numero)
                .Select(m => new
                {
                    value = m.IdModulo,
                    text = $"{m.Numero:00} - {m.Nome}"
                }));
        }

        private static ModuloAlunoItem MapearModulo(AlunoModuloDetalheDto dto) => new()
        {
            Nome = dto.NomeModulo,
            Numero = dto.NumeroModulo,
            DataInscricao = dto.DataInscricao,
            DataFim = dto.DataFim,
            Nota = dto.Nota,
            Status = dto.Status
        };
    }
}
