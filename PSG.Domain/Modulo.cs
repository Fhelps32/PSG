using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Domain
{
    public class Modulo
    {
        public int IdModulo { get; set; }
        public int IdCurso { get; set; }
        public string Nome { get; set; }
        public int Numero { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;  
        public bool Status { get; set; } = true;

        public Curso Curso { get; set; }
        public IEnumerable<AlunoModulo> Alunos { get; set; }

        private Modulo()
        {        
        }

        public Modulo(Curso curso, string nome, int numero)
        {
            Curso = curso ?? throw new ArgumentNullException(nameof(curso));
            Nome = nome;
            Numero = numero;
        }
    }
}
