using Locomotiv.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locomotiv.Data.ConfigurationsEntite
{
    public class BlockConfiguration : IEntityTypeConfiguration<Block>
    {
        public void Configure(EntityTypeBuilder<Block> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Nom).IsRequired().HasMaxLength(100);

            builder.Property(b => b.Signal).IsRequired();

            builder.Property(b => b.EstOccupe).IsRequired();

            builder
                .HasOne(b => b.Train)
                .WithMany()
                .HasForeignKey(b => b.TrainId)
                .OnDelete(DeleteBehavior.SetNull); // ou Restrict selon ton besoin
        }
    }
}
