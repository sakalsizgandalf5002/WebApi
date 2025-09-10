using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using FluentValidation;

namespace Api.Validators.Stock
{
    public class CreateStockRequestDtoValidator : AbstractValidator<CreateStockRequestDto>
    {
        public CreateStockRequestDtoValidator()
        {
            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage("Symbol is required")
                .Length(2, 10).WithMessage("Symbol length must be between 2 and 10 characters")
                .Matches("^[A-Z]+$").WithMessage("Symbol must contain only uppercase letters");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100);

            RuleFor(x => x.Purchase)
                .GreaterThan(0).WithMessage("Purchase price must be greater than zero");

            RuleFor(x => x.MarketCap)
                .GreaterThanOrEqualTo(0).WithMessage("Market cap cannot be negative");

            RuleFor(x => x.Industry)
                .NotEmpty().WithMessage("Industry is required");
         }
    }
}