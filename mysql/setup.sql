USE crud;

CREATE TABLE User (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT, 
    Email VARCHAR(320),
    Name VARCHAR(50),
    Password VARCHAR(256),
    CreationDate DATETIME DEFAULT NOW(), 
    ModificationDate DATETIME DEFAULT NOW()
);
CREATE TABLE TokenType(
    Id BIGINT PRIMARY KEY AUTO_INCREMENT, 
    Name VARCHAR(20)
);
CREATE TABLE UserToken(
    Id BIGINT PRIMARY KEY AUTO_INCREMENT,
    UserId BIGINT NOT NULL, 
    TokenTypeId BIGINT NOT NULL,
    Token VARCHAR(256),
    ExpiryTime DATETIME,
    FOREIGN KEY (UserId) REFERENCES User(Id),
    FOREIGN KEY (TokenTypeId) REFERENCES TokenType(Id)
);
CREATE TABLE Note (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT, 
    UserId BIGINT NOT NULL, 
    Title VARCHAR(100), 
    Content TEXT, 
    CreationDate DATETIME DEFAULT NOW(), 
    ModificationDate DATETIME DEFAULT NOW(), 
    FOREIGN KEY (UserId) REFERENCES User(Id)
);
CREATE TABLE Checklist (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT, 
    UserId BIGINT NOT NULL, 
    Title VARCHAR(100), 
    CreationDate DATETIME DEFAULT NOW(), 
    ModificationDate DATETIME DEFAULT NOW(), 
    FOREIGN KEY (UserId) REFERENCES User(Id)
);
CREATE TABLE ChecklistDetail (
    Id BIGINT PRIMARY KEY AUTO_INCREMENT, 
    ChecklistId BIGINT NOT NULL, 
    ParentDetailId BIGINT, 
    TaskName VARCHAR(100) NOT NULL, 
    Status TINYINT(1) NOT NULL, 
    CreationDate DATETIME DEFAULT NOW(), 
    ModificationDate DATETIME DEFAULT NOW(), 
    FOREIGN KEY (ChecklistId) REFERENCES Checklist(Id)
);

-- INSERTS
INSERT INTO TokenType (Name) VALUES ('RefreshToken');
-- zeusensacion is the password for migue300995@gmail.com
INSERT INTO User (Email, Name, Password) VALUES ('migue300995@gmail.com', 'Miguel Angel', '10000.9f71G4vvx5BdkPshEU3SSw==.a8ma/2AEyDCDRAGyaznoTRUICR6zvwMS4U1mQGi+nok=');

INSERT INTO Note
    (UserId, Title, Content) 
VALUES 
    (1, 'Note 1', ''),
    (1, 'Note 2', ''),
    (1, 'Note 3', ''),
    (1, 'Note 4', ''),
    (1, 'Note 5', ''),
    (1, 'Note 6', ''),
    (1, 'Note 7', '');

INSERT INTO Checklist(UserId, Title) VALUES (1, 'Checklist 1');

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status) 
VALUES 
    (1, NULL, 'Limpiar cuarto', 0), -- 1
    (1, NULL, 'Limpiar oficina', 0), -- 2
    (1, NULL, 'Bañar perro', 0), -- 3
    (1, NULL, 'Hacer tarea', 0); -- 4

INSERT INTO Checklist(UserId, Title) VALUES (1, 'Animales');

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status) 
VALUES 
    (2, NULL, 'Vertebrados', 0), -- 5
    (2, NULL, 'Invertebrados', 0); -- 6

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status) 
VALUES 
    (2, 5, 'Mamiferos', 0), -- 7
    (2, 5, 'Peces', 0), -- 8
    (2, 5, 'Aves', 0), -- 9
    (2, 5, 'Reptiles', 0), -- 10
    (2, 5, 'Anfibios', 0); -- 11

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    (2, 7, 'Leon', 0), -- 12
    (2, 7, 'Perro', 0), -- 13
    (2, 7, 'Ballena', 0); -- 14

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    (2, 8, 'Pez Ballena', 0); -- 15

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    (2, 9, 'Aguila', 0), -- 16
    (2, 9, 'Cito', 0); -- 17

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    (2, 10, 'Serpiente', 0), -- 18 
    (2, 10, 'Cocodrilo', 0), -- 19
    (2, 10, 'Tortuga', 0); -- 20

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status)
VALUES (2, 11, 'Rana', 0); -- 21

INSERT INTO ChecklistDetail
    (ChecklistId, ParentDetailId, TaskName, Status)
VALUES
    (2, 6, 'Gusanos', 0), -- 22
    (2, 6, 'Esponjas', 0), -- 23
    (2, 6, 'Estrellas de mar', 0); -- 24

-- dotnet ef dbcontext scaffold "Server=localhost; Database=crud; Uid=self; Pwd=P455w0rd;TreatTinyAsBoolean=true" Pomelo.EntityFrameworkCore.MySql -c DatabaseContext --context-dir MySql -o MySql/Entities --force
