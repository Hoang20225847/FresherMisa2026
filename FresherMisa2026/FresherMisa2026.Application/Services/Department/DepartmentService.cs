using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using System;
using FresherMisa2026.Entities.Employee;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Services
{
    public class DepartmentService : BaseService<Department>, IDepartmentSerice
    {
        private readonly IDepartmentRepository _deptRepository;

        public DepartmentService(
            IBaseRepository<Department> baseRepository,
            IDepartmentRepository departmentRepository
            ) : base(baseRepository)
        {
            _deptRepository = departmentRepository;
        }

        /// <summary>
        /// Lấy department theo code
        /// </summary>
        /// <returns></returns>
        /// Created By: dvhai (10/04/2026)
        public async Task<Department> GetDepartmentByCodeAsync(string code)
        {
            var department = await _deptRepository.GetDepartmentByCode(code);
            if (department == null)
                throw new Exception("department is null");

            return department;
        }

        #region OVERRIDE METHODS
        protected override async Task<bool> ValidateBeforeDeleteAsync(Guid entityId)
        {
            //1. Validate còn nhân viên trong phòng ban không
            bool hasEmployee = true;

            return !hasEmployee;
        }
        /// <summary>
        /// Lấy danh sách nhân viên theo mã phòng ban
        /// </summary>
        /// <param name="code">Mã phòng ban</param>
        /// <returns>Danh sách nhân viên</returns>
        /// Created By: Hnguyen (18/04/2026)
        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentCode(string code)
        {
            return await _deptRepository.GetEmployeesByDepartmentCode(code);
        }
        /// <summary>
        /// Đếm số nhân viên trong phòng ban
        /// </summary>
        /// <param name="code">Mã phòng ban</param>
        /// <returns>Số lượng nhân viên</returns>
        /// Created By: Hnguyen (18/04/2026)
        public async Task<int> GetEmployeeCountByDepartmentCode(string code)
        {
            return await _deptRepository.GetEmployeeCountByDepartmentCode(code);
        }

        /// <summary>
        /// Validate tùy chỉnh cho Department
        /// </summary>
        protected override async Task<List<ValidationError>> ValidateCustom(Department department)
        {
            var errors = new List<ValidationError>();

            // Ví dụ: Kiểm tra mã phòng ban không được vượt quá 20 ký tự
            if (!string.IsNullOrEmpty(department.DepartmentCode) && department.DepartmentCode.Length > 20)
            {
                errors.Add(new ValidationError("DepartmentCode", "Mã phòng ban không được vượt quá 20 ký tự"));
            }

            return errors;
        }
        #endregion OVERRIDE METHODS
    }
}
