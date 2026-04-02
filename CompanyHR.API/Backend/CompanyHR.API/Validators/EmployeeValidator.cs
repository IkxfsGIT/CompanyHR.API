cat > Backend/CompanyHR.API/Validators/EmployeeValidator.cs << 'EOF'
using FluentValidation;
using CompanyHR.API.DTOs; // если используете DTO

namespace CompanyHR.API.Validators;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MaximumLength(100).WithMessage("Максимум 100 символов");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Дата найма обязательна");
    }
}
EOF
