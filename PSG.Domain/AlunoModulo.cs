using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Domain
{
    public class AlunoModulo
    {
        [Key]
        public int IdAlunoModulo { get; set; }

        [Required]
        [ForeignKey("Aluno")]
        public int IdAluno { get; set; }

        [Required]
        [ForeignKey("Modulo")]
        public int IdModulo { get; set; }

        [Required]
        public DateTime DataAcesso { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? ObsTempo { get; set; }
        public string? ObsNota { get; set; }

        [Required]
        public EnumStatus StatusInscricao { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        public Aluno Aluno { get; set; }
        public Modulo Modulo { get; set; }

        private AlunoModulo()
        {
        }
    }
}
