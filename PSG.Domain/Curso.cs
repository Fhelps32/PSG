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

        public IEnumerable<Modulo> Modulos { get; set; } = Enumerable.Empty<Modulo>();
        public IEnumerable<Aluno> Alunos { get; set; } = Enumerable.Empty<Aluno>();

        private Curso()
        {
        }

        public Curso(string nome)
        {
            Nome = nome;
        }
    }
}
