using AutoMapper;
using EventHost.Web.Entities.Events;
using EventHost.Web.Models.Sessions;

namespace EventHost.Web.Mappers
{
    public static class SessionMappers
    {
        internal static IMapper Mapper { get; }

        static SessionMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SessionMapperProfile>()).CreateMapper();
        }
    }

    public class SessionMapperProfile : Profile
    {
        public SessionMapperProfile()
        {
            CreateMap<Session, EditSessionViewModel>()
                .ForMember(x => x.SectionOptions, x => x.Ignore())
                .ForMember(x => x.UserOptions, x => x.Ignore());
            CreateMap<Session, SessionModel>()
                .ForMember(x => x.HostName, opt => opt.MapFrom(u => u.Host != null ? $"{u.Host.FirstName} {u.Host.LastName}" : u.HostName));
                //.ForMember(x => x.RegistrationCount, opt => opt.MapFrom(x => x.Registrations.Count));
        }
    }

    public static class SessionModelExtensions
    {
        public static SessionModel ToModel(this Session Session)
        {
            return Mapper.Map<SessionModel>(Session);
        }

        public static EditSessionViewModel ToEditModel(this Session Session)
        {
            return Mapper.Map<EditSessionViewModel>(Session);
        }
    }
}
