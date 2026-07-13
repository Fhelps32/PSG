using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Shared
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> Paginar<T>(this IQueryable<T> query, PaginationRequest paginationRequest)
        {
            if (paginationRequest == null)
            {
                throw new ArgumentNullException(nameof(paginationRequest));
            }
            
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((paginationRequest.NumeroPagina - 1) * paginationRequest.TamanhoPagina)
                .Take(paginationRequest.TamanhoPagina)
                .ToListAsync();

            return new PagedResult<T>
            {
                TotalItems = totalItems,
                TamanhoPagina = paginationRequest.TamanhoPagina,
                PaginaAtual = paginationRequest.NumeroPagina,
                Items = items
            };
        }
    }
}
