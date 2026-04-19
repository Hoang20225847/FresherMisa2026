using FresherMisa2026.Entities.Employee;
using System;
using System.Collections.Generic;
using FresherMisa2026.Entities;


namespace FresherMisa2026.Application.Interfaces.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<Employee> GetEmployeeByCode(string code);
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentId(Guid departmentId);
        Task<IEnumerable<Employee>> GetEmployeesByPositionId(Guid positionId);
        Task<PagingResponse<Employee>> GetEmployeesByFilterAsync(
            Guid? departmentId,
            Guid? positionId,
            decimal? salaryFrom,
            decimal? salaryTo,
            int? gender,
            DateTime? hireDateFrom,
            DateTime? hireDateTo,
            int pageSize = 10,
            int pageIndex = 1);
    }
}
