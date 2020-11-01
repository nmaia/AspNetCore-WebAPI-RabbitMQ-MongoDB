﻿using Demo.Domain.Entities;
using Demo.Domain.Enums;
using Demo.Infra.Contracts.MongoDB;
using Demo.Infra.Contracts.Repository;
using Demo.Infra.DTO;
using Demo.Infra.Repository.Base;
using LinqKit;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Infra.Repository
{
    public class ResearchRepository 
        : BaseRepository<Research>
        , IResearchRepository
    {
        public ResearchRepository(IMongoDBContext context)
            : base(context)
        {

        }

        public async Task<IEnumerable<Research>> GetFilteredResearches(Research filter)
        {
            var predicate = PredicateBuilder.New<Research>();

            if (!string.IsNullOrEmpty(filter.Region.ToString()))
                predicate = predicate.And(r => r.Region == (Region)Enum.Parse(typeof(Region), filter.Region.ToString().ToUpper()));

            if (!string.IsNullOrEmpty(filter.Person.FirstName))
                predicate = predicate.And(r => r.Person.FirstName.ToLower().Contains(filter.Person.FirstName.ToLower()));

            if (!string.IsNullOrEmpty(filter.Person.Gender.ToString()))
                predicate = predicate.And(r => r.Person.Gender == (Gender)Enum.Parse(typeof(Gender), filter.Person.Gender.ToString().ToUpper()));

            if (!string.IsNullOrEmpty(filter.Person.SkinColor.ToString()))
                predicate = predicate.And(r => r.Person.SkinColor == (SkinColor)Enum.Parse(typeof(SkinColor), filter.Person.SkinColor.ToString().ToUpper()));

            if (!string.IsNullOrEmpty(filter.Person.Schooling.ToString()))
                predicate = predicate.And(r => r.Person.Schooling == (Schooling)Enum.Parse(typeof(Schooling), filter.Person.Schooling.ToString().ToUpper()));

            var queryResult = await Task.Run(() =>
            {

                return DbSet.AsQueryable<Research>()
                            .Where(predicate).ToListAsync();
            });

            return queryResult;
        }

        public async Task<IEnumerable<FilteredResearchGrouped>> GetFilteredResearchesGrouped(Research filter)
        {
            var nonGroupedResponse = (await GetFilteredResearches(filter)).ToList();

            var groupedResponse = nonGroupedResponse.AsEnumerable()
                    .Select(x => new
                    {

                        Region = x.Region.ToString(),
                        FirstName = x.Person.FirstName,
                        Gender = x.Person.Gender.ToString(),
                        SkinColor = x.Person.SkinColor.ToString(),
                        Schooling = x.Person.Schooling.ToString(),

                    }).GroupBy(g => new
                    {

                        g.Region,
                        g.FirstName,
                        g.Gender,
                        g.Schooling,
                        g.SkinColor

                    }).Select(s => new FilteredResearchGrouped
                    {

                        Region = s.Key.Region,
                        FirstName = s.Key.FirstName,
                        Gender = s.Key.Gender,
                        SkinColor = s.Key.SkinColor,
                        Schooling = s.Key.Schooling,
                        Quantity = Convert.ToInt16(s.Count()).ToString()

                    });

            return groupedResponse;
        }
    }
}
