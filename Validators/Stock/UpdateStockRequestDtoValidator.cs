using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs.Stock;
using FluentValidation;

namespace Api.Validators.Stock
{
    public class UpdateStockRequestDtoValidator : AbstractValidator<UpdateStockRequestDto>
    {
        public UpdateStockRequestDtoValidator()
        {
            RuleFor(x => x.CompanyName)
               .NotEmpty().WithMessage("Company name is required")
               .MaximumLength(100);

            RuleFor(x => x.Purchase)
                .GreaterThan(0);

            RuleFor(x => x.MarketCap)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Industry)
                .NotEmpty();
        }
    }
}