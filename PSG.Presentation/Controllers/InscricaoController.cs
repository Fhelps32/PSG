using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Servicos.AlunoModulos;
using PSG.Application.Servicos.Csv;
using PSG.Application.Servicos.Cursos;
using PSG.Domain.Enum;
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

        public async Task<IActionResult> IndexAsync(
            int pagina = 1,
            string? nomeAluno = null,
            int? cursoId = null,
            int? moduloId = null,
            int? statusId = null)
        {
            // O form manda o status como int; converte para o enum do domínio.
            EnumStatus? status = statusId.HasValue ? (EnumStatus)statusId.Value : null;

            // Busca paginada + filtrada no service.
            // Obs.: o service ainda não filtra por módulo (recebe nomeModulo, não idModulo), então passamos null.
            var resultado = await alunoModuloService.ObterInscricoesFiltradasPaginadoAsync(
                pagina,
                nomeAluno: nomeAluno,
                nomeModulo: null,
                idCurso: cursoId,
                status: status);

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

                // Dropdowns de filtro
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

                // Módulo: dropdown vazio por enquanto (service ainda não filtra por módulo)
                Modulos = new List<SelectListItem>(),

                // Dados de paginação vindos do PagedResult
                PaginaAtual = resultado.PaginaAtual,
                TotalPaginas = resultado.TotalPaginas,
                TotalItems = resultado.TotalItems,
                TemPaginaAnterior = resultado.TemPaginaAnterior,
                TemProximaPagina = resultado.TemProximaPagina
            };

            return View(model);
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
