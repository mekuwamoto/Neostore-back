using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Neostore.Domain.Entities;

namespace Neostore.Persistence.Context.Configurations;

public class UsuarioAdminConfiguration : IEntityTypeConfiguration<UsuarioAdmin>
{
    public void Configure(EntityTypeBuilder<UsuarioAdmin> builder)
    {
        builder.ToTable("usuarios_admin");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
