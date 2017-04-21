using AutoMapper;
using EventHost.Web.Entities.Users;
using EventHost.Web.Models.Users;

namespace EventHost.Web.Mappers
{
    public static class UserMappers
    {
        internal static IMapper Mapper { get; }

        static UserMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserMapperProfile>()).CreateMapper();
        }
    }

    public class UserMapperProfile : Profile
    {
        public UserMapperProfile()
        {
            CreateMap<User, UserModel>();
        }
    }

    public static class UserModelExtensions
    {
        public static UserModel ToModel(this User user)
        {
            return Mapper.Map<UserModel>(user);
        }
    }
}
