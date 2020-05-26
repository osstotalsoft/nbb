using AutoMapper;
using AutoMapper.Configuration;
using FluentAssertions;
using Xunit;

namespace NBB.Tools.AutoMapperExtensions.Tests
{
    public class AutomapperExtensionsTests
    {
        [Fact]
        public void Should_map_immutable_type_using_constructor_match()
        {
            //Arrange
            var source = new Source(111, 222);
            var config = new MapperConfigurationExpression();
            config.CreateMap<Source, Destination>();
            config.AddProfile<MappingProfile>();

            var mapperConfig = new MapperConfiguration(config);
            var mapper = new Mapper(mapperConfig);
            

            //Act

            var destination = mapper.Map<Destination>(source);

            //Assert
            destination.CommonValue.Should().Be(source.CommonValue);
            destination.DestinationValue.Should().Be(source.SourceValue);
        }

        class MappingProfile: Profile
        {
            public MappingProfile()
            {
                CreateMap<Source, Destination>()
                     .ForCtorParamMatching(dest => dest.DestinationValue, opt => opt.MapFrom(src => src.SourceValue));
            }
        }


        private class Source
        {
            public int SourceValue { get; }
            public int CommonValue { get; }

            public Source(int sourceValue, int commonValue)
            {
                SourceValue = sourceValue;
                CommonValue = commonValue;
            }
        }

        public class Destination
        {
            public int DestinationValue { get; }
            public int CommonValue { get; }

            public Destination(int destinationValue, int commonValue)
            {
                DestinationValue = destinationValue;
                CommonValue = commonValue;
            }
        }
    }
}
