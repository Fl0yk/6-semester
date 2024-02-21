
--Task 1--
CREATE TABLE Students(
    id NUMBER NOT NULL,
    name VARCHAR2(30) NOT NULL,
    group_id NUMBER NOT NULL);

CREATE TABLE Groups(
    id NUMBER NOT NULL,
    name VARCHAR2(40) NOT NULL,
    c_val NUMBER NOT NULL);

--Task 2--
CREATE SEQUENCE student_id_seq
START WITH 1
INCREMENT BY 1;

--Если в этот триггер зашло с id, то его не меняем, а отправляем дальше
CREATE OR REPLACE TRIGGER generete_student_id
 BEFORE INSERT ON Students
 FOR EACH ROW
BEGIN
    IF :new.id IS NULL THEN
        :new.id := student_id_seq.nextval;
    END IF;
END;


CREATE SEQUENCE group_id_seq
START WITH 1
INCREMENT BY 1;

CREATE OR REPLACE TRIGGER generete_group_id
 BEFORE INSERT ON Groups
 FOR EACH ROW
BEGIN
    IF :new.id IS NULL THEN
        :new.id := group_id_seq.nextval;
    END IF;
END;


--Task 3--
CREATE OR REPLACE TRIGGER fk_group_delete_cascade
 AFTER DELETE ON Groups
 FOR EACH ROW
BEGIN
    UPDATE cascade SET is_cascade = TRUE WHERE id = 1;
    DELETE FROM Students
    WHERE group_id = :OLD.id;
    UPDATE cascade SET is_cascade = FALSE WHERE id = 1;
END;

INSERT INTO Groups(name, c_val) VALUES ('153505', 0);
INSERT INTO Students(name, group_id) VALUES ('A', 3);
INSERT INTO Students(name, group_id) VALUES ('B', 3);

SELECT * FROM Students
LEFT JOIN Groups ON students.group_id = groups.id;

--Task 4--
CREATE TABLE student_journal (
    id NUMBER PRIMARY KEY,
    operation VARCHAR2(10),
    op_time TIMESTAMP,
    s_id NUMBER,
    s_name VARCHAR2(30),
    s_group_id NUMBER,
    n_s_id NUMBER,
    n_s_name VARCHAR2(30),
    n_s_group_id NUMBER
);

CREATE SEQUENCE stud_journal_id_seq
START WITH 1
INCREMENT BY 1;

CREATE OR REPLACE TRIGGER students_journal
 AFTER INSERT OR UPDATE OR DELETE ON Students
 FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO student_journal VALUES (stud_journal_id_seq.nextval, 'INSERT', CURRENT_TIMESTAMP, :NEW.id, :NEW.name, :NEW.group_id, NULL, NULL, NULL);
    END IF;

    IF UPDATING THEN
        INSERT INTO student_journal VALUES (stud_journal_id_seq.nextval, 'UPDATE', CURRENT_TIMESTAMP, :OLD.id, :OLD.name, :OLD.group_id, :NEW.id, :NEW.name, :NEW.group_id);
    END IF;
        
    IF DELETING THEN
        INSERT INTO student_journal VALUES (stud_journal_id_seq.nextval, 'DELETE', CURRENT_TIMESTAMP, :OLD.id, :OLD.name, :OLD.group_id, NULL, NULL, NULL);
    END IF;
END;

--Task 5--
CREATE OR REPLACE PROCEDURE roll_back_students(r_time TIMESTAMP) IS
BEGIN
    FOR action IN (SELECT * FROM student_journal WHERE r_time < op_time ORDER BY id DESC)
    LOOP
        IF action.operation = 'INSERT' THEN
            DELETE Students WHERE id = action.s_id;
        END IF;

        IF action.operation = 'UPDATE' THEN
            UPDATE Students SET
             id = action.s_id,
             name = action.s_name,
             group_id = action.s_group_id
             WHERE id = action.n_s_id;
        END IF;

        IF action.operation = 'DELETE' THEN
            INSERT INTO Students VALUES (action.s_id, action.s_name, action.s_group_id);
        END IF;
    END LOOP;
END;

--Test tasks 4-5
INSERT INTO Students(name, group_id) VALUES ('T1', 1);
INSERT INTO Students(name, group_id) VALUES ('T2', 1);
UPDATE Students SET name = 'T_n' WHERE name = 'T1';
DELETE Students WHERE name = 'T1';
SELECT * FROM Students;

DELETE student_journal;
SELECT * FROM student_journal;

EXEC roll_back_students(TO_TIMESTAMP(CURRENT_TIMESTAMP - 50));

--Task 6--

CREATE TABLE cascade(id NUMBER, is_cascade BOOLEAN);
INSERT INTO cascade VALUES (1, FALSE);

CREATE OR REPLACE TRIGGER c_val_trigger
 AFTER INSERT OR UPDATE OR DELETE ON Students
 FOR EACH ROW
 DECLARE
    v_is_cascade BOOLEAN;
BEGIN 
    IF UPDATING THEN
        UPDATE Groups
        SET c_val = c_val + 1
        WHERE id = :NEW.group_id;

        UPDATE Groups
        SET c_val = c_val - 1
        WHERE id = :OLD.group_id;
    END IF;

    IF INSERTING THEN
        UPDATE Groups
        SET c_val = c_val + 1
        WHERE id = :NEW.group_id;
    END IF;

    IF DELETING THEN
        SELECT is_cascade INTO v_is_cascade FROM cascade WHERE id = 1;
        IF v_is_cascade THEN
            RETURN;
        END IF;

        UPDATE Groups
        SET c_val = c_val - 1
        WHERE id = :OLD.group_id;
    END IF;
END;

INSERT INTO Groups(name, c_val) VALUES ('155004', 0);
SELECT * FROM Groups;

INSERT INTO Students(name, group_id) VALUES ('Vova', 4);
INSERT INTO Students(name, group_id) VALUES ('Vlad', 4);
INSERT INTO Students(name, group_id) VALUES ('Dima', 4);

DELETE Students WHERE name = 'Dima';

--Last tests
INSERT INTO Groups(id, name, c_val) VALUES (1, '153501', 0);
INSERT INTO Groups(id, name, c_val) VALUES (2, '153502', 0);

SELECT * FROM Groups;

INSERT INTO Students(name, group_id) VALUES ('Bob', 1);
INSERT INTO Students(name, group_id) VALUES ('Bib', 1);
INSERT INTO Students(name, group_id) VALUES ('Vova', 2);
INSERT INTO Students(id, name, group_id) VALUES (5, 'Miha', 2);

--Проверяем изменение c_val
SELECT * FROM Students
LEFT JOIN Groups ON students.group_id = groups.id;

--Проверяем изменения таблиц
UPDATE Students SET name = 'Dima' WHERE id = 5;

--Ошибки
UPDATE Groups SET id = 1 WHERE id = 2;
UPDATE Groups SET name = '153501' WHERE id = 2;

--Проверяем каскадное удаление
DELETE FROM Groups WHERE id = 2;

SELECT * FROM student_journal;
DELETE FROM student_journal;

EXEC roll_back_students(TO_TIMESTAMP(CURRENT_TIMESTAMP - 50));