using Dapper;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Employee;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<Employee> GetEmployeeByCode(string code)
        {
            await OpenConnectionAsync();
            
            string query = SQLExtension.GetQuery("Employee.GetByCode");
            var param = new DynamicParameters();
            param.Add("@EmployeeCode", code);
            return await _dbConnection.QueryFirstOrDefaultAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentId(Guid departmentId)
        {
            await OpenConnectionAsync();
            
            string query = SQLExtension.GetQuery("Employee.GetByDepartmentId");
            var param = new DynamicParameters();
            param.Add("@DepartmentID", departmentId);
            return await _dbConnection.QueryAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPositionId(Guid positionId)
        {
            await OpenConnectionAsync();
            
            string query = SQLExtension.GetQuery("Employee.GetByPositionId");
            var param = new DynamicParameters();
            param.Add("@PositionID", positionId);
            return await _dbConnection.QueryAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }
    }
}