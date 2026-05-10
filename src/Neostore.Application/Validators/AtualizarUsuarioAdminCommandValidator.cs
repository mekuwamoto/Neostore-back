using FluentValidation;
using Neostore.Application.Commands.UsuarioAdmin;

namespace Neostore.Application.Validators;

public class AtualizarUsuarioAdminCommandValidator : AbstractValidator<AtualizarUsuarioAdminCommand>
{
    public AtualizarUsuarioAdminCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ser válido");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role é obrigatória");
    }
}
