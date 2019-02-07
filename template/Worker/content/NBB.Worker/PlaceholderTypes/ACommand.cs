using System;
using NBB.Application.DataContracts;

namespace NBB.Worker
{
    public class __ACommand__ : Command
    {
        public __ACommand__(Guid commandId, ApplicationMetadata metadata) : base(commandId, metadata)
        {
        }
    }
}
