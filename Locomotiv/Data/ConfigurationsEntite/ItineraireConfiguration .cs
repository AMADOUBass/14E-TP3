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
    public class ItineraireConfiguration : IEntityTypeConfiguration<Itineraire>
    {
        public void Configure(EntityTypeBuilder<Itineraire> builder)
        {
            builder
                .HasMany(i => i.Etapes)
                .WithOne(e => e.Itineraire)
                .HasForeignKey(e => e.ItineraireId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(i => i.Train)
                .WithOne(t => t.Itineraire)
                .HasForeignKey<Itineraire>(i => i.TrainId);
        }
    }
}
