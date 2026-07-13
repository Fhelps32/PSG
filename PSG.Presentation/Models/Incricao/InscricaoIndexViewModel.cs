using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Domain.Enum;

namespace PSG.Presentation.Models.Incricao
{
    public class InscricaoIndexViewModel
    {
        public List<InscricaoItemViewModel> Inscricoes { get; set; }
        public int? CursoId { get; set; }
        public int? StatusId { get; set; }
        public int? ModuloId { get; set; }
        public List<SelectListItem> Cursos { get; set; }
        public List<SelectListItem> Status { get; set; }
        public List<SelectListItem> Modulos { get; set; }
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
