using Dapper;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Entities.Employee;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;
using System.Threading.Tasks;
using System.Data;
using FresherMisa2026.Entities;

namespace FresherMisa2026.Infrastructure.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(IConfiguration configuration,IMemoryCache cache) : base(configuration,cache)
        {
        }

        public async Task<Employee> GetEmployeeByCode(string code)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            string query = SQLExtension.GetQuery("Employee.GetByCode");
            var param = new DynamicParameters();
            param.Add("@EmployeeCode", code);
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentId(Guid departmentId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            string query = SQLExtension.GetQuery("Employee.GetByDepartmentId");
            var param = new DynamicParameters();
            param.Add("@DepartmentID", departmentId);
            return await connection.QueryAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPositionId(Guid positionId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            string query = SQLExtension.GetQuery("Employee.GetByPositionId");
            var param = new DynamicParameters();
            param.Add("@PositionID", positionId);
            return await connection.QueryAsync<Employee>(query, param, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Lấy danh sách nhân viên theo filter
        /// </summary>
        /// <param name="departmentId">Mã phòng ban</param>
        /// <param name="positionId">Mã chức vụ</param>
        /// <param name="salaryFrom">Mức lương từ</param>
        /// <param name="salaryTo">Mức lương đến</param>
        /// <param name="gender">Giới tính</param>
        /// <param name="hireDateFrom">Ngày tuyển dụng từ</param>
        /// <param name="hireDateTo">Ngày tuyển dụng đến</param>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <param name="pageIndex">Chỉ số trang</param>
        /// <returns>Danh sách nhân viên</returns>
        /// CREATED BY: NHoang (17/04/2026)
        /// UPDATED BY: NHoang (19/04/2026) THÊM PAGING
        public async Task<PagingResponse<Employee>> GetEmployeesByFilterAsync(
            Guid? departmentId,
            Guid? positionId,
            decimal? salaryFrom,
            decimal? salaryTo,
            int? gender,
            DateTime? hireDateFrom,
            DateTime? hireDateTo,
            int pageSize = 10,
            int pageIndex = 1)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = new StringBuilder("WHERE 1=1");
            var parameters = new DynamicParameters();

            if (departmentId.HasValue)
            {
                query.Append(" AND DepartmentID = @DepartmentID");
                parameters.Add("@DepartmentID", departmentId.Value);
            }

            if (positionId.HasValue)
            {
                query.Append(" AND PositionID = @PositionID");
                parameters.Add("@PositionID", positionId.Value);
            }

            if (salaryFrom.HasValue)
            {
                query.Append(" AND Salary >= @SalaryFrom");
                parameters.Add("@SalaryFrom", salaryFrom.Value);
            }

            if (salaryTo.HasValue)
            {
                query.Append(" AND Salary <= @SalaryTo");
                parameters.Add("@SalaryTo", salaryTo.Value);
            }

            if (gender.HasValue)
            {
                query.Append(" AND Gender = @Gender");
                parameters.Add("@Gender", gender.Value);
            }

            if (hireDateFrom.HasValue)
            {
                query.Append(" AND HireDate >= @HireDateFrom");
                parameters.Add("@HireDateFrom", hireDateFrom.Value);
            }

            if (hireDateTo.HasValue)
            {
                query.Append(" AND HireDate <= @HireDateTo");
                parameters.Add("@HireDateTo", hireDateTo.Value);
            }
            //lấy tổng số bản ghi
            

            var countSql = $"SELECT COUNT(*) FROM Employee {query.ToString()}";
            var total = await connection.ExecuteScalarAsync<long>(countSql, parameters);

            
            var offset = pageSize * (pageIndex - 1);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@Offset", offset);
            var dataSql = $"SELECT * FROM Employee {query.ToString()} LIMIT @PageSize OFFSET @Offset";
            var data = (await connection.QueryAsync<Employee>(dataSql, parameters)).ToList();

            return new PagingResponse<Employee>
            {
                Total = total,
                PageSize = pageSize,
                PageIndex = pageIndex,
                Data = data
            };
        }
    }
}