using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Domain;
using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Cursos
{
    public class CursoService
    {
        private readonly IPSGDbContext _context;

        public CursoService(IPSGDbContext context)
        {
            _context = context;
        }

        public async Task<List<Curso>> ObterTodosOsCursosAsync()
        {
            return await Task.FromResult(_context.Cursos.ToList());
        }

        public async Task<int> ObterQuantidadeTotalCursosAsync()
        {
            return await _context.Cursos.CountAsync();
        }

        /// <summary>
        /// Curso com a maior proporção de inscrições no status informado
        /// (ex.: Cancelado para taxa de cancelamento, Reprovado para taxa de reprovação).
        /// Retorna null quando nenhum curso tem inscrições nesse status.
        /// </summary>
        public async Task<CursoTaxaDto?> ObterCursoComMaiorTaxaAsync(EnumStatus status)
        {
            var dados = await _context.Cursos
                .Select(c => new
                {
                    c.Nome,
                    Total = c.Modulos.SelectMany(m => m.Alunos).Count(),
                    Importancia = c.Modulos.SelectMany(m => m.Alunos).Count(am => am.StatusInscricao == status)
                })
                .Where(x => x.Total > 0 && x.Importancia > 0)
                .ToListAsync();

            var maior = dados
                .OrderByDescending(x => (double)x.Importancia / x.Total)
                .ThenByDescending(x => x.Importancia)
                .FirstOrDefault();

            if (maior is null)
            {
                return null;
            }

            var taxa = (int)Math.Round(100d * maior.Importancia / maior.Total);
            return new CursoTaxaDto(maior.Nome, taxa, maior.Importancia, maior.Total);
        }
    }
}
