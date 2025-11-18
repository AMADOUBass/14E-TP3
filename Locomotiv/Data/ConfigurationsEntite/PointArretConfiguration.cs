using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Locomotiv.Model;
namespace Locomotiv.Data.ConfigurationsEntite
{
    public class PointArretConfiguration : IEntityTypeConfiguration<PointArret>
    {
        public void Configure(EntityTypeBuilder<PointArret> builder)
        {
            builder.HasKey(pa => pa.Id);
            builder.Property(pa => pa.Nom)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(pa => pa.Localisation)
                   .HasMaxLength(200);
            builder.Property(pa => pa.Latitude)
                   .IsRequired();
            builder.Property(pa => pa.Longitude)
                   .IsRequired();
            builder.Property(pa => pa.EstStation)
                   .IsRequired();
        }

    }
}