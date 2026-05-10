using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neostore.Domain.Entities;

namespace Neostore.Persistence.Context.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Nome)
            .HasColumnName("nome")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasColumnName("slug")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.IdCategoriaPai)
            .HasColumnName("id_categoria_pai");

        builder.HasIndex(c => c.Nome).IsUnique();
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(c => c.IdCategoriaPai)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
