using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.EntityFrameworkCore;

namespace Fluxo_GS_2026_2TDSPR.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Estacao> Estacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Estacao>().ToTable("TBL_ESTACOES_2TDSPR");

            modelBuilder.Entity<Estacao>().HasKey(e => e.Id);

            modelBuilder.Entity<Estacao>()
                .Property(e => e.Nome)
                .HasColumnName("NM_ESTACAO")
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}