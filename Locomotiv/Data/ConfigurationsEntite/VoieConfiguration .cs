using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Data.ConfigurationsEntite
{
    public class VoieConfiguration : IEntityTypeConfiguration<Voie>
    {
        public void Configure(EntityTypeBuilder<Voie> builder)
        {
            builder.Property(v => v.Nom).IsRequired();

            builder.HasOne(v => v.Station)
                   .WithMany(s => s.Voies)
                   .HasForeignKey(v => v.StationId);
        }
    }
}
