USE [master];

CREATE TABLE [User] (
    Id BIGINT PRIMARY KEY IDENTITY(1, 1), 
    Email VARCHAR(320), 
    [Name] VARCHAR(50),
    [Password] VARCHAR(256), 
    CreationDate DATETIME DEFAULT GETDATE(), 
    ModificationDate DATETIME DEFAULT GETDATE()
);
CREATE TABLE TokenType(
    Id BIGINT PRIMARY KEY IDENTITY(1, 1), 
    [Name] VARCHAR(20)
);
CREATE TABLE UserToken(
    Id BIGINT PRIMARY KEY IDENTITY(1, 1),
    UserId BIGINT NOT NULL, 
    TokenTypeId BIGINT NOT NULL,
    Token VARCHAR(256),
    ExpiryTime DATETIME,
    FOREIGN KEY (UserId) REFERENCES [User](Id),
    FOREIGN KEY (TokenTypeId) REFERENCES TokenType(Id)
);
CREATE TABLE Note (
    Id BIGINT PRIMARY KEY IDENTITY(1, 1), 
    UserId BIGINT NOT NULL, 
    Title VARCHAR(100), 
    Content TEXT, 
    CreationDate DATETIME DEFAULT GETDATE(), 
    ModificationDate DATETIME DEFAULT GETDATE(), 
    FOREIGN KEY (UserId) REFERENCES [User](Id)
);
CREATE TABLE Checklist (
    Id BIGINT PRIMARY KEY IDENTITY(1, 1), 
    UserId BIGINT NOT NULL, 
    Title VARCHAR(100), 
    CreationDate DATETIME DEFAULT GETDATE(), 
    ModificationDate DATETIME DEFAULT GETDATE(), 
    FOREIGN KEY (UserId) REFERENCES [User](Id)
);
CREATE TABLE ChecklistDetail (
    Id BIGINT PRIMARY KEY IDENTITY(1, 1), 
    ChecklistId BIGINT NOT NULL, 
    ParentDetailId BIGINT, 
    TaskName VARCHAR(100) NOT NULL, 
    Status BIT NOT NULL, 
    CreationDate DATETIME DEFAULT GETDATE(),
    ModificationDate DATETIME DEFAULT GETDATE(), 
    FOREIGN KEY (ChecklistId) REFERENCES Checklist(Id)
);
-- dotnet ef dbcontext scaffold "Server=(localdb)\mssqllocaldb;Database=ArchitectureTest;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -c DatabaseContext --context-dir Database/SQLServer -o Database/SQLServer/Entities --force

---- INSERTS
INSERT INTO [TokenType] ([Name]) VALUES ('RefreshToken')
INSERT INTO [User] (Email, [Name]) VALUES ('migue300995@gmail.com', 'Miguel Angel'); -- SignUp user in api

INSERT INTO Note (UserId, Title, Content) VALUES (1, 'Note 1', ''), (1, 'Note 2', ''), (1, 'Note 3', ''), (1, 'Note 4', ''), (1, 'Note 5', ''), (1, 'Note 16', ''), (1, 'Note 7', '');
INSERT INTO Checklist(UserId, Title) VALUES (1, 'Checklist 1')
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (1, NULL, 'Limpiar cuarto', 0), (1, NULL, 'Limpiar oficina', 0), (1, NULL, 'Bañar perro', 0), (1, NULL, 'Hacer tarea', 0)
INSERT INTO Checklist(UserId, Title) VALUES (1, 'Animales')
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, NULL, 'Vertebrados', 0), (2, NULL, 'Invertebrados', 0)
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, 5, 'Mamiferos', 0), (2, 5, 'Peces', 0), (2, 5, 'Aves', 0), (2, 5, 'Reptiles', 0), (2, 5, 'Anfibios', 0)
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, 7, 'Leon', 0), (2, 7, 'Perro', 0), (2, 7, 'Ballena', 0)
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, 8, 'Pez Ballena', 0)
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, 9, 'Aguila', 0), (2, 9, 'Cito', 0)
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, 10, 'Serpiente', 0), (2, 10, 'Cocodrilo', 0), (2, 10, 'Tortuga', 0)
INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, 11, 'Rana', 0)

INSERT INTO ChecklistDetail(ChecklistId, ParentDetailId, TaskName, Status) VALUES (2, 6, 'Gusanos', 0), (2, 6, 'Esponjas', 0), (2, 6, 'Estrellas de mar', 0)
