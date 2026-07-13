using PSG.Application.Context;
using PSG.Application.Servicos.Shared;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.AlunoModulos
{
    public class AlunoModuloService
    {
        private readonly IPSGDbContext _context;

        public AlunoModuloService(IPSGDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AlunoModulo>> ObterInscricoesFiltradasPaginadoAsync(int pagina, 
            string? nomeAluno = null, string? nomeCurso = null, int? idModulo = null
            )
        {
            var query = _context.AlunoModulos.AsQueryable();
            query = query.Where(am => am.IdModulo == idModulo);
            var result = await query.Paginar<AlunoModulo>(new PaginationRequest { NumeroPagina = pagina, TamanhoPagina = 20 });
            return result;
        }
    }
}
