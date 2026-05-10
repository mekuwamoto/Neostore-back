namespace Neostore.Domain.Interfaces;

public interface ISoftDeletable
{
    bool Ativo { get; set; }
    DateTime? DeletadoEm { get; set; }
}
