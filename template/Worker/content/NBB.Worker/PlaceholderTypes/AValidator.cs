#if FluentValidation
using FluentValidation;
using NBB.Application.DataContracts;

namespace NBB.Worker
{
    public class __AValidator__ : AbstractValidator<Command>
    {
        public __AValidator__()
        {
            RuleFor(x => x.Metadata.CommandId).NotEmpty();
        }
    }
}
#endif