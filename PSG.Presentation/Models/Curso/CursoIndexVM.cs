namespace PSG.Presentation.Models.Curso
{
    public class CursoIndexVM
    {
        public List<CursoItem> Cursos { get; set; } = new List<CursoItem>();
        public CursoDetalhes CursoDetalhes { get; set; } = new CursoDetalhes();

        // Termo digitado na busca de cursos. Guardado aqui para o campo continuar
        // preenchido depois do submit e para os links de seleção não perderem o filtro.
        public string? Busca { get; set; }
    }

    public class CursoItem
    {
        // Necessário para selecionar o curso na listagem (link ?cursoId=...).
        public int IdCurso { get; set; }
        public string Nome { get; set; } = string.Empty;
        // Nulos enquanto Sigla e Coordenador não existirem no Domain.
        public string? Sigla { get; set; }
        public string? NomeCoordenador { get; set; }
        public int QuantidadeAlunos { get; set; }
        public int TaxaCancelamento { get; set; }
    }

    public class CursoDetalhes
    {
        public int IdCurso { get; set; }
        // OBS: faltava o "public" em Nome e Sigla — como estavam, ficavam implicitamente
        // privadas (padrão do C# para membros de classe) e a view não conseguia lê-las.
        // Mesmo ajuste que já tinha sido feito no LineGraphSection do DashBoardVM.
        public string Nome { get; set; } = string.Empty;
        public string? Sigla { get; set; }
        public int QuantidadeAlunos { get; set; }
        public int QuantidadeAlunosCancelados { get; set; }
        public int QuantidadeAlunosReprovados { get; set; }
        public int TaxaCancelamento { get; set; }
        public int TaxaReprovacao { get; set; }

        public List<ModuloCursoItem> Modulos { get; set; } = new List<ModuloCursoItem>();
    }

    public class ModuloCursoItem
    {
        public string Nome { get; set; } = string.Empty;
        public int Numero { get; set; }
        // Nulo enquanto não existir entidade Professor no Domain.
        public string? NomeProfessor { get; set; }
        public int QuantidadeAlunos { get; set; }
    }
}
