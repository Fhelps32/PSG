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
    public class ModuloMapping : IEntityTypeConfiguration<Modulo>
    {
        public void Configure(EntityTypeBuilder<Modulo> builder)
        {
            builder.HasKey(m => m.IdModulo);
            builder.Property(m => m.Nome).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Numero).IsRequired();
            builder.Property(m => m.DataCadastro).IsRequired();
            builder.Property(m => m.Status).IsRequired();

            builder.HasOne(m => m.Curso)
                   .WithMany(c => c.Modulos)
                   .HasForeignKey(m => m.IdCurso)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
