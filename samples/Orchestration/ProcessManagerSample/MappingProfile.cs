using AutoMapper;
using ProcessManagerSample.Events;

namespace ProcessManagerSample
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrderShipped, OrderCompleted>()
                .ConstructUsing(shipped => new OrderCompleted(shipped.OrderId, 0, shipped.DocumentId, shipped.SiteId));
            CreateMap<OrderCreated, OrderCompleted>()
                .ConstructUsing(created => new OrderCompleted(created.OrderId, created.Amount, created.DocumentId, created.SiteId));
        }
    }
}