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
        public DbSet<Trem> Trens { get; set; }
        public DbSet<Vagao> Vagoes { get; set; }

        public DbSet<ParadaProgramada> ParadasProgramadas { get; set; }

        public DbSet<MedicaoPlataforma> MedicoesPlataforma { get; set; }

        public DbSet<MedicaoVagao> MedicoesVagao { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<ParadaProgramada>(parada =>
            {
                parada.ToTable("TBL_PARADAS_2TDSPR");
                parada.HasIndex(p => new { p.PlataformaId, p.ChegadaPrevistaEm });
                parada.HasOne(p => p.Trem).WithMany().HasForeignKey(p => p.TremId)
                    .OnDelete(DeleteBehavior.Restrict);
                parada.HasOne(p => p.Plataforma).WithMany().HasForeignKey(p => p.PlataformaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MedicaoPlataforma>(medicao =>
            {
                medicao.ToTable("TBL_MED_PLATAFORMA_2TDSPR");
                medicao.Property(m => m.Origem).HasMaxLength(50).IsRequired();
                medicao.HasIndex(m => new { m.PlataformaId, m.MedidaEm });
                medicao.HasOne<Plataforma>().WithMany().HasForeignKey(m => m.PlataformaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MedicaoVagao>(medicao =>
            {
                medicao.ToTable("TBL_MED_VAGAO_2TDSPR");
                medicao.Property(m => m.Origem).HasMaxLength(50).IsRequired();
                medicao.HasIndex(m => new { m.VagaoId, m.MedidaEm });
                medicao.HasOne<Vagao>().WithMany().HasForeignKey(m => m.VagaoId)
                    .OnDelete(DeleteBehavior.Restrict);
                medicao.HasOne<ParadaProgramada>().WithMany().HasForeignKey(m => m.ParadaProgramadaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}