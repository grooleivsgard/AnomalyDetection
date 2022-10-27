using AutoMapper;
using DTO.IntrusionDetectionSystem;

using Models;
using static Models.Endpoint;

namespace AutoMapperProfiles.IntrusionDetectionSystem

{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            // source -> Target 
            //Root ReadJsonDto = new Root(); 
            //IEnumerable<Result> result = ReadJsonDto.Data.Result;
            //IEnumerable<Result> Results =  ReadJsonDto.Data.Result; 
            CreateMap<Metric,Connection>(); 
            CreateMap<EndpointItem, Endpoint>(); 
            
        }

    }
}