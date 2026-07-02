using Microsoft.EntityFrameworkCore;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Infra.Data.Mappings
{
    public class AlunoModuloMapping : IEntityTypeConfiguration<AlunoModulo>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AlunoModulo> builder)
        {
            builder.ToTable("AlunoModulo");
            builder.HasKey(am => am.IdAlunoModulo);
            builder.Property(am => am.DataAcesso).IsRequired();
            builder.Property(am => am.Status).IsRequired();
            builder.Property(am => am.StatusInscricao).IsRequired();

            builder.HasOne(am => am.Aluno)
                   .WithMany(a => a.Modulos)
                   .HasForeignKey(am => am.IdAluno);
            builder.HasOne(am => am.Modulo)
                   .WithMany(m => m.Alunos)
                   .HasForeignKey(am => am.IdModulo);
        }
    }
}
