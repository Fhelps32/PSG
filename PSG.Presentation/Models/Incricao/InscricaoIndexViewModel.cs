using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Domain.Enum;

namespace PSG.Presentation.Models.Incricao
{
    public class InscricaoIndexViewModel
    {
        public List<InscricaoItemViewModel> Inscricoes { get; set; }

        // Filtros selecionados (preservados no form e nos links de paginação)
        public string? NomeAluno { get; set; }
        public int? CursoId { get; set; }
        public int? StatusId { get; set; }
        public int? ModuloId { get; set; }

        // Opções dos dropdowns de filtro
        public List<SelectListItem> Cursos { get; set; }
        public List<SelectListItem> Status { get; set; }
        public List<SelectListItem> Modulos { get; set; }

        // Dados de paginação (vindos do PagedResult do service)
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItems { get; set; }
        public bool TemPaginaAnterior { get; set; }
        public bool TemProximaPagina { get; set; }
    }

    public class InscricaoItemViewModel
    {
        public string NomeAluno { get; set; }
        public string NomeCurso { get; set; }
        public string NomeModulo { get; set; }
        public int NumeroModulo { get; set; }
        // Nota agora é nullable: um módulo em andamento / não iniciado pode não ter nota ainda.
        public decimal? Nota { get; set; }
        public EnumStatus EnumStatus { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
