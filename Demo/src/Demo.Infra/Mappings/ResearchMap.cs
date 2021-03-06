﻿using Demo.Domain.Entities;
using MongoDB.Bson.Serialization;

namespace Demo.Infra.Mappings
{
    public static class ResearchMap
    {
        public static void ConfigureMap()
        {
            BsonClassMap.RegisterClassMap<Research>(map => {

                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.MapMember(x => x.Region);
                map.MapMember(x => x.Person);

            });
        }
    }
}
