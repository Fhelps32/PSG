using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Domain
{
    public class Professor
    {
        public int IdProfessor { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;

        public ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();
        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();

        private Professor() { }

        public Professor(string matricula, string nome)
        {
            Matricula = matricula;
            Nome = nome;
        }
    }
}
