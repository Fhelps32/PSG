using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Infra.Data.Mappings
{
    public class AlunoMapping : IEntityTypeConfiguration<Aluno>
    {
        public void Configure(EntityTypeBuilder<Aluno> builder)
        {
            builder.HasKey(a => a.IdAluno);
            builder.Property(a => a.Nome).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Matricula).IsRequired().HasMaxLength(20);
            builder.Property(a => a.DataCadastro).IsRequired();
            builder.Property(a => a.Status).IsRequired();

            builder.HasOne(a => a.Curso)
                   .WithMany(c => c.Alunos)
                   .HasForeignKey(a => a.IdCurso)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
