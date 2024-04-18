CREATE TABLE Products (
    Id INT NOT NULL, 
    Name VARCHAR2(100),
    Price INT,
    PRIMARY KEY (Id)
)


INSERT INTO Products (Id, Name, Price) VALUES
(1, 'Prod 1', 100);
INSERT INTO Products (Id, Name, Price) VALUES
(2, 'Prod 2', 120);
INSERT INTO Products (Id, Name, Price) VALUES
(3, 'Prod 3', 70);
INSERT INTO Products (Id, Name, Price) VALUES
(4, 'Prod 4', 90);

CREATE TABLE Orders (
    Id INT NOT NULL, 
    ProductId INT, 
    PRIMARY KEY (Id), 
    FOREIGN KEY (ProductId) REFERENCES Products (Id)
)

INSERT INTO Orders (Id, ProductId) VALUES
(1, 2);
INSERT INTO Orders (Id, ProductId) VALUES
(2, 4);