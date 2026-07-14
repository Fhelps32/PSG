using PSG.Application.Context;
using PSG.Domain;
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

        public async Task<List<Curso>> GetAllCursosAsync()
        {
            return await Task.FromResult(_context.Cursos.ToList());
        }
    }
}
