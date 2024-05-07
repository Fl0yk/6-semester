CREATE TABLE Tab1 (
    Id NUMBER PRIMARY KEY,
    Name VARCHAR2(20),
    Val NUMBER
);

CREATE TABLE Tab2 (
    Id NUMBER PRIMARY KEY,
    Name VARCHAR2(20),
    Time TIMESTAMP
);

CREATE TABLE Tab3 (
    Id NUMBER PRIMARY KEY,
    Name VARCHAR2(20),
    Tab1_Id INT,
    FOREIGN KEY (Tab1_Id) REFERENCES TAb1(Id) ON DELETE CASCADE
);


-- Тесты

-- Вносим данные
SELECT * FROM Tab1;
SELECT * FROM Tab2;
SELECT * FROM Tab3;

DELETE FROM Tab1;

SELECT * FROM Tab3Log;
DELETE FROM Tab1Log;

INSERT INTO Tab1 VALUES(1, 'tab1 1', 10);
INSERT INTO Tab1 VALUES(2, 'tab1 2', 11);
INSERT INTO Tab1 VALUES(3, 'tab1 3', 12);

INSERT INTO Tab2 VALUES(1, 'tab2 1', CURRENT_TIMESTAMP);
INSERT INTO Tab2 VALUES(2, 'tab2 2', CURRENT_TIMESTAMP - INTERVAL '5' MINUTE);
INSERT INTO Tab2 VALUES(3, 'tab2 3', CURRENT_TIMESTAMP - INTERVAL '5' DAY);

INSERT INTO Tab3 VALUES(1, 'tab3 1', 1);
INSERT INTO Tab3 VALUES(2, 'tab3 2', 1);
INSERT INTO Tab3 VALUES(3, 'tab3 3', 1);


UPDATE Tab1 SET val = 20 WHERE Id = 2;

UPDATE Tab2 SET name = 'tab 2 new' WHERE Id = 2;

UPDATE Tab3 SET name = 'tab 3 new' WHERE Id = 2;


DELETE FROM Tab1 WHERE Id = 3;

DELETE FROM Tab2 WHERE Id = 1;

DELETE FROM Tab3 WHERE Id = 1;

DELETE FROM Tab1 WHERE Id = 1;

-- Откатываем данные
BEGIN
    recovery_package.Tabs_recovery(5);
    --recovery_package.Tabs_recovery(TO_TIMESTAMP('21.04.24 10:53:39'));
END;
/

-- Получаем отчет
BEGIN
    otchet_package.Tabs_report(TO_TIMESTAMP('21.04.24 10:53:39'));
    --otchet_package.Tabs_report();
END;
/



-- ТОЖЕ НИНАДА!!!

SELECT * FROM Tab2;
DELETE FROM Tab3;
DELETE FROM Lab3Log;

INSERT INTO Tab2 VALUES(1, 'tab2 1', CURRENT_TIMESTAMP);
INSERT INTO Tab2 VALUES(2, 'tab2 1', SYSTIMESTAMP);
INSERT INTO Tab2 VALUES(3, 'tab2 1', CURRENT_TIMESTAMP - INTERVAL 5 DAY);

UPDATE Tab2 SET name = 'tab 2 new' WHERE Id = 2;

DELETE FROM Tab2 WHERE Id = 1;

-- Откатываем данные
BEGIN
    recovery_package.Tab2_recovery(5);
    --recovery_package.Tab1_recovery(TO_TIMESTAMP('21.04.24 10:53:39'));
END;
/

-- Получаем отчет
BEGIN
    otchet_packege.Tab2_report(TO_TIMESTAMP('21.04.24 10:53:39'));
END;
/

SELECT * FROM Tab3;
DELETE FROM Tab3;
DELETE FROM Lab3Log;

INSERT INTO Tab3 VALUES(1, 'tab3 1', 1);
INSERT INTO Tab3 VALUES(2, 'tab3 2', 1);
INSERT INTO Tab3 VALUES(3, 'tab3 3', 1);

UPDATE Tab3 SET name = 'tab 3 new' WHERE Id = 2;

DELETE FROM Tab3 WHERE Id = 1;

-- Откатываем данные
BEGIN
    recovery_package.Tab2_recovery(5);
    --recovery_package.Tab1_recovery(TO_TIMESTAMP('21.04.24 10:53:39'));
END;
/

-- Получаем отчет
BEGIN
    otchet_packege.Tab2_report(TO_TIMESTAMP('21.04.24 10:53:39'));
END;
/