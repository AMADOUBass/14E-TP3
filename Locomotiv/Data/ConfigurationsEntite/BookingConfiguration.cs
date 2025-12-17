using Microsoft.EntityFrameworkCore;
using Locomotiv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Locomotiv.Data.ConfigurationsEntite
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.NombrePlaces)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(b => b.DateReservation)
               .IsRequired();

            builder.Property(b => b.PrixTotal)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(b => b.Statut)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Confirmé");

            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Itineraire)
                .WithMany()
                .HasForeignKey(b => b.ItineraireId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.ItineraireId);
            builder.HasIndex(b => b.DateReservation);

        }
    }
}
