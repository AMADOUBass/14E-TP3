using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Locomotiv.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locomotiv.Data.ConfigurationsEntite
{
    public class EtapeConfiguration : IEntityTypeConfiguration<Etape>
    {
        public void Configure(EntityTypeBuilder<Etape> builder)
        {
            builder.Property(e => e.Lieu).IsRequired();
            builder.Property(e => e.HeureArrivee).IsRequired();
            builder.Property(e => e.HeureDepart).IsRequired();
            builder.Property(e => e.Ordre).IsRequired();

            builder
                .HasOne(e => e.Itineraire)
                .WithMany(i => i.Etapes)
                .HasForeignKey(e => e.ItineraireId);

            builder
                .HasOne(e => e.Block)
                .WithMany()
                .HasForeignKey(e => e.BlockId)
                .OnDelete(DeleteBehavior.SetNull);

            builder 
                .HasOne(e => e.Station)
                .WithMany()
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
