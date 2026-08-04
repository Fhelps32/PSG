using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSG.Presentation.Models.DashBoard
{
    public class DashBoardVM
    {
        public LineGraphSection LineGraphSection { get; set; } = new LineGraphSection();
        public SectorGraphSection SectorGraphSection { get; set; } = new SectorGraphSection();
        public List<AlunoCanceladoItem> AlunosCancelados { get; set; } = new List<AlunoCanceladoItem>();
        public CursoImportanciaItem CursoMaiorCancelamento { get; set; } = new CursoImportanciaItem();
        public CursoImportanciaItem CursoMaiorReprovacao { get; set; } = new CursoImportanciaItem();
        public int TotalAlunos { get; set; }
        public int TotalCursos { get; set; }
    }

    public class LineGraphSection
    {
        // OBS: faltava o "public" nessas 3 propriedades - como estava, elas ficavam
        // implicitamente "private" (padrão do C# para membros de classe) e não
        // seriam enxergadas pela view (Model.LineGraphSection.ValueLineGraph daria
        // erro de compilação). Adicionei "public" e inicializei pra evitar null.
        public List<ValueLineGraph> ValueLineGraph { get; set; } = new List<ValueLineGraph>();
        public List<SelectListItem> Tempo { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
    }

    public class ValueLineGraph
    {
        public int QuantidadeAlunos { get; set; }
        public DateTime Data { get; set; }
    }

    public class SectorGraphSection
    {
        // Mesmo ajuste do LineGraphSection acima.
        public List<ValueSectorGraph> ValueSectorGraph { get; set; } = new List<ValueSectorGraph>();
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Tempo { get; set; } = new List<SelectListItem>();
    }

    public class ValueSectorGraph
    {
        public int QuantidadeAlunos { get; set; }
        public string Curso { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
    }

    public class AlunoCanceladoItem
    {
        public string Nome { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public DateTime DataCancelamento { get; set; }
    }

    public class CursoImportanciaItem
    {
        public string Curso { get; set; } = string.Empty;
        public int Taxa { get; set; }
        public int QuantidadeAlunosImportancia { get; set; }
        public int QuantidadeAlunosTotal { get; set; }
    }
}