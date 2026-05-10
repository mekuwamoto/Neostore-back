using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neostore.Domain.Entities;

namespace Neostore.Persistence.Context.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Nome)
            .HasColumnName("nome")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.SKU)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Preço)
            .HasColumnName("preco")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.IdCategoria)
            .HasColumnName("id_categoria")
            .IsRequired();

        builder.Property(p => p.Descrição)
            .HasColumnName("descricao")
            .HasMaxLength(1000);

        builder.Property(p => p.Estoque)
            .HasColumnName("estoque")
            .IsRequired();

        builder.Property(p => p.Ativo)
            .HasColumnName("ativo")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.DeletadoEm)
            .HasColumnName("deletado_em");

        builder.HasQueryFilter(p => p.Ativo);

        builder.HasIndex(p => p.SKU).IsUnique();

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(p => p.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Imagens)
            .WithOne()
            .HasForeignKey(i => i.IdProduto)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
