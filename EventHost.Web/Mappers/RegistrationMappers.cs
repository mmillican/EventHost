using AutoMapper;
using EventHost.Web.Entities.Events;
using EventHost.Web.Models.Registrations;

namespace EventHost.Web.Mappers
{
    public static class RegistrationMappers
    {
        internal static IMapper Mapper { get; }

        static RegistrationMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<RegistrationMapperProfile>()).CreateMapper();
        }
    }

    public class RegistrationMapperProfile : Profile
    {
        public RegistrationMapperProfile()
        {
            //CreateMap<Registration, EditRegistrationViewModel>();
            CreateMap<Registration, RegistrationModel>()
                .ForMember(x => x.SectionId, opt => opt.MapFrom(x => x.Session.SectionId))
                .ForMember(x => x.UserName, opt => opt.MapFrom(x => $"{x.User.FirstName} {x.User.LastName}"))
                .ForMember(x => x.UserEmail, opt => opt.MapFrom(x => x.User.Email));
        }
    }

    public static class RegistrationModelExtensions
    {
        public static RegistrationModel ToModel(this Registration Registration)
        {
            return Mapper.Map<RegistrationModel>(Registration);
        }

        //public static EditRegistrationViewModel ToEditModel(this Registration Registration)
        //{
        //    return Mapper.Map<EditRegistrationViewModel>(Registration);
        //}
    }
}
