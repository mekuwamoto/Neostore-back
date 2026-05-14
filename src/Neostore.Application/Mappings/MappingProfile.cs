using AutoMapper;
using Neostore.Application.DTOs;
using Neostore.Domain.Entities;

namespace Neostore.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Categoria, CategoriaDto>()
            .ForMember(dest => dest.Id,            opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nome,           opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Slug,           opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.IdCategoriaPai, opt => opt.MapFrom(src => src.IdCategoriaPai));

        CreateMap<Produto, ProdutoDto>()
            .ForMember(dest => dest.Id,          opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nome,         opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.SKU,          opt => opt.MapFrom(src => src.SKU))
            .ForMember(dest => dest.Preço,        opt => opt.MapFrom(src => src.Preço))
            .ForMember(dest => dest.IdCategoria,  opt => opt.MapFrom(src => src.IdCategoria))
            .ForMember(dest => dest.Descrição,    opt => opt.MapFrom(src => src.Descrição))
            .ForMember(dest => dest.Imagens,      opt => opt.MapFrom(src => src.Imagens))
            .ForMember(dest => dest.Estoque,      opt => opt.MapFrom(src => src.Estoque));

        CreateMap<Imagem, ImagemDto>()
            .ForMember(dest => dest.Id,           opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NomeArquivo,  opt => opt.MapFrom(src => src.NomeArquivo))
            .ForMember(dest => dest.ChaveS3,      opt => opt.MapFrom(src => src.ChaveS3))
            .ForMember(dest => dest.TipoConteudo, opt => opt.MapFrom(src => src.TipoConteudo))
            .ForMember(dest => dest.TamanhoBytes, opt => opt.MapFrom(src => src.TamanhoBytes))
            .ForMember(dest => dest.DataCriacao,  opt => opt.MapFrom(src => src.DataCriacao));

        CreateMap<UsuarioAdmin, UsuarioAdminDto>()
            .ForMember(dest => dest.Id,    opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role,  opt => opt.MapFrom(src => src.Role));
    }
}
