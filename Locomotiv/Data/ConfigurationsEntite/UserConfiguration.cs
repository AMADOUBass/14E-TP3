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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Prenom).IsRequired();
            builder.Property(u => u.Nom).IsRequired();
            builder.Property(u => u.Username).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.PasswordSalt).IsRequired();
            builder.Property(u => u.Role).HasConversion<string>().IsRequired();

            builder
                .HasOne(u => u.Station)
                .WithMany(s => s.Employes)
                .HasForeignKey(u => u.StationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
