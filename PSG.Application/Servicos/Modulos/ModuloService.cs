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

        public async Task<List<Modulo>> ObterModulosPorCursoAsync(int idCurso)
        {
            // Só módulos ativos: os excluídos (exclusão lógica, Status = false) não
            // podem continuar aparecendo nos filtros e no cadastro de inscrição.
            var modulos = await _context.Modulos
                .Where(m => m.IdCurso == idCurso && m.Status)
                .ToListAsync();
            return modulos;
        }

        /// <summary>
        /// Dados do módulo para a modal de edição, junto com o nome do curso
        /// (mostrado como contexto). Retorna null quando o módulo não existe
        /// ou já foi excluído.
        /// </summary>
        public async Task<ModuloEdicaoDto?> ObterModuloParaEdicaoAsync(int idModulo)
        {
            return await _context.Modulos
                .Where(m => m.IdModulo == idModulo && m.Status)
                .Select(m => new ModuloEdicaoDto(
                    m.IdModulo,
                    m.IdCurso,
                    m.Curso.Nome,
                    m.Nome,
                    m.Numero))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Atualiza nome e número do módulo. Lança exceção quando o módulo não existe.
        /// </summary>
        public async Task AtualizarModuloAsync(int idModulo, ModuloDtoAtualizar dto)
        {
            var modulo = await _context.Modulos.FindAsync(idModulo);
            if (modulo == null)
            {
                throw new Exception($"Módulo com ID {idModulo} não encontrado.");
            }

            modulo.Nome = dto.Nome;
            modulo.Numero = dto.Numero;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Exclusão lógica do módulo (Status = false), mesma abordagem do
        /// AlunoService.ExcluirAlunoAsync — o histórico de inscrições é preservado.
        /// </summary>
        public async Task ExcluirModuloAsync(int idModulo)
        {
            var modulo = await _context.Modulos.FindAsync(idModulo);
            if (modulo == null)
            {
                throw new Exception($"Módulo com ID {idModulo} não encontrado.");
            }
            if (!modulo.Status)
            {
                throw new Exception($"O Módulo com ID {idModulo} já está inativo.");
            }

            modulo.Status = false;
            await _context.SaveChangesAsync();
        }
    }
}
