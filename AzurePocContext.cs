using Microsoft.EntityFrameworkCore;

namespace AzurePOC;

public partial class AzurePocContext : DbContext
{
    public AzurePocContext()
    {
    }

    public AzurePocContext(DbContextOptions<AzurePocContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Pokemon> Pokemon { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
        .UseSqlServer("name=AzurePOC");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pokemon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_NewTable");

            entity.ToTable("Pokemon");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.Property(e => e.Types)
                .HasColumnName("types")
                .HasConversion(x => string.Join(',', x), x => x.Split(',', StringSplitOptions.TrimEntries));
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
