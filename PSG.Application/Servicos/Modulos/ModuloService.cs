using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
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
            return await _context.Modulos.FirstOrDefaultAsync(m => m.IdModulo == idModulo);
        }

        public async Task<IEnumerable<Modulo>> ObterModulosPorCursoAsync(int idCurso)
        {
            return await _context.Modulos.Where(m => m.IdCurso == idCurso).ToListAsync();
        }
    }
}
