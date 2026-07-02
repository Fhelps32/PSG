using Microsoft.EntityFrameworkCore;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Infra.Data
{
    public class PSGDbContext : DbContext
    {
        public PSGDbContext(DbContextOptions<PSGDbContext> options) : base(options) 
        {
        }

        public DbSet<Curso> Cursos { get; set; } = null!;
        public DbSet<Aluno> Alunos { get; set; } = null!;
        public DbSet<Modulo> Modulos { get; set; } = null!;
        public DbSet<AlunoModulo> AlunoModulos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PSGDbContext).Assembly);
        }
    }
}
