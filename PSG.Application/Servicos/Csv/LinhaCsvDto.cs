using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Csv
{
    public sealed class LinhaCsvDto
    {
        public string Curso { get; set; }
        public string NumeroModulo { get; set; }
        public string NomeModulo { get; set; }
        public string Aluno { get; set; }
        public string DataMatricula { get; set; }
        public string DataAcesso { get; set; }
        public string DataFim { get; set; }
        public string Nota { get; set; }
        public string Aprovacao { get; set; }
        public string Celular { get; set; }
        public string Observacao { get; set; }
        public string Recuperacao { get; set; }
    }
}
