using FluentValidation;
using Neostore.Application.Commands.UsuarioAdmin;

namespace Neostore.Application.Validators;

public class AtualizarSenhaCommandValidator : AbstractValidator<AtualizarSenhaCommand>
{
    public AtualizarSenhaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório");

        RuleFor(x => x.SenhaAtual)
            .NotEmpty().WithMessage("Senha atual é obrigatória");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("Nova senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres");
    }
}
