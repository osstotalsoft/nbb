// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentValidation;
using NBB.Contracts.PublishedLanguage;

namespace NBB.Contracts.Application.CommandValidators
{
    public class ValidateContractValidator : AbstractValidator<ValidateContract>
    {
        public ValidateContractValidator()
        {
            RuleFor(a => a.ContractId).NotEmpty().WithMessage("Invalid contract id");
        }
    }
}
