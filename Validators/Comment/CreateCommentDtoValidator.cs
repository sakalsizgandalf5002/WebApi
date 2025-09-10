using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs.Comment;
using FluentValidation;

namespace Api.Validators.Comment
{
    public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Title is required.")
                .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("Title is required.")
                .Must(s => s.Trim().Length >= 8).WithMessage("Title must be at least 8 characters long.")
                .Must(s => s.Trim().Length <= 256).WithMessage("Title cannot exceed 256 characters.");

            RuleFor(x => x.Body)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Body is required.")
                .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("Body is required.")
                .Must(s => s.Trim().Length >= 8).WithMessage("Body must be at least 8 characters long.")
                .Must(s => s.Trim().Length <= 256).WithMessage("Body cannot exceed 256 characters.");
        }
    }
}