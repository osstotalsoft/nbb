// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentValidation;
using NBB.Contracts.PublishedLanguage;

namespace NBB.Contracts.Application.CommandValidators
{
    public class AddContractLineValidator : AbstractValidator<AddContractLine>
    {
        public AddContractLineValidator()
        {
            RuleFor(a => a.Product).NotEmpty().WithMessage("Invalid product");
            RuleFor(a => a.Product).MaximumLength(200).WithMessage("Maximum lenght for product is 200");
            RuleFor(a => a.ContractId).NotEmpty().WithMessage("Invalid contract id"); ;
            RuleFor(a => a.Price).Must(x => x > 0).WithMessage("Enter a positive price");
            RuleFor(a => a.Quantity).Must(x => x > 0).WithMessage("Enter a quantity price");
        }
    }
}
