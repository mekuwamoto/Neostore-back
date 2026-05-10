using FluentValidation;
using Neostore.Application.Commands.UsuarioAdmin;

namespace Neostore.Application.Validators;

public class CriarUsuarioAdminCommandValidator : AbstractValidator<CriarUsuarioAdminCommand>
{
    public CriarUsuarioAdminCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ser válido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role é obrigatória");
    }
}
