namespace PSG.Domain
{
    public class Aluno
    {
        public int IdAluno { get; set; }
        public int IdCurso { get; set; }
        public string? Matricula { get; set; }
        public string Nome { get; set; }
        public string? Celular { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        public Curso Curso { get; set; }
        public ICollection<AlunoModulo> Modulos { get; set; } = new List<AlunoModulo>();

        private Aluno()
        {
        }

        public Aluno(Curso curso, string matricula, string nome)
        {
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            Matricula = matricula;
            Nome = nome;
        }

        public Aluno(Curso curso, string nome)
        {
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            Nome = nome;
        }

        public void SwitchStatus()
        {
            Status = !Status;
        }
    }
}