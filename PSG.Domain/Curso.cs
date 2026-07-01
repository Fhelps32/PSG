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
        [Key]
        public int IdCurso { get; set; }
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        public IEnumerable<Modulo> Modulos { get; set; } = Enumerable.Empty<Modulo>();
        public IEnumerable<Aluno> Alunos { get; set; } = Enumerable.Empty<Aluno>();

        private Curso()
        {
        }
    }
}
