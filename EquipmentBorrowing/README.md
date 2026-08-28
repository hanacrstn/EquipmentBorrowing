# Equipment Borrowing System — Architecture Overview

## 1. Solution Structure
- **Domain** – Core business concepts (Student, Equipment, Borrowing, BorrowingStatus) and the
  rules that belong to a single concept (e.g. equipment cannot be marked borrowed twice).
- **Application** – Use cases/orchestration (e.g. BorrowEquipmentService) and the repository
  interfaces (contracts) the use cases depend on.
- **Infrastructure** – Concrete, swappable implementations of those contracts. Currently
  in-memory; could become SQLite/PostgreSQL/file-based later without changing Domain or
  Application.
- **Tests** – Automated verification of domain rules and application service behavior.

**Development approach:** this solution was built domain-by-domain — Student was completed
end-to-end (domain class, repository interface, in-memory repository) before Equipment was
started, and Equipment was completed end-to-end before Borrowing. BorrowEquipmentService was
implemented last, as the point where all three domain concepts come together, rather than
building every layer across all concepts before any of them worked.

## 2. Dependency Direction

    ConsoleDemo (composition root / future Avalonia UI)
              |
              v
         Application
           |      ^
           v      |
         Domain   |
                  |
         Infrastructure

Domain depends on nothing. Application depends only on Domain. Infrastructure depends on
Domain and Application (it implements Application's interfaces). The composition root
(ConsoleDemo today, later the UI) is the only place that knows about Infrastructure directly.

## 3. Use Case Mapping

Actor: Student
Use Case: Borrow Equipment
Application Service: BorrowEquipmentService
Domain Objects Used: Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces Used: IStudentRepository, IEquipmentRepository, IBorrowingRepository
Infrastructure Implementations Used: InMemoryStudentRepository, InMemoryEquipmentRepository,
InMemoryBorrowingRepository

## 4. Reflection

1. **Why depend on a repository interface instead of a database implementation directly?**
   Because the service's job is business logic, not data access technology. Depending on an
   interface means the service can be tested with fakes, and the storage technology can change
   without touching a single line of the service.

2. **Which parts could remain unchanged if SQLite were added later?**
   Domain and Application in their entirety — only Infrastructure would gain a new
   SqliteEquipmentRepository (etc.), and the composition root would swap which concrete class
   it hands to the service constructor.

3. **Which project would eventually contain Avalonia Views?**
   A new UI project (e.g. EquipmentBorrowing.Desktop) that references Application (to call
   services) — it would replace/extend today's ConsoleDemo as the composition root.

4. **Should an Avalonia button directly execute database queries? Why or why not?**
   No. That would collapse all the layering above: the UI would become coupled to a specific
   storage technology and business rules would leak into UI event handlers, making the rules
   untestable and unreusable from any other entry point (e.g. a future API).

5. **What represents the actual business operation requested by the actor?**
   BorrowEquipmentService.ExecuteAsync — everything else (Domain, repositories) exists only to
   support that one orchestrated operation.

   -----

   PRE-DEVELOPMENT ANALYSIS

##### A. ACTORS

\-- Students (Primary)

&#x09;- The student expects the system to allow them to view available equipment, submit borrowing requests, track their active borrowings, and successfully return equipment.


\-- Staff / Administrator (Assumption)

&#x09;- To trust that the system enforces borrowing rules automatically (eligibility, availability, limits) without manual double-checking, and to see accurate equipment status.


##### B. USE CASES

###### UC-01: Borrow Equipment

|ITEM|DESCRIPTION|
|-|-|
|Use-Case|Borrow equipment|
|Primary Actor|Student|
|Pre-conditions|Student is registered and currently allowed to borrow; equipment exists|
|Main Action|Student requests to borrow a specific piece of equipment|
|Expected Result|A new `Borrowing` record is created with status `Active`; equipment becomes unavailable|
|Possible Failure|Equipment does not exist, equipment is unavailable, student is not allowed to borrow, or student has reached the maximum active borrowings|


###### UC-02: Return Equipment

|ITEM|DESCRIPTION|
|-|-|
|Use-Case|Return equipment|
|Primary Actor|Student|
|Pre-conditions|An active `Borrowing` exists for this student and equipment|
|Main Action|Student returns previously borrowed equipment|
|Expected Result|Borrowing status changes to `Returned`; equipment becomes available again|
|Possible Failure|No matching active borrowing found; equipment already marked returned|


###### UC-03: Find Available Equipment

|ITEM|DESCRIPTION|
|-|-|
|Use-Case|Find Available Equipment|
|Primary Actor|Student|
|Pre-conditions|None (read-only inquiry)|
|Main Action|Student requests a list of equipment currently available for borrowing|
|Expected Result|System returns the set of equipment whose status is available|
|Possible Failure|No equipment currently available (empty result)|


##### C. DOMAIN CONCEPTS

###### **Student**

1. **Must contain:** an identifier, a name, and something indicating current eligibility to borrow (e.g., `IsAllowedToBorrow` flag, or a status/standing).
2. **Rules/state it owns:** whether it is currently in good standing (this is a fact about the student, so it can live here as a simple flag/property).
3. **Should not be responsible for:** deciding whether a specific borrowing request should be approved (that requires knowledge of equipment availability and current borrowing count, that's a cross-object decision, which belongs in the ***Application Service,*** not inside `Student` itself).

###### **Equipment**

1. **Must contain:** an identifier, a name/description, and its current availability status.
2. **Rules/state it owns:** its own availability (`IsAvailable`), and the behavior to flip that state (`MarkAsBorrowed()` / `MarkAsAvailable()`) since availability is intrinsic to the equipment itself.
3. **Should not be responsible for:** knowing who borrowed it or *when* — that relationship belongs to `Borrowing`, not to `Equipment`.

###### **Borrowing**

1. **Must contain:** a reference to the student, a reference to the equipment, date borrowed, expected return date, and current status (`BorrowingStatus`).
2. **Rules/state it owns:** its own lifecycle — e.g., a `MarkReturned()` method that can only transition `Active → Returned` (not the reverse).
3. **Should not be responsible for:** checking whether the student was allowed to borrow in the first place, or updating the equipment's availability flag on its own initiative without being told to (that orchestration is the ***Application Service***<i>'s</i> job, because it needs to touch *two* objects — Equipment and Borrowing — which a single domain object shouldn't reach out and mutate on its own).
