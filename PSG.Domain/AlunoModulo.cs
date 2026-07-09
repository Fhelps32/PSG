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
        public int IdAlunoModulo { get; set; }
        public int IdAluno { get; set; }
        public int IdModulo { get; set; }
        public DateTime DataAcesso { get; set; }
        public DateTime? DataConclusao { get; set; }
        public DateTime? DataMatricula { get; set; }
        public decimal Nota { get; set; }
        public string? ObsTempo { get; set; }
        public string? ObsNota { get; set; }
        public string? ObsGeral { get; set; }
        public EnumStatus StatusInscricao { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Status { get; set; } = true;

        public Aluno Aluno { get; set; }
        public Modulo Modulo { get; set; }

        private AlunoModulo()
        {
        }

        public AlunoModulo(Aluno aluno, Modulo modulo, DateTime dataAcesso, EnumStatus statusInscricao, DateTime? dataConclusao)
        {
            Aluno = aluno ?? throw new ArgumentNullException(nameof(aluno));
            Modulo = modulo ?? throw new ArgumentNullException(nameof(modulo));
            DataAcesso = dataAcesso;
            StatusInscricao = statusInscricao;
            DataConclusao = dataConclusao;
        }
    }
}