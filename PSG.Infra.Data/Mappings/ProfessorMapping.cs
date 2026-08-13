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
    public class ProfessorMapping : IEntityTypeConfiguration<Professor>
    {
        public void Configure(EntityTypeBuilder<Professor> builder)
        {
            builder.HasKey(p => p.IdProfessor);
            builder.Property(p => p.Matricula).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Nome).IsRequired().HasMaxLength(100);
        }
    }
}
