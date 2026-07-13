using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Shared
{
    public class PaginationRequest
    {
        private const int TamanhoMaximo = 50;
        private int _tamanhoPagina = 10;

        public int NumeroPagina { get; set; } = 1;

        public int TamanhoPagina
        {
            get => _tamanhoPagina;
            set => _tamanhoPagina = value > TamanhoMaximo ? TamanhoMaximo : value;
        }
    }
}
