using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;

namespace PSG.Application.Servicos.Professores
{
    public class ProfessorService
    {
        private readonly IPSGDbContext _context;

        public ProfessorService(IPSGDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Todos os professores em ordem alfabética. Usado para preencher o campo
        /// de professor na edição de módulo.
        /// </summary>
        public async Task<List<ProfessorDto>> ObterTodosOsProfessoresAsync()
        {
            return await _context.Professores
                .OrderBy(p => p.Nome)
                .Select(p => new ProfessorDto(p.IdProfessor, p.Matricula, p.Nome))
                .ToListAsync();
        }
    }
}
