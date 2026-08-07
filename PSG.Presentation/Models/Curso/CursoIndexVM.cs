namespace PSG.Presentation.Models.Curso
{
    public class CursoIndexVM
    {
        public List<CursoItem> Cursos { get; set; } = new List<CursoItem>();
        public CursoDetalhes CursoDetalhes { get; set; } = new CursoDetalhes();
    }

    public class CursoItem
    {
        public string Nome { get; set; }
        public string Sigla { get; set; }
        public string NomeCoordenador { get; set; }
        public int QuantidadeAlunos { get; set; }
        public int TaxaCancelamento { get; set; }
    }

    public class CursoDetalhes
    {
        string Nome { get; set; }
        string Sigla { get; set; }
        public int QuantidadeAlunos { get; set; }
        public int QuantidadeAlunosCancelados { get; set; }
        public int QuantidadeAlunosReprovados { get; set; }
        public int TaxaCancelamento { get; set; }
        public int TaxaReprovacao { get; set; }

        public List<ModuloCursoItem> Modulos { get; set; } = new List<ModuloCursoItem>();
    }

    public class ModuloCursoItem
    {
        public string Nome { get; set; }
        public string NomeProfessor { get; set; }
        public int QuantidadeAlunos { get; set; }
    }
}
