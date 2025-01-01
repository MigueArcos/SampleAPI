USE crud;

CREATE TABLE User (
    Id VARCHAR(32) PRIMARY KEY, 
    Email VARCHAR(320),
    Name VARCHAR(50),
    Password VARCHAR(256),
    CreationDate DATETIME NOT NULL DEFAULT NOW(), 
    ModificationDate DATETIME
);
CREATE TABLE TokenType(
    Id VARCHAR(32) PRIMARY KEY,
    CreationDate DATETIME NOT NULL DEFAULT NOW(), 
    Name VARCHAR(20)
);
CREATE TABLE UserToken(
    Id VARCHAR(32) PRIMARY KEY,
    UserId VARCHAR(32) NOT NULL, 
    TokenTypeId VARCHAR(32) NOT NULL,
    Token VARCHAR(256),
    CreationDate DATETIME NOT NULL DEFAULT NOW(), 
    ExpiryTime DATETIME,
    FOREIGN KEY (UserId) REFERENCES User(Id),
    FOREIGN KEY (TokenTypeId) REFERENCES TokenType(Id)
);
CREATE TABLE Note (
    Id VARCHAR(32) PRIMARY KEY, 
    UserId VARCHAR(32) NOT NULL, 
    Title VARCHAR(100), 
    Content TEXT, 
    CreationDate DATETIME NOT NULL DEFAULT NOW(), 
    ModificationDate DATETIME, 
    FOREIGN KEY (UserId) REFERENCES User(Id)
);
CREATE TABLE Checklist (
    Id VARCHAR(32) PRIMARY KEY, 
    UserId VARCHAR(32) NOT NULL, 
    Title VARCHAR(100), 
    CreationDate DATETIME NOT NULL DEFAULT NOW(), 
    ModificationDate DATETIME, 
    FOREIGN KEY (UserId) REFERENCES User(Id)
);
CREATE TABLE ChecklistDetail (
    Id VARCHAR(32) PRIMARY KEY, 
    ChecklistId VARCHAR(32) NOT NULL,
    ParentDetailId VARCHAR(32), 
    TaskName VARCHAR(100) NOT NULL, 
    Status TINYINT(1) NOT NULL, 
    CreationDate DATETIME NOT NULL DEFAULT NOW(), 
    ModificationDate DATETIME, 
    FOREIGN KEY (ChecklistId) REFERENCES Checklist(Id)
);

-- INSERTS
INSERT INTO TokenType (Id, Name) VALUES ('1', 'RefreshToken');
-- zeusensacion is the password for migue300995@gmail.com
INSERT INTO User (Id, Email, Name, Password) VALUES ('1', 'migue300995@gmail.com', 'Miguel Angel', '10000.9f71G4vvx5BdkPshEU3SSw==.a8ma/2AEyDCDRAGyaznoTRUICR6zvwMS4U1mQGi+nok=');

INSERT INTO Note
    (Id, UserId, Title, Content) 
VALUES 
    ('1', '1', 'Note 1', ''),
    ('2', '1', 'Note 2', ''),
    ('3', '1', 'Note 3', ''),
    ('4', '1', 'Note 4', ''),
    ('5', '1', 'Note 5', ''),
    ('6', '1', 'Note 6', ''),
    ('7', '1', 'Note 7', '');

INSERT INTO Checklist(Id, UserId, Title) VALUES ('1', '1', 'Checklist 1');

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status) 
VALUES 
    ('1', '1', NULL, 'Limpiar cuarto', 0), -- 1
    ('2', '1', NULL, 'Limpiar oficina', 0), -- 2
    ('3', '1', NULL, 'Bañar perro', 0), -- 3
    ('4', '1', NULL, 'Hacer tarea', 0); -- 4

INSERT INTO Checklist(Id, UserId, Title) VALUES ('2', '1', 'Animales');

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status) 
VALUES 
    ('5', '2', NULL, 'Vertebrados', 0), -- 5
    ('6', '2', NULL, 'Invertebrados', 0); -- 6

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status) 
VALUES 
    ('7', '2', '5', 'Mamiferos', 0), -- 7
    ('8', '2', '5', 'Peces', 0), -- 8
    ('9', '2', '5', 'Aves', 0), -- 9
    ('10', '2', '5', 'Reptiles', 0), -- 10
    ('11', '2', '5', 'Anfibios', 0); -- 11

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    ('12', '2', '7', 'Leon', 0), -- 12
    ('13', '2', '7', 'Perro', 0), -- 13
    ('14', '2', '7', 'Ballena', 0); -- 14

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    ('15', '2', '8', 'Pez Ballena', 0); -- 15

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    ('16', '2', '9', 'Aguila', 0), -- 16
    ('17', '2', '9', 'Cito', 0); -- 17

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    ('18', '2', '10', 'Serpiente', 0), -- 18 
    ('19', '2', '10', 'Cocodrilo', 0), -- 19
    ('20', '2', '10', 'Tortuga', 0); -- 20

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status)
VALUES ('21', '2', '11', 'Rana', 0); -- 21

INSERT INTO ChecklistDetail
    (Id, ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    ('22', '2', '6', 'Gusanos', 0), -- 22
    ('23', '2', '6', 'Esponjas', 0), -- 23
    ('24', '2', '6', 'Estrellas de mar', 0); -- 24

-- dotnet ef dbcontext scaffold "Server=localhost; Database=crud; Uid=self; Pwd=P455w0rd;TreatTinyAsBoolean=true" Pomelo.EntityFrameworkCore.MySql -c DatabaseContext --context-dir MySql -o MySql/Entities --force
