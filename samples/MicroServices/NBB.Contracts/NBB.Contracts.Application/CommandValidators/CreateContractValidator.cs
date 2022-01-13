// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentValidation;
using NBB.Contracts.PublishedLanguage;

namespace NBB.Contracts.Application.CommandValidators
{
    public class CreateContractValidator : AbstractValidator<CreateContract>
    {
        public CreateContractValidator()
        {
            RuleFor(a => a.ClientId).NotEmpty();
        }
    }
}
