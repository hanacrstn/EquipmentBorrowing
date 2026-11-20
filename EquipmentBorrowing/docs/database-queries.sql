-- 1. Basic Retrieval
SELECT * FROM Equipment;

-- 2. Filtering
SELECT * FROM Equipment WHERE IsAvailable = 1;

-- 3. Join
SELECT
    s.Name AS Student,
    e.Name AS Equipment,
    b.DateBorrowed AS Borrowed,
    b.ExpectedReturnDate AS Due
FROM Borrowings b
JOIN Students s ON b.StudentId = s.Id
JOIN Equipment e ON b.EquipmentId = e.Id
WHERE b.Status = 0; -- 0 = Active

-- 4. Aggregate
SELECT s.Name AS Student, COUNT(*) AS ActiveBorrowingCount
FROM Borrowings b
JOIN Students s ON b.StudentId = s.Id
WHERE b.Status = 0
GROUP BY s.Name;

-- 5. Update
UPDATE Equipment SET IsAvailable = 1 WHERE Id = '<some-equipment-guid>';