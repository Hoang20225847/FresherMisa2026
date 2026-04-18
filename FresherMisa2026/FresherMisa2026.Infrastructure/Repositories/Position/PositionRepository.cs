using Dapper;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Position;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class PositionRepository : BaseRepository<Position>, IPositionRepository
    {
        public PositionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<Position> GetPositionByCode(string code)
        {
            await OpenConnectionAsync();
            
            string query = SQLExtension.GetQuery("Position.GetByCode");
            var param = new DynamicParameters();
            param.Add("@PositionCode", code);
            return await _dbConnection.QueryFirstOrDefaultAsync<Position>(query, param, commandType: System.Data.CommandType.Text);
        }
    }
}