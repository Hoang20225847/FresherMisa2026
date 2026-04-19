using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Employee;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data;
namespace FresherMisa2026.Application.Services


{
    public class EmployeeService : BaseService<Employee>, IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(
            IBaseRepository<Employee> baseRepository,
            IEmployeeRepository employeeRepository
            ) : base(baseRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Employee> GetEmployeeByCodeAsync(string code)
        {
            var employee = await _employeeRepository.GetEmployeeByCode(code);
            if (employee == null)
                throw new Exception("Employee not found");

            return employee;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentIdAsync(Guid departmentId)
        {
            return await _employeeRepository.GetEmployeesByDepartmentId(departmentId);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPositionIdAsync(Guid positionId)
        {
            return await _employeeRepository.GetEmployeesByPositionId(positionId);
        }
         /// <summary>
        /// Validate cho Employee
        /// </summary>
        /// <returns>Danh sách lỗi validate</returns>
        /// CREATED BY: NHoang(17/4/2026)
        protected override async Task<List<ValidationError>> ValidateCustom(Employee employee,Guid? entityId = null)
        {
            var errors = new List<ValidationError>();
        //validate employee code: length > 20 and not exist
            if (!string.IsNullOrEmpty(employee.EmployeeCode))
            {  
                if( employee.EmployeeCode.Length > 20)
                {
                    errors.Add(new ValidationError("EmployeeCode", "Mã nhân viên không được vượt quá 20 ký tự"));
                }
                else{
                        var existingEmployee = await _employeeRepository.GetEmployeeByCode(employee.EmployeeCode);
                        
                   bool isDuplicate = existingEmployee != null 
                        && (!entityId.HasValue || existingEmployee.EmployeeID != entityId.Value);
            
                        if (isDuplicate)
                        {
                            errors.Add(new ValidationError("EmployeeCode", "Mã nhân viên đã tồn tại"));
                        }
                }
              

            }
            //validate required fields
            if (string.IsNullOrEmpty(employee.EmployeeName))
            {
                errors.Add(new ValidationError("EmployeeName", "Tên nhân viên không được để trống"));
            }
            if (employee.DepartmentID == Guid.Empty)
            {
                errors.Add(new ValidationError("DepartmentID", "Phòng ban không được để trống"));
            }
            if (employee.PositionID == Guid.Empty)
            {
                errors.Add(new ValidationError("PositionID", "Vị trí không được để trống"));
            }
            //validate email
            if(!string.IsNullOrEmpty(employee.Email))
            {
                if(!Regex.IsMatch(employee.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    errors.Add(new ValidationError("Email", "Email không hợp lệ"));
                }
            }
            //validate phone number
            if(!string.IsNullOrEmpty(employee.PhoneNumber))
            {
                if(!Regex.IsMatch(employee.PhoneNumber, @"^0[0-9]{9}$"))
                {
                    errors.Add(new ValidationError("PhoneNumber", "Số điện thoại không hợp lệ"));
                }
            }
            //validate date of birth
            if(employee.DateOfBirth.HasValue)
            {
                if(employee.DateOfBirth.Value > DateTime.Now)
                {
                    errors.Add(new ValidationError("DateOfBirth", "Ngày sinh không được lớn hơn ngày hiện tại"));
                }
            }

            return errors;
        }
        /// <summary>
        /// Lọc employee theo filter
        /// </summary>
        /// <returns>Danh sách employee theo filter</returns>
        /// CREATED BY: NHoang(17/4/2026)
        public async Task<IEnumerable<Employee>> GetEmployeesByFilterAsync(
            Guid? departmentId,
            Guid? positionId,
            decimal? salaryFrom,
            decimal? salaryTo,
            int? gender,
            DateTime? hireDateFrom,
            DateTime? hireDateTo)
        {
            return await _employeeRepository.GetEmployeesByFilterAsync(
                departmentId, positionId, salaryFrom, salaryTo, gender, hireDateFrom, hireDateTo);
        }
    }
}