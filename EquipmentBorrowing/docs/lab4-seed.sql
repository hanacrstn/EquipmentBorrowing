INSERT INTO "Students" ("StudentNumber", "Name", "Contact", "IsAllowedToBorrow")
VALUES ('ST-001', 'Alex Reyes', NULL, true), ('ST-002', 'Sam Cruz', NULL, true),
       ('ST-003', 'Casey Tan', NULL, false), ('ST-004', 'Jordan Lim', NULL, true)
ON CONFLICT ("NormalizedNumber") DO NOTHING;

INSERT INTO "Equipment" ("Identifier", "Name", "Description", "IsAvailable")
VALUES ('EQ-001', 'Camera', NULL, true), ('EQ-002', 'Tripod', NULL, true),
       ('EQ-003', 'Projector', NULL, true), ('EQ-004', 'Microphone', NULL, true),
       ('EQ-005', 'Light stand', NULL, true)
ON CONFLICT ("NormalizedIdentifier") DO NOTHING;
