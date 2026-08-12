using Microsoft.AspNetCore.Mvc;
using PSG.Application.Servicos.Cursos;
using PSG.Presentation.Models.Curso;

namespace PSG.Presentation.Controllers
{
    public class CursoController(CursoService cursoService) : Controller
    {
        private readonly CursoService _cursoService = cursoService;

        /// <summary>
        /// Tela de cursos: listagem à esquerda (com busca por nome) e, à direita,
        /// os detalhes do curso selecionado com os módulos dele.
        /// Sem cursoId na URL, seleciona o primeiro curso da listagem.
        /// </summary>
        public async Task<IActionResult> Index(int? cursoId = null, string? busca = null)
        {
            var cursos = await _cursoService.ObterCursosParaListagemAsync(busca);

            var viewModel = new CursoIndexVM
            {
                Busca = busca,
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

            // O curso pedido só é aceito se estiver na listagem atual; senão o painel
            // da direita mostraria um curso que a busca filtrou fora.
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
    }
}
