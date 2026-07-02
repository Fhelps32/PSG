using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Domain
{
    public class Aluno
    {
        public int IdAluno { get; set; }
        public int IdCurso { get; set; }
        public string Matricula { get; set; }
        public string Nome { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        public Curso Curso { get; set; }
        public IEnumerable<AlunoModulo> Modulos { get; set; }

        private Aluno()
        {
        }

        public Aluno(Curso curso, string matricula, string nome)
        {
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            Matricula = matricula;
            Nome = nome;
        }
    }
}