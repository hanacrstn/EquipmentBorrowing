### DESKTOP APPLICATION DEVELOPMENT

#### LABORATORY ACTIVTY 1



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





















