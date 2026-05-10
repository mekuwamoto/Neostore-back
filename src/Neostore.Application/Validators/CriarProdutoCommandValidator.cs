using FluentValidation;
using Neostore.Application.Commands.Produto;

namespace Neostore.Application.Validators;

public class CriarProdutoCommandValidator : AbstractValidator<CriarProdutoCommand>
{
    public CriarProdutoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU é obrigatório")
            .MinimumLength(2).WithMessage("SKU deve ter no mínimo 2 caracteres");

        RuleFor(x => x.Preço)
            .GreaterThan(0).WithMessage("Preço deve ser maior que 0");

        RuleFor(x => x.IdCategoria)
            .NotEmpty().WithMessage("IdCategoria é obrigatório");

        RuleFor(x => x.Descrição)
            .NotEmpty().WithMessage("Descrição é obrigatória");

        RuleFor(x => x.Estoque)
            .GreaterThanOrEqualTo(0).WithMessage("Estoque não pode ser negativo");

        RuleForEach(x => x.Imagens)
            .ChildRules(imagem =>
            {
                imagem.RuleFor(i => i.ChaveS3)
                    .NotEmpty().WithMessage("ChaveS3 é obrigatória para cada imagem");
            });
    }
}
