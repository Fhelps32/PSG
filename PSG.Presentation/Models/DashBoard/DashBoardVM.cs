using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSG.Presentation.Models.DashBoard
{
    public class DashBoardVM
    {
        public LineGraphSection LineGraphSection { get; set; } = new LineGraphSection();
        public SectorGraphSection SectorGraphSection { get; set; } = new SectorGraphSection();
        public List<AlunoCanceladoItem> AlunosCancelados { get; set; } = new List<AlunoCanceladoItem>();
        public int TotalAlunos { get; set; }
        public int TotalCursos { get; set; }
    }

    public class LineGraphSection
    {
        List<ValueLineGraph> ValueLineGraph { get; set; } = new List<ValueLineGraph>();
        List<SelectListItem> Tempo { get; set; } = new List<SelectListItem>();
        List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
    }

    public class ValueLineGraph
    {
        public int QuantidadeAlunos { get; set; }
        public DateTime Data { get; set; }
    }

    public class SectorGraphSection
    {
        List<ValueSectorGraph> ValueSectorGraph { get; set; } = new List<ValueSectorGraph>();
        List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        List<SelectListItem> Tempo { get; set; } = new List<SelectListItem>();
    }

    public class ValueSectorGraph
    {
        public int QuantidadeAlunos { get; set; }
        public string Curso { get; set; }
        public string Modulo { get; set; }
    }

    public class AlunoCanceladoItem
    {
        public string Nome { get; set; }
        public string Curso { get; set; }
        public string Modulo { get; set; }  
        public DateTime DataCancelamento { get; set; }
    }
}