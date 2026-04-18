using Dapper;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Position;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class PositionRepository : BaseRepository<Position>, IPositionRepository
    {
        public PositionRepository(IConfiguration configuration,IMemoryCache cache) : base(configuration,cache)
        {
        }

        public async Task<Position> GetPositionByCode(string code)
        {
            
            
            string query = SQLExtension.GetQuery("Position.GetByCode");
            var param = new DynamicParameters();
            param.Add("@PositionCode", code);
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var position = await connection.QueryFirstOrDefaultAsync<Position>(query.ToString(), param);
            return position;
        }
    }
}