using AutoMapper;
using Neostore.Application.DTOs;
using Neostore.Domain.Entities;

namespace Neostore.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Categoria, CategoriaDto>();
        CreateMap<Produto, ProdutoDto>();
        CreateMap<Imagem, ImagemDto>();
        CreateMap<UsuarioAdmin, UsuarioAdminDto>();
    }
}
