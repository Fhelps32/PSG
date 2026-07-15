using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Application.Servicos.Shared;
using PSG.Domain;
using PSG.Domain.Enum;
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
            string? nomeAluno = null, 
            string? nomeModulo = null, 
            int? idCurso = null,
            EnumStatus? status = null
            )
        {
            // Include das navegações: sem lazy loading, Aluno/Modulo/Curso viriam nulos
            // e a projeção na Presentation (NomeAluno, NomeCurso, etc.) quebraria.
            var query = _context.AlunoModulos
                .Include(am => am.Aluno)
                .Include(am => am.Modulo)
                    .ThenInclude(m => m.Curso)
                .AsQueryable();
            query = query.Where(am => am.Status == true);
            if (idCurso.HasValue)
            {
                query = query.Where(am => am.Modulo.IdCurso == idCurso.Value);
            }
            if (status.HasValue)
            {
                query = query.Where(am => am.StatusInscricao == status.Value);
            }
            if (!string.IsNullOrEmpty(nomeAluno))
            {
                query = query.Where(am => am.Aluno.Nome.Contains(nomeAluno));
            }
            var result = await query.Paginar<AlunoModulo>(new PaginationRequest { NumeroPagina = pagina, TamanhoPagina = 20 });
            return result;
        }

        public async Task<List<AlunoModulo>> GetAllAlunoModuloAsync()
        {
            var result = await _context.AlunoModulos
                .Include(am => am.Aluno)
                .Include(am => am.Modulo)
                .ThenInclude(m => m.Curso)
                .Where(am => am.Status == true)
                .ToListAsync();
            return result;
        }
    }
}
