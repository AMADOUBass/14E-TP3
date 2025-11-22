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
    public class StationConfiguration : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> builder)
        {
            builder.Property(s => s.Nom).IsRequired();
            builder.Property(s => s.Localisation).IsRequired();
            builder.Property(s => s.CapaciteMaxTrains).IsRequired();

            builder.HasMany(s => s.Train).WithOne(t => t.Station).HasForeignKey(t => t.StationId);

            builder.HasMany(s => s.Voies).WithOne(v => v.Station).HasForeignKey(v => v.StationId);

            builder
                .HasMany(s => s.Signaux)
                .WithOne(sig => sig.Station)
                .HasForeignKey(sig => sig.StationId);

            builder
                .HasMany(s => s.Employes)
                .WithOne(u => u.Station)
                .HasForeignKey(u => u.StationId)
                .IsRequired(false);
        }
    }
}
