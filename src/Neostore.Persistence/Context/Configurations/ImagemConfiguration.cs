using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neostore.Domain.Entities;

namespace Neostore.Persistence.Context.Configurations;

public class ImagemConfiguration : IEntityTypeConfiguration<Imagem>
{
    public void Configure(EntityTypeBuilder<Imagem> builder)
    {
        builder.ToTable("imagens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.NomeArquivo)
            .HasColumnName("nome_arquivo")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(i => i.ChaveS3)
            .HasColumnName("chave_s3")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.TipoConteudo)
            .HasColumnName("tipo_conteudo")
            .HasMaxLength(100);

        builder.Property(i => i.TamanhoBytes)
            .HasColumnName("tamanho_bytes");

        builder.Property(i => i.IdProduto)
            .HasColumnName("id_produto")
            .IsRequired();

        builder.Property(i => i.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.HasIndex(i => i.IdProduto);
    }
}
