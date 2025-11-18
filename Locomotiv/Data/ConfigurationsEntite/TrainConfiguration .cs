using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Data.ConfigurationsEntite
{
    public class TrainConfiguration : IEntityTypeConfiguration<Train>
    {
        public void Configure(EntityTypeBuilder<Train> builder)
        {
            builder.Property(t => t.Nom).IsRequired();
            builder.Property(t => t.Etat).IsRequired();
            builder.Property(t => t.Capacite).IsRequired();

            builder.HasOne(t => t.Station)
                   .WithMany(s => s.Train)
                   .HasForeignKey(t => t.StationId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(t => t.Block)
                    .WithMany()
                    .HasForeignKey(t => t.BlockId)
                    .OnDelete(DeleteBehavior.SetNull); // ← optionnel
            builder.HasOne(t => t.Itineraire)
                   .WithOne(i => i.Train)
                   .HasForeignKey<Itineraire>(i => i.TrainId)
                   .OnDelete(DeleteBehavior.Cascade); // ou SetNull selon ton besoin
            builder.Property(t => t.Etat)
                     .HasConversion<string>()
                     .IsRequired();
        }
    }
}
