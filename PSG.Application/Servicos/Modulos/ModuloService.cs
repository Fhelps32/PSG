using Azure;
using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Application.Servicos.Shared;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Modulos
{
    public class ModuloService
    {
        private readonly IPSGDbContext _context;

        public ModuloService(IPSGDbContext context)
        {
            _context = context;
        }

        public async Task<Modulo> ObterModuloPorNumeroAsync(int numeroModulo)
        {
            try
            {
                var modulo = await _context.Modulos.FirstOrDefaultAsync(m => m.Numero == numeroModulo);
                if (modulo == null)
                {
                    throw new Exception($"Módulo com número {numeroModulo} não encontrado.");
                }
                return modulo;

            }
            catch (Exception ex)
            {
                throw new Exception("Ocorreu um erro ao obter o módulo.", ex);
            }
        }

        public async Task<Modulo> ObterModuloPorIdAsync(int idModulo)
        {
            var modulo = await _context.Modulos.FirstOrDefaultAsync(m => m.IdModulo == idModulo);
            if (modulo == null)
            {
                throw new Exception($"Módulo com ID {idModulo} não encontrado.");
            } 
            return modulo;
        }

        public async Task<PagedResult<Modulo>> ObterModulosPorCursoPaginadoAsync(int idCurso, int pagina)
        {
            var query = _context.Modulos.AsQueryable();
            query = query.Where(m => m.IdCurso == idCurso);

            var result = await query.Paginar<Modulo>(new PaginationRequest { NumeroPagina = pagina, TamanhoPagina = 20 });
            return result;
        }
    }
}
