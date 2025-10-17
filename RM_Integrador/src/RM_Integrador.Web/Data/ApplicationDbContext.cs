using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RM_Integrador.Shared.Models;
using RM_Integrador.Web.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks; // Para o TestConnectionAsync

namespace RM_Integrador.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataServerInfo> DataServers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configura DataServerInfo
            builder.Entity<DataServerInfo>(entity =>
            {
                entity.ToTable("DataServers");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Routine)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                // Configuração atualizada para Keywords
                entity.Property(e => e.Keywords)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                    )
                    .HasColumnType("nvarchar(max)")
                    .Metadata.SetValueComparer(
                        new ValueComparer<List<string>>(
                            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()
                        ));

                // Configuração atualizada para PrimaryKeys
                entity.Property(e => e.PrimaryKeys)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                    )
                    .HasColumnType("nvarchar(max)")
                    .Metadata.SetValueComparer(
                        new ValueComparer<List<string>>(
                            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()
                        ));

                entity.Property(e => e.GetExample)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.PostExample)
                    .HasColumnType("nvarchar(max)");

                // Índices para melhor performance
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Routine);
            });
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await Database.CanConnectAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}