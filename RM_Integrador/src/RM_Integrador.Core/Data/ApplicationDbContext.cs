using Microsoft.EntityFrameworkCore;
using RM_Integrador.Core.Entities;

namespace RM_Integrador.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataServer> DataServers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataServer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Routine).IsRequired();
                
                // Converter para JSON
                entity.Property(e => e.PrimaryKeys)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, null));
                
                entity.Property(e => e.Keywords)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, null));
            });
        }
    }
}