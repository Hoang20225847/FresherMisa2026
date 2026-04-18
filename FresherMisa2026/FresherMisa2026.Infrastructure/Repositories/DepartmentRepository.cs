using Dapper;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Employee;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for Department entity
    /// </summary>
    /// Created By: dvhai (09/04/2026)
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(IConfiguration configuration) : base(configuration)
        {

        }

        /// <summary>
        /// Lấy department theo code
        /// </summary>
        /// <param name="code">Mã department</param>
        /// <returns>Department tìm thấy hoặc null</returns>
        /// CREATED BY: dvhai (09/04/2026)
        public async Task<Department> GetDepartmentByCode(string code)
        {
            await OpenConnectionAsync();
            
            string query = SQLExtension.GetQuery("Department.GetByCode");
            var @param = new DynamicParameters();
            @param.Add("@DepartmentCode", code);
            return await _dbConnection.QueryFirstOrDefaultAsync<Department>(query, @param, commandType: System.Data.CommandType.Text);
        }
        
        /// <summary>
        /// Lấy danh sách nhân viên theo mã phòng ban
        /// </summary>
        /// <param name="code">Mã phòng ban</param>
        /// <returns>Danh sách nhân viên</returns>
        /// CREATED BY: Hnguyen (18/04/2026)
        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentCode(string code)
        {
            await OpenConnectionAsync();
            
            string query = SQLExtension.GetQuery("Department.GetEmployeesByCode");
            var @param = new DynamicParameters();
            @param.Add("@DepartmentCode", code);
            return await _dbConnection.QueryAsync<Employee>(query, @param, commandType: System.Data.CommandType.Text);
        }
        
        /// <summary>
        /// Đếm số nhân viên trong phòng ban
        /// </summary>
        /// <param name="code">Mã phòng ban</param>
        /// <returns>Số lượng nhân viên</returns>
        /// CREATED BY: Hnguyen (18/04/2026)
        public async Task<int> GetEmployeeCountByDepartmentCode(string code)
        {
            await OpenConnectionAsync();
            
            string query = SQLExtension.GetQuery("Department.GetEmployeeCountByCode");
            var @param = new DynamicParameters();
            @param.Add("@DepartmentCode", code);
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query, @param, commandType: System.Data.CommandType.Text);
        }
    }
}
