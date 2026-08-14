using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSG.Presentation.Models
{
    public class AlunoVM
    {
        public List<AlunoItem> Alunos { get; set; }

        public List<SelectListItem> FiltroCurso { get; set; }

        public List<SelectListItem> FiltroModulo { get; set; }

        public int QuantidadeAlunosFiltrados { get; set; }
        public AlunoDetalhado AlunoSelecionado { get; set; } = new AlunoDetalhado();
}

    public class AlunoItem
    {
        public string Nome { get; set; } 
        public string Matricula { get; set; }
        public string NomeModuloAtual { get; set; }
        public string SiglaCurso { get; set; }
    }

    public class AlunoDetalhado
    {
        public string Nome { get; set; }
        public string Matricula { get; set; }
        public string NomeCurso { get; set; } 
        public string Status { get; set; }

        public List<ModulosAluno> Modulos = new List<ModulosAluno>();
        public List<ModulosAluno> ModuloReprovados = new List<ModulosAluno>();
    }

    public class ModulosAluno
    {
        public string Nome { get; set; }
        public DateTime? DataInscricao { get; set; } //caso aluno não esteja fazendo o módulo, a data de inscrição e a nota serão nulos
        public DateTime? DataFim { get; set; }
        public int? Nota { get; set; }
    
    }
}
