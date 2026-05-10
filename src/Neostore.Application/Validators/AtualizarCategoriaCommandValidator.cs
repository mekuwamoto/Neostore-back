using FluentValidation;
using Neostore.Application.Commands.Categoria;

namespace Neostore.Application.Validators;

public class AtualizarCategoriaCommandValidator : AbstractValidator<AtualizarCategoriaCommand>
{
    public AtualizarCategoriaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres");
    }
}
