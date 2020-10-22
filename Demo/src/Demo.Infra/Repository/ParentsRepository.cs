﻿using Demo.Contracts.Database;
using Demo.Contracts.Repository;
using Demo.Domain.Entities;
using Demo.Infra.Repository.Base;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Demo.Infra.Repository
{
    public class ParentsRepository
        : BaseRepository<ParentsReport>
        , IParentsRepository
    {
        public ParentsRepository(IMongoDBContext context)
            : base(context)
        {

        }

        public virtual async Task<ParentsReport> GetParentsById(string id)
            => (await DbSet.FindAsync(Builders<ParentsReport>.Filter.Eq("_id", id))).FirstOrDefault();
    }
}
