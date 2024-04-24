CREATE TABLE Tab1Log(
    id NUMBER PRIMARY KEY,
    operation VARCHAR2(10),
    op_time TIMESTAMP,
    n_id NUMBER,
    o_id NUMBER,
    n_name VARCHAR2(20),
    o_name VARCHAR2(20),
    n_val NUMBER,
    o_val NUMBER
);

CREATE SEQUENCE tab1_id_seq
START WITH 1
INCREMENT BY 1;

CREATE TABLE Tab2Log(
    id NUMBER PRIMARY KEY,
    operation VARCHAR2(10),
    op_time TIMESTAMP,
    n_id NUMBER,
    o_id NUMBER,
    n_name VARCHAR2(20),
    o_name VARCHAR2(20),
    n_time TIMESTAMP,
    o_time TimeSTAMP
);

CREATE SEQUENCE tab2_id_seq
START WITH 1
INCREMENT BY 1;

CREATE TABLE Tab3Log(
    id NUMBER PRIMARY KEY,
    operation VARCHAR2(10),
    op_time TIMESTAMP,
    n_id NUMBER,
    o_id NUMBER,
    n_name VARCHAR2(20),
    o_name VARCHAR2(20),
    n_fk INT,
    o_fk INT
);

CREATE SEQUENCE tab3_id_seq
START WITH 1
INCREMENT BY 1;

CREATE OR REPLACE TRIGGER Tab1_logger
BEFORE INSERT OR UPDATE OR DELETE 
ON Tab1 FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO Tab1Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_val, o_val) 
        VALUES (tab1_id_seq.nextval, 'INSERT', SYSTIMESTAMP, :NEW.id, NULL, :NEW.name, NULL, :NEW.val, NULL);
    ELSIF UPDATING THEN
        INSERT INTO Tab1Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_val, o_val) 
        VALUES (tab1_id_seq.nextval, 'UPDATE', SYSTIMESTAMP, :NEW.id, :OLD.id, :NEW.name, :OLD.name, :NEW.val, :OLD.val);
    ELSIF DELETING THEN
        INSERT INTO Tab1Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_val, o_val) 
        VALUES (tab1_id_seq.nextval, 'DELETE', SYSTIMESTAMP, NULL, :OLD.id, NULL, :OLD.name, NULL, :OLD.val);
    END IF;
END;
/

CREATE OR REPLACE TRIGGER Tab2_logger
BEFORE INSERT OR UPDATE OR DELETE 
ON Tab2 FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO Tab2Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_time, o_time) 
        VALUES (tab2_id_seq.nextval, 'INSERT', SYSTIMESTAMP, :NEW.id, NULL, :NEW.name, NULL, :NEW.time, NULL);
    ELSIF UPDATING THEN
        INSERT INTO Tab2Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_time, o_time) 
        VALUES (tab2_id_seq.nextval, 'UPDATE', SYSTIMESTAMP, :NEW.id, :OLD.id, :NEW.name, :OLD.name, :NEW.time, :OLD.time);
    ELSIF DELETING THEN
        INSERT INTO Tab2Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_time, o_time) 
        VALUES (tab2_id_seq.nextval, 'DELETE', SYSTIMESTAMP, NULL, :OLD.id, NULL, :OLD.name, NULL, :OLD.time);
    END IF;
END;
/

CREATE OR REPLACE TRIGGER Tab3_logger
BEFORE INSERT OR UPDATE OR DELETE 
ON Tab3 FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO Tab3Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_fk, o_fk) 
        VALUES (tab1_id_seq.nextval, 'INSERT', SYSTIMESTAMP, :NEW.id, NULL, :NEW.name, NULL, :NEW.Tab1_Id, NULL);
    ELSIF UPDATING THEN
        INSERT INTO Tab3Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_fk, o_fk) 
        VALUES (tab1_id_seq.nextval, 'UPDATE', SYSTIMESTAMP, :NEW.id, :OLD.id, :NEW.name, :OLD.name, :NEW.Tab1_Id, :OLD.Tab1_Id);
    ELSIF DELETING THEN
        INSERT INTO Tab3Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_fk, o_fk) 
        VALUES (tab1_id_seq.nextval, 'DELETE', SYSTIMESTAMP, NULL, :OLD.id, NULL, :OLD.name, NULL, :OLD.Tab1_Id);
    END IF;
END;
/

CREATE OR REPLACE PACKAGE recovery_package AS
    PROCEDURE Tab1_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tab1_recovery(p_seconds INT);
    PROCEDURE Tab2_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tab2_recovery(p_seconds INT);
    PROCEDURE Tab3_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tab3_recovery(p_seconds INT);
END recovery_package;
/

CREATE OR REPLACE PACKAGE BODY recovery_package AS
    PROCEDURE Tab1_recovery(p_datetime TIMESTAMP) IS
    BEGIN
        FOR action IN (SELECT * FROM Tab1Log WHERE p_datetime < op_time ORDER BY id DESC)
        LOOP
            IF action.operation = 'INSERT' THEN
                DELETE FROM Tab1 WHERE id = action.n_id;
            END IF;

            IF action.operation = 'UPDATE' THEN
                UPDATE Tab1 SET
                    id = action.o_id,
                    name = action.o_name,
                    val = action.o_val
                WHERE id = action.n_id;
            END IF;

            IF action.operation = 'DELETE' THEN
                INSERT INTO Tab1 VALUES (action.o_id, action.o_name, action.o_val);
            END IF;
        END LOOP;

        DELETE FROM Tab1Log WHERE op_time > p_datetime;
    END;

    DBMS_OUTPUT.PUT_LINE('End loop');
    PROCEDURE Tab1_recovery(p_seconds INT) AS
    BEGIN
        Tab1_recovery(SYSTIMESTAMP - INTERVAL '1' SECOND * p_seconds);
    END Tab1_recovery;


    PROCEDURE Tab2_recovery(p_datetime TIMESTAMP) IS
    BEGIN
        FOR action IN (SELECT * FROM Tab2Log WHERE p_datetime < op_time ORDER BY id DESC)
        LOOP
            IF action.operation = 'INSERT' THEN
                DELETE FROM Tab2 WHERE id = action.n_id;
            END IF;

            IF action.operation = 'UPDATE' THEN
                UPDATE Tab2 SET
                    id = action.o_id,
                    name = action.o_name,
                    time = action.o_time
                WHERE id = action.n_id;
            END IF;

            IF action.operation = 'DELETE' THEN
                INSERT INTO Tab2 VALUES (action.o_id, action.o_name, action.o_time);
            END IF;
        END LOOP;

        DELETE FROM Tab2Log WHERE op_time > p_datetime;
    END;


    PROCEDURE Tab2_recovery(p_seconds INT) AS
    BEGIN
        Tab2_recovery(SYSTIMESTAMP - INTERVAL '1' SECOND * p_seconds);
    END Tab2_recovery;


    PROCEDURE Tab3_recovery(p_datetime TIMESTAMP) IS
    BEGIN
        FOR action IN (SELECT * FROM Tab3Log WHERE p_datetime < op_time ORDER BY id DESC)
        LOOP
            IF action.operation = 'INSERT' THEN
                DELETE FROM Tab3 WHERE id = action.n_id;
            END IF;

            IF action.operation = 'UPDATE' THEN
                UPDATE Tab3 SET
                    id = action.o_id,
                    name = action.o_name,
                    Tab1_Id = action.o_fk
                WHERE id = action.n_id;
            END IF;

            IF action.operation = 'DELETE' THEN
                INSERT INTO Tab3 VALUES (action.o_id, action.o_name, action.o_fk);
            END IF;
        END LOOP;

        DELETE FROM Tab3Log WHERE op_time > p_datetime;
    END;


    PROCEDURE Tab3_recovery(p_seconds INT) AS
    BEGIN
        Tab3_recovery(SYSTIMESTAMP - INTERVAL '1' SECOND * p_seconds);
    END Tab3_recovery;

END recovery_package;
/

-- Task 4

CREATE OR REPLACE DIRECTORY my_directory AS '/opt/oracle';
GRANT READ, WRITE ON DIRECTORY my_directory TO PUBLIC; 

CREATE OR REPLACE PACKAGE otchet_packege AS
    FUNCTION get_html (title IN VARCHAR2, insert_count IN NUMBER, update_count IN NUMBER, delete_count IN NUMBER) RETURN VARCHAR2;
    PROCEDURE Tab1_report(p_datetime TIMESTAMP);
    PROCEDURE Tab1_report;
    PROCEDURE Tab2_report(p_datetime TIMESTAMP);
    PROCEDURE Tab2_report;
    PROCEDURE Tab3_report(p_datetime TIMESTAMP);
    PROCEDURE Tab3_report;
END otchet_packege;
/


CREATE OR REPLACE PACKAGE BODY otchet_packege AS

    FUNCTION get_html (title IN VARCHAR2, insert_count IN NUMBER, update_count IN NUMBER, delete_count IN NUMBER) 
RETURN VARCHAR2 IS
    result VARCHAR(4000);
BEGIN
    result := '<!DOCTYPE html>' || CHR(10) ||
                '<html lang="en">' || CHR(10) ||
                '<head>' || CHR(10) ||
                '</head>' || CHR(10) ||
                '<body>' || CHR(10) ||
                '<h2>' || title || '</h2>' || CHR(10) ||
                '<table>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <th>Операция</th>' || CHR(10) ||
                '        <th>Количество</th>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>INSERT</td>' || CHR(10) ||
                '        <td>' || insert_count || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>UPDATE</td>' || CHR(10) ||
                '        <td>' || update_count || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>DELETE</td>' || CHR(10) ||
                '        <td>' || delete_count || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '</table>' || CHR(10) ||
                '</body>' || CHR(10) ||
                '</html>' || CHR(10);

    DBMS_OUTPUT.PUT_LINE(result);
    RETURN result;
END get_html;

    -- Получение отчета по времени
    PROCEDURE Tab1_report(p_datetime TIMESTAMP) AS
        v_file_handle UTL_FILE.FILE_TYPE;
        report VARCHAR2(4000);
        title VARCHAR2(100);
        insert_count NUMBER;
        update_count NUMBER;
        delete_count NUMBER;
        result VARCHAR(4000);
    BEGIN
        title := 'Таблица 1 с ' || p_datetime;
        SELECT COUNT(*) INTO insert_count FROM Tab1Log WHERE operation = 'INSERT' AND p_datetime <= op_time;
        SELECT COUNT(*) INTO update_count FROM Tab1Log WHERE operation = 'UPDATE' AND p_datetime <= op_time;
        SELECT COUNT(*) INTO delete_count FROM Tab1Log WHERE operation = 'DELETE' AND p_datetime <= op_time;

        result := get_html(title, insert_count, update_count, delete_count);

        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tab1.html', 'W');
        UTL_FILE.PUT_LINE(v_file_handle, result);
        UTL_FILE.FCLOSE(v_file_handle);
    END Tab1_report;


    -- Получение отчета с момента последнего отчета
    PROCEDURE Tab1_report AS
        v_file_handle UTL_FILE.FILE_TYPE;
        v_file_text CLOB;
        v_pattern VARCHAR2(100) := 'Таблица 1 с ([^<]+)';
        v_match VARCHAR2(100);
        v_line_number NUMBER := 1;
    BEGIN
        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tab1.html', 'r');    
        LOOP
            UTL_FILE.GET_LINE(v_file_handle, v_file_text);
            IF v_line_number = 6 THEN
                EXIT;
            END IF;
            v_line_number := v_line_number + 1;
        END LOOP;  
        UTL_FILE.FCLOSE(v_file_handle);
        
        v_match := REGEXP_SUBSTR(v_file_text, v_pattern, 1, 1, NULL, 1);
        Tab1_report(v_match);
    END Tab1_report;

    -- Получение отчета по времени
    PROCEDURE Tab2_report(p_datetime TIMESTAMP) AS
        v_file_handle UTL_FILE.FILE_TYPE;
        report VARCHAR2(4000);
        title VARCHAR2(100);
        insert_count NUMBER;
        update_count NUMBER;
        delete_count NUMBER;
        result VARCHAR(4000);
    BEGIN
        title := 'Таблица 2 с ' || p_datetime;
        SELECT COUNT(*) INTO insert_count FROM Tab2Log WHERE operation = 'INSERT' AND p_datetime <= op_time;
        SELECT COUNT(*) INTO update_count FROM Tab2Log WHERE operation = 'UPDATE' AND p_datetime <= op_time;
        SELECT COUNT(*) INTO delete_count FROM Tab2Log WHERE operation = 'DELETE' AND p_datetime <= op_time;

        result := get_html(title, insert_count, update_count, delete_count);

        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tab2.html', 'W');
        UTL_FILE.PUT_LINE(v_file_handle, result);
        UTL_FILE.FCLOSE(v_file_handle);
    END Tab2_report;


    -- Получение отчета с момента последнего отчета
    PROCEDURE Tab2_report AS
        v_file_handle UTL_FILE.FILE_TYPE;
        v_file_text CLOB;
        v_pattern VARCHAR2(100) := 'Таблица 2 с ([^<]+)';
        v_match VARCHAR2(100);
        v_line_number NUMBER := 1;
    BEGIN
        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tab2.html', 'r');    
        LOOP
            UTL_FILE.GET_LINE(v_file_handle, v_file_text);
            IF v_line_number = 6 THEN
                EXIT;
            END IF;
            v_line_number := v_line_number + 1;
        END LOOP;  
        UTL_FILE.FCLOSE(v_file_handle);
        
        v_match := REGEXP_SUBSTR(v_file_text, v_pattern, 1, 1, NULL, 1);
        Tab2_report(v_match);
    END Tab2_report;

    -- Получение отчета по времени
    PROCEDURE Tab3_report(p_datetime TIMESTAMP) AS
        v_file_handle UTL_FILE.FILE_TYPE;
        report VARCHAR2(4000);
        title VARCHAR2(100);
        insert_count NUMBER;
        update_count NUMBER;
        delete_count NUMBER;
        result VARCHAR(4000);
    BEGIN
        title := 'Таблица с ' || p_datetime;
        SELECT COUNT(*) INTO insert_count FROM Tab3Log WHERE operation = 'INSERT' AND p_datetime <= op_time;
        SELECT COUNT(*) INTO update_count FROM Tab3Log WHERE operation = 'UPDATE' AND p_datetime <= op_time;
        SELECT COUNT(*) INTO delete_count FROM Tab3Log WHERE operation = 'DELETE' AND p_datetime <= op_time;

        result := get_html(title, insert_count, update_count, delete_count);

        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tab1.html', 'W');
        UTL_FILE.PUT_LINE(v_file_handle, result);
        UTL_FILE.FCLOSE(v_file_handle);
    END Tab3_report;


    -- Получение отчета с момента последнего отчета
    PROCEDURE Tab3_report AS
        v_file_handle UTL_FILE.FILE_TYPE;
        v_file_text CLOB;
        v_pattern VARCHAR2(100) := 'Таблица 3 с ([^<]+)';
        v_match VARCHAR2(100);
        v_line_number NUMBER := 1;
    BEGIN
        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tab3.html', 'r');    
        LOOP
            UTL_FILE.GET_LINE(v_file_handle, v_file_text);
            IF v_line_number = 6 THEN
                EXIT;
            END IF;
            v_line_number := v_line_number + 1;
        END LOOP;  
        UTL_FILE.FCLOSE(v_file_handle);
        
        v_match := REGEXP_SUBSTR(v_file_text, v_pattern, 1, 1, NULL, 1);
        Tab3_report(v_match);
    END Tab3_report;
END;
/
