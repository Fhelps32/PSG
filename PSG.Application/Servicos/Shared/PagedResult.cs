using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Shared
{
    public class PagedResult<T>
    {
        public int TotalItems { get; set; }
        public int TamanhoPagina { get; set; }
        public int PaginaAtual { get; set; } = 1;
        
        public List<T> Items { get; set; } = new List<T>();

        public int TotalPaginas => (int)Math.Ceiling((double)TotalItems / TamanhoPagina);
        public bool TemPaginaAnterior => PaginaAtual > 1;
        public bool TemProximaPagina => PaginaAtual < TotalPaginas;
    }
}
