-- создаем схемы
CREATE USER C##prod_schema IDENTIFIED BY prod;

-- инициализируем prod схему
CREATE TABLE C##prod_schema.common_table (
    id INT PRIMARY KEY,
    name VARCHAR(50)
);

CREATE TABLE C##prod_schema.foreign_table (
    id INT PRIMARY KEY,
    common_id INT,
    FOREIGN KEY (common_id) REFERENCES C##prod_schema.common_table(id)
);

CREATE TABLE C##prod_schema.diff_table (
    id INT PRIMARY KEY,
    name VARCHAR(50)
);

CREATE TABLE C##prod_schema.circle1 (
    id INT PRIMARY KEY,
    circle2_id INT
);

CREATE TABLE C##prod_schema.circle2 (
    id INT PRIMARY KEY,
    circle1_id INT,
    FOREIGN KEY (circle1_id) REFERENCES C##prod_schema.circle1(id)
);

ALTER TABLE C##prod_schema.circle1
ADD CONSTRAINT fk_circle2_id
FOREIGN KEY (circle2_id) REFERENCES C##prod_schema.circle2(id);

CREATE OR REPLACE PROCEDURE C##prod_schema.my_procedure AS
BEGIN
  NULL;
END;
/

CREATE OR REPLACE FUNCTION C##prod_schema.my_function
RETURN NUMBER AS
BEGIN
  RETURN 2;
END;
/