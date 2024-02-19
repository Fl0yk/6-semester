
--Task 1--
CREATE TABLE Students(
    id NUMBER PRIMARY KEY,
    name VARCHAR2(30) NOT NULL,
    group_id NUMBER);

CREATE TABLE Groups(
    id NUMBER PRIMARY KEY,
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

--Этот триггер уже будет проверять уникальность id
CREATE OR REPLACE TRIGGER check_student_id
 BEFORE INSERT OR UPDATE ON Students
 FOR EACH ROW
DECLARE
    v_id_count NUMBER;
    id_exist_ex EXCEPTION;
BEGIN
    IF UPDATING AND :OLD.id = :NEW.id THEN
        RAISE NO_DATA_FOUND;
    END IF;

    SELECT COUNT(1) INTO v_id_count FROM Students WHERE id = :new.id;

    IF v_id_count <> 0 THEN
        dbms_output.put_line('This student id exists ' || :new.id);
        RAISE id_exist_ex;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        null;
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

CREATE OR REPLACE TRIGGER check_group_id
 BEFORE INSERT OR UPDATE ON Groups
 FOR EACH ROW
DECLARE
    v_id_count NUMBER;
    id_exist_ex EXCEPTION;
BEGIN
    IF UPDATING AND :OLD.id = :NEW.id THEN
        RAISE NO_DATA_FOUND;
    END IF;

    SELECT COUNT(1) INTO v_id_count FROM Groups WHERE id = :new.id;
    IF v_id_count <> 0 THEN
        dbms_output.put_line('This group id exists ' || :new.id);
        RAISE id_exist_ex;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        null;
END;

CREATE OR REPLACE TRIGGER check_group_name
 BEFORE INSERT OR UPDATE ON Groups
 FOR EACH ROW
DECLARE
    v_name_count NUMBER;
    name_exist_ex EXCEPTION;
BEGIN
    IF UPDATING AND :OLD.name = :NEW.name THEN
        RAISE NO_DATA_FOUND;
    END IF;

    SELECT COUNT(1) INTO v_name_count FROM Groups WHERE name = :new.name;
    IF v_name_count <> 0 THEN
        dbms_output.put_line('This group name exists ' || :new.name);
        RAISE name_exist_ex;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        null;
END;


INSERT INTO Groups(name, c_val) VALUES ('153501', 0);
INSERT INTO Students(name, group_id) VALUES ('Bob', 1);
INSERT INTO Students(name, group_id) VALUES ('Bib', 1);

--Task 3--
CREATE OR REPLACE TRIGGER fk_group_delete_cascade
 AFTER DELETE ON Groups
 FOR EACH ROW
BEGIN
    DELETE FROM Students
    WHERE group_id = :OLD.id;
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
CREATE OR REPLACE TRIGGER c_val_trigger
 AFTER INSERT OR UPDATE OR DELETE ON Students
 FOR EACH ROW
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