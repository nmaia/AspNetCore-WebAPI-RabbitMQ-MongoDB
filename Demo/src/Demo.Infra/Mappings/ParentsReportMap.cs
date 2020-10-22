﻿using Demo.Domain.Entities;
using MongoDB.Bson.Serialization;

namespace Demo.Infra.Mappings
{
    public class ParentsReportMap
    {
        public static void ConfigureMap()
        {
            BsonClassMap.RegisterClassMap<ParentsReport>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.MapIdMember(x => x.Id);
                map.MapMember(x => x.Parent);
            });
        }
    }
}
