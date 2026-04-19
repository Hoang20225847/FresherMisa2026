-- ============================================================
-- TASK 3.4: Tối ưu SQL Query với Index
-- Database: misaemployee_development
-- Created by: NHoang
-- Date: 2026-04-19
-- ============================================================
USE misaemployee_development;


-- ---
-- INDEX 1: Don (DepartmentID)
-- Phuc vu: SELECT * FROM Employee WHERE DepartmentID = @DepartmentID
-- Tai sao: Bang employee khong co FOREIGN KEY constraint
--          nen MySQL khong tu dong tao index cho cot nay
--          Query don le chi co DepartmentID se bi Full Table Scan
--          neu khong co index nay
-- ---
CREATE INDEX IX_Employee_DepartmentID
ON employee (DepartmentID);

-- ---
-- INDEX 2: Don (PositionID)
-- Phuc vu: SELECT * FROM Employee WHERE PositionID = @PositionID
-- Tai sao: Tuong tu DepartmentID, khong co FK constraint
--          Composite index (DepartmentID, PositionID, Salary)
--          KHONG the su dung khi query chi co PositionID
--          vi vi pham Leftmost Prefix Rule
-- ---
CREATE INDEX IX_Employee_PositionID
ON employee (PositionID);

-- ---
-- INDEX 3: Don (HireDate)
-- Phuc vu: SELECT * FROM Employee WHERE HireDate BETWEEN @From AND @To
--          (truong hop chi co HireDate, khong co Gender)
-- Tai sao: Composite index (Gender, HireDate) KHONG dung duoc
--          khi query khong co dieu kien Gender (Leftmost Prefix Rule)
--          Can index rieng de phuc vu range query tren HireDate
-- ---
CREATE INDEX IX_Employee_HireDate
ON employee (HireDate);

-- ============================================================
-- CAC COMPOSITE INDEX (Multi Column Index)
-- ============================================================

-- ---
-- INDEX 4: Composite (DepartmentID, PositionID, Salary)
-- Phuc vu: Dynamic filter Task 3.3
--   WHERE DepartmentID = ?
--   AND PositionID = ?
--   AND Salary >= ? AND Salary <= ?
-- Tai sao: 3 filter nay hay di cung nhau nhat trong thuc te
--          Composite index hieu qua hon 3 index don rieng le
--          khi co du ca 3 dieu kien trong WHERE clause
--          MySQL chi can quet 1 index thay vi ket hop nhieu index
-- Cover duoc cac truong hop:
--   WHERE DepartmentID = ?                                    OK (prefix A)
--   WHERE DepartmentID = ? AND PositionID = ?                 OK (prefix A,B)
--   WHERE DepartmentID = ? AND PositionID = ? AND Salary >= ? OK (prefix A,B,C)
-- ---
CREATE INDEX IX_Employee_DepartmentID_PositionID_Salary
ON employee (DepartmentID, PositionID, Salary);

-- ---
-- INDEX 5: Composite (Gender, HireDate)
-- Phuc vu: WHERE Gender = ? AND HireDate BETWEEN @From AND @To
-- Tai sao: Gender (3 gia tri: 0/1/2) + HireDate range
--          thuong duoc filter cung luc trong bao cao nhan su
--          Gender loc truoc (cardinality thap) giup thu hep
--          tap du lieu, HireDate range loc tiep trong index
--          → rat hieu qua, tranh Full Table Scan
-- Cover duoc cac truong hop:
--   WHERE Gender = ?                                  OK (prefix A)
--   WHERE Gender = ? AND HireDate BETWEEN ? AND ?     OK (prefix A,B)
-- ---
CREATE INDEX IX_Employee_Gender_HireDate
ON employee (Gender, HireDate);

-- ============================================================
-- VERIFY KET QUA
-- Chay lenh nay sau khi tao index de kiem tra
-- ============================================================
SHOW INDEX FROM employee;

-- ============================================================
-- KIEM TRA HIEU QUA VOI EXPLAIN
-- So sanh truoc va sau khi tao index
-- ============================================================

-- Test 1: Filter theo DepartmentID
-- Ket qua mong doi: type = ref, key = IX_Employee_DepartmentID
EXPLAIN SELECT * FROM employee
WHERE DepartmentID = '550e8400-e29b-41d4-a716-446655440012';

-- Test 2: Filter theo PositionID
-- Ket qua mong doi: type = ref, key = IX_Employee_PositionID
EXPLAIN SELECT * FROM employee
WHERE PositionID = '22222222-2222-2222-2222-222222222222';

-- Test 3: Composite filter
-- Ket qua mong doi: type = range, key = IX_Employee_DepartmentID_PositionID_Salary
EXPLAIN SELECT * FROM employee
WHERE DepartmentID = '550e8400-e29b-41d4-a716-446655440012'
AND PositionID = '22222222-2222-2222-2222-222222222222'
AND Salary >= 15000000;

-- Test 4: Filter HireDate co Gender
-- Ket qua mong doi: type = range, key = IX_Employee_Gender_HireDate
EXPLAIN SELECT * FROM employee
WHERE Gender = 1
AND HireDate BETWEEN '2018-01-01' AND '2025-01-01';

-- Test 5: Filter chi HireDate (khong co Gender)
-- Ket qua mong doi: type = range, key = IX_Employee_HireDate
EXPLAIN SELECT * FROM employee
WHERE HireDate BETWEEN '2018-01-01' AND '2025-01-01';