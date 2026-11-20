# Appendix C verification

Use this runbook in the order below. **Fill in the observed results from your own run.** Record the tested Git commit, time, database, scenario setup, action, expected result, observed result, and evidence filename. Use disposable test data throughout. Manual testing is acceptable.

## 1. Set up the disposable database

1. In pgAdmin, create an empty database named `equipment_borrowing_lab4_test` with `lab4_app` as owner.
2. In PowerShell, run the README connection block with the test database name. Confirm the database name without printing your password:

   ```powershell
   $target = [System.Data.Common.DbConnectionStringBuilder]::new()
   $target.ConnectionString = $env:EQUIPMENT_BORROWING_DB
   $target['Database']
   ```

3. Run the README migration command. Save the terminal output and run the **Schema and migrations** section of [lab4-verification.sql](lab4-verification.sql) in a fresh pgAdmin Query Tool.
4. Before seeding, capture the empty **Catalog** and **Active borrowings** views. Then run [lab4-seed.sql](lab4-seed.sql) on the test database and refresh the app. Use the supplied fixture IDs where possible.
5. For each persistence check, close the app, relaunch from the same test-configured PowerShell, and use a fresh pgAdmin Query Tool for readback. Match internal IDs as well as visible names.

## 2. Run the integrated demonstration

1. **Students and equipment (A12, A13, A15, A16).** Register a student and item. Reopen them, edit permitted fields, and clear optional values. Record stable internal IDs. After borrowing, edit again and verify history still points to the same IDs. Test required-field and case/space duplicate rejections separately.
2. **Catalog and one-item loan (A01–A04).** Compare the full catalog with the equipment query. Issue one item to an eligible student, restart, and confirm the single active loan and unavailable item. Compare the **Available items only** view and active list with SQL. Test missing selection, invalid date, a restricted student, unavailable equipment, and the active limit.
3. **Eligibility and return (A05, A14).** On **Students**, select an active borrower, set **Restricted**, and click **Update selected student**. Verify a new issue is rejected and the existing loan stays active. Return the loan using the confirmation checkbox. Check the same ID, actual return date, and restored availability after restart. Try a repeat or stale return. Set **Eligible** and verify checkout works again. Repeat the current eligibility and record the actual feedback. The current UI uses a general save confirmation for that case.
4. **Batch (A06).** Give a student with one active loan two available items, reaching exactly three. After restart verify both batch rows and availability. Separately try duplicate selections, fewer than two items, unavailable or stale/missing equipment, a restricted borrower, an invalid date, and an over-limit combination. Compare fresh reads around each rejection.
5. **Histories and search (A07–A09).** Use two students and two items with distinguishable histories and one each with none. Compare student and equipment histories to the same database event. Exercise stale selection and failed loads, separating those from empty history. Search by student name, institutional number, and item name. Test every status, each date sort in both directions, combined criteria, clear/reset, and no matches. Explain the asynchronous query path from ViewModel to EF Core.
6. **Constraint and recovery checks (A17–A19).** Run the database constraint probes below, record connection failure and recovery, and review the use-case diagram, suite, traceability, and README against the actual app.

## 3. Prove atomic rollback (A06)

1. Register eligible student `QA-ROLLBACK` with zero active loans. Make sure `EQ-004` and `EQ-005` are both available. If necessary, return existing test loans first. Run **B1** from the SQL file and save the before-state.
2. Run **B2** as one block on the disposable database. This installs a temporary BEFORE INSERT trigger. When the second insert is attempted, the trigger checks that the first active borrowing is already visible inside the transaction. It advances a witness sequence and then raises a known error. A PostgreSQL sequence increment survives transaction rollback, so the witness can be read independently.
3. In **Checkout**, choose `QA-ROLLBACK`, both items and a valid expected date. Click **Check out selected batch** once. Capture the error message.
4. Open a fresh Query Tool on the same disposable database. Run **B1** again and **B3**. The expected readback is zero lending rows for `QA-ROLLBACK`, both items still available, and a witness reading of `last_value = 1` with `is_called = true`. Record the actual results. If the witness was never called, this run does not prove an insert was attempted.
5. Run **B4** to remove the trigger, function and sequence. Retry with the same inputs and verify that both loans and availability changes are now committed. Preserve the failed and successful runs as separate evidence.

## 4. Check database integrity and recovery (A17–A19)

- **A17.** Save migration history, schema constraints and index definitions. With `ST-001` and `EQ-001` present, use **C1–C4** to show normalized duplicates, a missing foreign key, and a second active loan rejected by PostgreSQL. Leave an active loan for `EQ-004` and ensure `EQ-003` exists and has no active loan for the two borrowing probes. Confirm the probe rows are absent via a fresh query.
- **A18.** On the disposable database, simulate connection loss before operations. For example, open a second launcher configured to an unused local port and try a read and write. Capture the red failure message, then use the correctly configured launcher, refresh and show SQL readback and a successful retry after checking state. For the source walkthrough, show the advisory lock, context lifetime, transaction, and unknown-commit feedback in `Lab4Repository.cs` plus the refresh/retry handling in `Lab4ViewModel.cs`. Record an acknowledgement-loss simulation only if it was actually performed. A known pre-write or trigger failure is a separate case.
- **A19.** Follow the README from a fresh setup, verify the diagram and catalog match the app, and link each of the fourteen cases to its evidence. Include the tested Git commit and history in the submission.

## Record your results

**Git commit:** Pending  
**Tester and date:** Pending  
**Database:** `equipment_borrowing_lab4_test`

Use separate rows for multiple checks under an acceptance ID. For each row, record fixture IDs and the action so the result can be reproduced.

| ID | Setup and action | Expected result | Observed result | Evidence | Status |
|---|---|---|---|---|---|
| A01 | Full and empty catalog plus failed load | Matches database, with distinct empty and error states | Pending | Pending | Pending |
| A02 | Available-only, no match, and failed load | Filters unavailable items, with distinct empty and error states | Pending | Pending | Pending |
| A03 | Single issue and individual rejections | One durable active loan on success and no write on rejection | Pending | Pending | Pending |
| A04 | Active list with a returned loan, empty and failed loads | Active rows only, correct student and equipment | Pending | Pending | Pending |
| A05 | Return, stale and repeated return | Same loan ID with return date, no duplicate change | Pending | Pending | Pending |
| A06 | Batch, exact limit, invalid inputs and forced failure | All successful rows commit; rejection and controlled rollback preserve state | Pending | Pending | Pending |
| A07 | Student history, empty, stale and failed load | Correct related active/returned rows and distinct feedback | Pending | Pending | Pending |
| A08 | Equipment history, empty, stale and failed load | Correct related active/returned rows and distinct feedback | Pending | Pending | Pending |
| A09 | Search fields, status, sort, reset and failure | Correct membership and order, distinct no matches/error | Pending | Pending | Pending |
| A12 | Valid, blank and duplicate registration | One eligible durable student | Pending | Pending | Pending |
| A13 | Student edit, clear contact and invalid edits | Same identity and history after restart | Pending | Pending | Pending |
| A14 | Restrict, return, permit and repeat state | Restricted issue rejected; return and subsequent eligible issue work | Pending | Pending | Pending |
| A15 | Valid, blank and duplicate equipment registration | One durable available item on success | Pending | Pending | Pending |
| A16 | Item edit, clear description and invalid edit | Same identity, history and availability | Pending | Pending | Pending |
| A17 | Clean migration and database-only rejection probes | Expected schema and enforced constraints | Pending | Pending | Pending |
| A18 | Known failure and recovery, source walkthrough | No false success or partial changes, working refresh and retry | Pending | Pending | Pending |
| A19 | README and traceability review | Instructions and documentation match the tested commit | Pending | Pending | Pending |
