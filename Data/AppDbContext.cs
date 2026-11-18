using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fluxo_GS_2026_2TDSPR.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Estacao> Estacoes { get; set; }
        public DbSet<Trem> Trens { get; set; }
        public DbSet<Vagao> Vagoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var horarioUtc = new ValueConverter<DateTime, DateTime>(
                valor => valor,
                valor => DateTime.SpecifyKind(valor, DateTimeKind.Utc));

            modelBuilder.Entity<Plataforma>()
                .Property(p => p.AtualizadoEm).HasConversion(horarioUtc);
            modelBuilder.Entity<Vagao>()
                .Property(v => v.AtualizadoEm).HasConversion(horarioUtc);

            modelBuilder.Entity<Estacao>().ToTable("TBL_ESTACOES_2TDSPR");

            modelBuilder.Entity<Estacao>().HasKey(e => e.Id);

            modelBuilder.Entity<Estacao>()
                .Property(e => e.Nome)
                .HasColumnName("NM_ESTACAO")
                .IsRequired();

            modelBuilder.Entity<Trem>(trem =>
            {
                trem.ToTable("TBL_TRENS_2TDSPR");
                trem.Property(t => t.Codigo).HasMaxLength(50).IsRequired();
                trem.HasIndex(t => t.Codigo).IsUnique();
            });

            modelBuilder.Entity<Vagao>(vagao =>
            {
                vagao.ToTable("TBL_VAGOES_2TDSPR");
                vagao.HasIndex(v => new { v.TremId, v.Posicao }).IsUnique();
                vagao.HasOne(v => v.Trem)
                    .WithMany(t => t.Vagoes)
                    .HasForeignKey(v => v.TremId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
