using System;
using System.Collections.Generic;
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

        public Curso Curso { get; set; }
        public IEnumerable<AlunoModulo> Alunos { get; set; }

        private Modulo()
        {        
        }
    }
}
