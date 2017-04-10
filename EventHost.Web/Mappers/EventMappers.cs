using AutoMapper;
using EventHost.Web.Entities.Events;
using EventHost.Web.Models.Events;

namespace EventHost.Web.Mappers
{
    public static class EventMappers
    {
        internal static IMapper Mapper { get; }

        static EventMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<EventMapperProfile>()).CreateMapper();
        }
    }

    public class EventMapperProfile : Profile
    {
        public EventMapperProfile()
        {
            CreateMap<Event, EditEventViewModel>();
            CreateMap<Event, EventModel>()
                .ForMember(x => x.OwnerName, x => x.MapFrom(u => $"{u.Owner.FirstName} {u.Owner.LastName}"));
        }
    }

    public static class EventModelExtensions
    {
        public static EventModel ToModel(this Event evt)
        {
            return Mapper.Map<EventModel>(evt);
        }

        public static EditEventViewModel ToEditModel(this Event evt)
        {
            return Mapper.Map<EditEventViewModel>(evt);
        }
    }
}
