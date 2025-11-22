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
    public class SignalConfiguration : IEntityTypeConfiguration<Signal>
    {
        public void Configure(EntityTypeBuilder<Signal> builder)
        {
            builder.Property(s => s.Type).HasConversion<string>().IsRequired();

            builder.Property(s => s.EstActif).IsRequired();

            builder
                .HasOne(s => s.Station)
                .WithMany(st => st.Signaux)
                .HasForeignKey(s => s.StationId);
        }
    }
}
