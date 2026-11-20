-- Appendix C. In pgAdmin execute one labeled section at a time.
-- Use the disposable database equipment_borrowing_lab4_test.

-- A. Schema and migrations
SELECT current_database(), current_user;
SELECT * FROM public."__EFMigrationsHistory";
SELECT tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public' AND tablename IN ('Students', 'Equipment', 'Borrowings')
ORDER BY tablename, indexname;
SELECT conrelid::regclass AS table_name, conname, pg_get_constraintdef(oid)
FROM pg_constraint
WHERE conrelid IN ('public."Students"'::regclass, 'public."Equipment"'::regclass,
                   'public."Borrowings"'::regclass)
ORDER BY 1, 2;

-- B1. Fresh readback before and after the forced failure
SELECT b.* FROM public."Borrowings" b
JOIN public."Students" s ON s."Id" = b."StudentId"
WHERE s."StudentNumber" = 'QA-ROLLBACK' ORDER BY b."Id";
SELECT "Id", "Identifier", "IsAvailable"
FROM public."Equipment" WHERE "Identifier" IN ('EQ-004', 'EQ-005')
ORDER BY "Identifier";

-- B2. Install the rollback test seam. Select this entire block through COMMIT.
BEGIN;
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab4_test' THEN
        RAISE EXCEPTION 'This section requires equipment_borrowing_lab4_test';
    END IF;
    IF NOT EXISTS (
        SELECT 1 FROM public."Students"
        WHERE "StudentNumber" = 'QA-ROLLBACK' AND "IsAllowedToBorrow"
    ) OR EXISTS (
        SELECT 1 FROM public."Borrowings" b
        JOIN public."Students" s ON s."Id" = b."StudentId"
        WHERE s."StudentNumber" = 'QA-ROLLBACK' AND b."Status" = 0
    ) THEN
        RAISE EXCEPTION 'QA-ROLLBACK must exist, be eligible, and have zero active loans';
    END IF;
END;
$$;
CREATE SEQUENCE public.lab4_rollback_witness START WITH 1;
CREATE FUNCTION public.lab4_fail_second_issue() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW."Status" <> 0 OR NOT EXISTS (
        SELECT 1 FROM public."Students"
        WHERE "Id" = NEW."StudentId" AND "StudentNumber" = 'QA-ROLLBACK'
    ) THEN
        RETURN NEW;
    END IF;
    IF EXISTS (
        SELECT 1 FROM public."Borrowings"
        WHERE "StudentId" = NEW."StudentId" AND "Status" = 0 AND "Id" <> NEW."Id"
    ) THEN
        PERFORM nextval('public.lab4_rollback_witness');
        RAISE EXCEPTION 'Appendix C: controlled failure after the first insert';
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER lab4_fail_second_issue
BEFORE INSERT ON public."Borrowings"
FOR EACH ROW EXECUTE FUNCTION public.lab4_fail_second_issue();
COMMIT;

-- B3. Confirm an insert reached the trigger before rollback
SELECT last_value, is_called FROM public.lab4_rollback_witness;

-- B4. Remove the seam before retrying. Select through COMMIT.
BEGIN;
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab4_test' THEN
        RAISE EXCEPTION 'This section requires equipment_borrowing_lab4_test';
    END IF;
END;
$$;
DROP TRIGGER lab4_fail_second_issue ON public."Borrowings";
DROP FUNCTION public.lab4_fail_second_issue();
DROP SEQUENCE public.lab4_rollback_witness;
COMMIT;

-- C1. Normalized duplicate student number. Expect unique violation (23505).
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab4_test' THEN
        RAISE EXCEPTION 'This section requires equipment_borrowing_lab4_test';
    END IF;
    INSERT INTO public."Students" ("StudentNumber", "Name", "IsAllowedToBorrow")
    VALUES (' st-001 ', 'Appendix C probe', true);
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Probe did not find its fixture';
    END IF;
    RAISE EXCEPTION 'Probe reached completion without the expected constraint rejection. Probe rolled back.';
END;
$$;

-- C2. Normalized duplicate equipment identifier. Expect unique violation (23505).
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab4_test' THEN
        RAISE EXCEPTION 'This section requires equipment_borrowing_lab4_test';
    END IF;
    INSERT INTO public."Equipment" ("Identifier", "Name", "IsAvailable")
    VALUES (' eq-001 ', 'Appendix C probe', true);
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Probe did not find its fixture';
    END IF;
    RAISE EXCEPTION 'Probe reached completion without the expected constraint rejection. Probe rolled back.';
END;
$$;

-- C3. Missing student foreign key. Expect foreign key violation (23503).
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab4_test' THEN
        RAISE EXCEPTION 'This section requires equipment_borrowing_lab4_test';
    END IF;
    INSERT INTO public."Borrowings" ("Id", "StudentId", "EquipmentId", "DateBorrowed", "ExpectedReturnDate", "Status")
    SELECT '91000000-0000-0000-0000-000000000001', -1, e."Id", CURRENT_DATE, CURRENT_DATE + 1, 0
    FROM public."Equipment" e WHERE e."Identifier" = 'EQ-003';
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Probe did not find its fixture';
    END IF;
    RAISE EXCEPTION 'Probe reached completion without the expected constraint rejection. Probe rolled back.';
END;
$$;

-- C4. Second active loan. Expect unique violation (23505) on the active-item index.
DO $$
BEGIN
    IF current_database() <> 'equipment_borrowing_lab4_test' THEN
        RAISE EXCEPTION 'This section requires equipment_borrowing_lab4_test';
    END IF;
    INSERT INTO public."Borrowings" ("Id", "StudentId", "EquipmentId", "DateBorrowed", "ExpectedReturnDate", "Status")
    SELECT '91000000-0000-0000-0000-000000000002', s."Id", e."Id", CURRENT_DATE, CURRENT_DATE + 1, 0
    FROM public."Students" s CROSS JOIN public."Equipment" e
    WHERE s."StudentNumber" = 'ST-001' AND e."Identifier" = 'EQ-004';
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Probe did not find its fixture';
    END IF;
    RAISE EXCEPTION 'Probe reached completion without the expected constraint rejection. Probe rolled back.';
END;
$$;

-- D. Compare fresh readbacks. Run separately before and after your scenario.
SELECT "Id", "StudentNumber", "Name", "Contact", "IsAllowedToBorrow"
FROM public."Students" ORDER BY "Id";
SELECT "Id", "Identifier", "Name", "Description", "IsAvailable"
FROM public."Equipment" ORDER BY "Id";
SELECT b."Id", b."StudentId", s."StudentNumber", b."EquipmentId", e."Identifier",
       b."DateBorrowed", b."ExpectedReturnDate", b."Status", b."ActualReturnDate", e."IsAvailable"
FROM public."Borrowings" b
JOIN public."Students" s ON s."Id" = b."StudentId"
JOIN public."Equipment" e ON e."Id" = b."EquipmentId"
ORDER BY b."DateBorrowed", b."Id";
SELECT "Id" FROM public."Borrowings" WHERE "Id" IN (
    '91000000-0000-0000-0000-000000000001', '91000000-0000-0000-0000-000000000002'
);
