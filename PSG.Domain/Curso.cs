using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Domain
{
    public class Curso
    {
        public int IdCurso { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true; 

        public ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();
        public ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();

        private Curso()
        {
        }

        public Curso(string nome)
        {
            Nome = nome;
        }
    }
}
