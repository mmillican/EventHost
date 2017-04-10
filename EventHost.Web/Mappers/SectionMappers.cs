using AutoMapper;
using EventHost.Web.Entities.Events;
using EventHost.Web.Models.Sections;

namespace EventHost.Web.Mappers
{
    public static class SectionMappers
    {
        internal static IMapper Mapper { get; }

        static SectionMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<SectionMapperProfile>()).CreateMapper();
        }
    }

    public class SectionMapperProfile : Profile
    {
        public SectionMapperProfile()
        {
            CreateMap<Section, EditSectionViewModel>();
            CreateMap<Section, SectionModel>();
        }
    }

    public static class SectionModelExtensions
    {
        public static SectionModel ToModel(this Section section)
        {
            return Mapper.Map<SectionModel>(section);
        }

        public static EditSectionViewModel ToEditModel(this Section section)
        {
            return Mapper.Map<EditSectionViewModel>(section);
        }
    }
}
