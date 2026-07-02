using Microsoft.EntityFrameworkCore;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Context
{
    public interface IPSGDbContext
    {
        DbSet<Curso> Cursos { get; }
        DbSet<Aluno> Alunos { get; }
        DbSet<Modulo> Modulos { get; }
        DbSet<AlunoModulo> AlunoModulos { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
