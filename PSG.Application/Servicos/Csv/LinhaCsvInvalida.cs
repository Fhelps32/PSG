using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Csv
{
    public class LinhaCsvInvalida
    {
        public int NumeroLinha { get; set; }
        public string Motivo { get; set; }
        public LinhaCsvDto LinhaOriginal { get; set; }

        public override string ToString()
        {
            return $"Linha {NumeroLinha}: {Motivo}";
        }
    }
}
