using TaskManagement.Application.DTOs.Requests;
using FluentValidation;

namespace TaskManagement.Application.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid task status");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("DueDate is required");
    }
}
