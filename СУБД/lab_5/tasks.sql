-- Журналирование + откат

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

CREATE TABLE TabsLog(
    id NUMBER PRIMARY KEY,
    op_time TIMESTAMP,
    tab_id NUMBER,
    tab_type NUMBER
);

CREATE SEQUENCE tabs_id_seq
START WITH 1
INCREMENT BY 1;

CREATE OR REPLACE TRIGGER Tabs_1_logger
BEFORE INSERT OR DELETE 
ON Tab1Log FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO TabsLog (id, op_time, tab_id, tab_type) 
        VALUES (tabs_id_seq.nextval, CURRENT_TIMESTAMP, :NEW.id, 1);
    ELSIF DELETING THEN
        DELETE FROM TabsLog WHERE tab_id = :OLD.id AND tab_type = 1;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER Tabs_2_logger
BEFORE INSERT OR DELETE 
ON Tab2Log FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO TabsLog (id, op_time, tab_id, tab_type) 
        VALUES (tabs_id_seq.nextval, CURRENT_TIMESTAMP, :NEW.id, 2);
    ELSIF DELETING THEN
        DELETE FROM TabsLog WHERE tab_id = :OLD.id AND tab_type = 2;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER Tabs_3_logger
BEFORE INSERT OR DELETE 
ON Tab3Log FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO TabsLog (id, op_time, tab_id, tab_type) 
        VALUES (tabs_id_seq.nextval, CURRENT_TIMESTAMP, :NEW.id, 3);
    ELSIF DELETING THEN
        DELETE FROM TabsLog WHERE tab_id = :OLD.id AND tab_type = 3;
    END IF;
END;
/


CREATE OR REPLACE TRIGGER Tab1_logger
BEFORE INSERT OR UPDATE OR DELETE 
ON Tab1 FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO Tab1Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_val, o_val) 
        VALUES (tab1_id_seq.nextval, 'INSERT', CURRENT_TIMESTAMP, :NEW.id, NULL, :NEW.name, NULL, :NEW.val, NULL);
    ELSIF UPDATING THEN
        INSERT INTO Tab1Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_val, o_val) 
        VALUES (tab1_id_seq.nextval, 'UPDATE', CURRENT_TIMESTAMP, :NEW.id, :OLD.id, :NEW.name, :OLD.name, :NEW.val, :OLD.val);
END;
/

CREATE OR REPLACE TRIGGER Tab1_logger_Del
AFTER DELETE 
ON Tab1 FOR EACH ROW
DECLARE
BEGIN
    INSERT INTO Tab1Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_val, o_val) 
    VALUES (tab1_id_seq.nextval, 'DELETE', CURRENT_TIMESTAMP, NULL, :OLD.id, NULL, :OLD.name, NULL, :OLD.val);
END;
/

CREATE OR REPLACE TRIGGER Tab2_logger
BEFORE INSERT OR UPDATE OR DELETE 
ON Tab2 FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO Tab2Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_time, o_time) 
        VALUES (tab2_id_seq.nextval, 'INSERT', CURRENT_TIMESTAMP, :NEW.id, NULL, :NEW.name, NULL, :NEW.time, NULL);
    ELSIF UPDATING THEN
        INSERT INTO Tab2Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_time, o_time) 
        VALUES (tab2_id_seq.nextval, 'UPDATE', CURRENT_TIMESTAMP, :NEW.id, :OLD.id, :NEW.name, :OLD.name, :NEW.time, :OLD.time);
    ELSIF DELETING THEN
        INSERT INTO Tab2Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_time, o_time) 
        VALUES (tab2_id_seq.nextval, 'DELETE', CURRENT_TIMESTAMP, NULL, :OLD.id, NULL, :OLD.name, NULL, :OLD.time);
    END IF;
END;
/

CREATE OR REPLACE TRIGGER Tab3_logger
BEFORE INSERT OR UPDATE
ON Tab3 FOR EACH ROW
DECLARE
BEGIN
    IF INSERTING THEN
        INSERT INTO Tab3Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_fk, o_fk) 
        VALUES (tab3_id_seq.nextval, 'INSERT', CURRENT_TIMESTAMP, :NEW.id, NULL, :NEW.name, NULL, :NEW.Tab1_Id, NULL);
    ELSIF UPDATING THEN
        INSERT INTO Tab3Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_fk, o_fk) 
        VALUES (tab3_id_seq.nextval, 'UPDATE', CURRENT_TIMESTAMP, :NEW.id, :OLD.id, :NEW.name, :OLD.name, :NEW.Tab1_Id, :OLD.Tab1_Id);
END;
/

CREATE OR REPLACE TRIGGER Tab3_logger_Del
AFTER DELETE 
ON Tab3 FOR EACH ROW
DECLARE
BEGIN
    INSERT INTO Tab3Log (id, operation, op_time, n_id, o_id, n_name, o_name, n_fk, o_fk) 
        VALUES (tab3_id_seq.nextval, 'DELETE', CURRENT_TIMESTAMP, NULL, :OLD.id, NULL, :OLD.name, NULL, :OLD.Tab1_Id);
END;
/


CREATE OR REPLACE PACKAGE recovery_package AS
    PROCEDURE Tabs_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tabs_recovery(p_seconds INT);
END recovery_package;
/

CREATE OR REPLACE PACKAGE BODY recovery_package AS

    PROCEDURE Tabs_recovery(p_datetime TIMESTAMP) AS
        tab1_record Tab1Log%ROWTYPE;
        tab2_record Tab2Log%ROWTYPE;
        tab3_record Tab3Log%ROWTYPE;
    BEGIN
        FOR action IN (SELECT * FROM TabsLog WHERE p_datetime < op_time ORDER BY id DESC)
        LOOP
            -- Если действие для первой таблицы
            IF action.tab_type = 1 THEN
                SELECT * INTO tab1_record FROM Tab1Log WHERE id = action.tab_id;

                IF tab1_record.operation = 'INSERT' THEN
                    DELETE FROM Tab1 WHERE id = tab1_record.n_id;
                END IF;

                IF tab1_record.operation = 'UPDATE' THEN
                    UPDATE Tab1 SET
                        id = tab1_record.o_id,
                        name = tab1_record.o_name,
                        val = tab1_record.o_val
                    WHERE id = tab1_record.n_id;
                END IF;

                IF tab1_record.operation = 'DELETE' THEN
                    INSERT INTO Tab1 VALUES (tab1_record.o_id, tab1_record.o_name, tab1_record.o_val);
                END IF;

            -- Если действие для второй таблицы
            ELSIF action.tab_type = 2 THEN
                SELECT * INTO tab2_record FROM Tab2Log WHERE id = action.tab_id;

                IF tab2_record.operation = 'INSERT' THEN
                    DELETE FROM Tab2 WHERE id = tab2_record.n_id;
                END IF;

                IF tab2_record.operation = 'UPDATE' THEN
                    UPDATE Tab2 SET
                        id = tab2_record.o_id,
                        name = tab2_record.o_name,
                        time = tab2_record.o_time
                    WHERE id = tab2_record.n_id;
                END IF;

                IF tab2_record.operation = 'DELETE' THEN
                    INSERT INTO Tab2 VALUES (tab2_record.o_id, tab2_record.o_name, tab2_record.o_time);
                END IF;
            -- Если действие для третьей таблицы
            ELSIF action.tab_type = 3 THEN
                SELECT * INTO tab3_record FROM Tab3Log WHERE id = action.tab_id;

                IF tab3_record.operation = 'INSERT' THEN
                    DELETE FROM Tab3 WHERE id = tab3_record.n_id;
                END IF;

                IF tab3_record.operation = 'UPDATE' THEN
                    UPDATE Tab3 SET
                        id = tab3_record.o_id,
                        name = tab3_record.o_name,
                        Tab1_Id = tab3_record.o_fk
                    WHERE id = tab3_record.n_id;
                END IF;

                IF tab3_record.operation = 'DELETE' THEN
                    INSERT INTO Tab3 VALUES (tab3_record.o_id, tab3_record.o_name, tab3_record.o_fk);
                END IF;
            END IF;
        END LOOP;

        DELETE FROM Tab1Log WHERE op_time > p_datetime;
        DELETE FROM Tab2Log WHERE op_time > p_datetime;
        DELETE FROM Tab3Log WHERE op_time > p_datetime;
    END Tabs_recovery;


    PROCEDURE Tabs_recovery(p_seconds INT) AS
    BEGIN
        Tabs_recovery(CURRENT_TIMESTAMP - INTERVAL '1' SECOND * p_seconds);
    END Tabs_recovery;

END recovery_package;
/






-- СТАРОЕ!!! НИНАДА!!!


CREATE OR REPLACE PACKAGE recovery_package AS
    PROCEDURE Tab1_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tab1_recovery(p_seconds INT);
    PROCEDURE Tab2_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tab2_recovery(p_seconds INT);
    PROCEDURE Tab3_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tab3_recovery(p_seconds INT);
    PROCEDURE Tabs_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tabs_recovery(p_seconds INT);
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

    PROCEDURE Tab1_recovery(p_seconds INT) AS
    BEGIN
        Tab1_recovery(CURRENT_TIMESTAMP - INTERVAL '1' SECOND * p_seconds);
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
    END Tab2_recovery;


    PROCEDURE Tab2_recovery(p_seconds INT) AS
    BEGIN
        Tab2_recovery(CURRENT_TIMESTAMP - INTERVAL '1' SECOND * p_seconds);
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
    END Tab3_recovery;


    PROCEDURE Tab3_recovery(p_seconds INT) AS
    BEGIN
        Tab3_recovery(CURRENT_TIMESTAMP - INTERVAL '1' SECOND * p_seconds);
    END Tab3_recovery;

    PROCEDURE Tabs_recovery(p_datetime TIMESTAMP) AS
        TYPE t_tab1 IS TABLE OF Tab1Log%ROWTYPE;
        tab1_record t_tab1;
        TYPE t_tab2 IS TABLE OF Tab2Log%ROWTYPE;
        tab3_record t_tab2;
        TYPE t_tab3 IS TABLE OF Tab3Log%ROWTYPE;
        tab3_record t_tab3;
    BEGIN
        FOR action IN (SELECT * FROM TabsLog WHERE p_datetime < op_time ORDER BY id DESC)
        LOOP
            -- Если действие для первой таблицы
            IF action.tab_type = 1 THEN
                SELECT * INTO tab1_record FROM Tab1Log WHERE id = action.tab_id;

                IF tab1_record.operation = 'INSERT' THEN
                    DELETE FROM Tab1 WHERE id = tab1_record.n_id;
                END IF;

                IF tab1_record.operation = 'UPDATE' THEN
                    UPDATE Tab1 SET
                        id = tab1_record.o_id,
                        name = tab1_record.o_name,
                        val = tab1_record.o_val
                    WHERE id = tab1_record.n_id;
                END IF;

                IF tab1_record.operation = 'DELETE' THEN
                    INSERT INTO Tab1 VALUES (tab1_record.o_id, tab1_record.o_name, tab1_record.o_val);
                END IF;

            -- Если действие для второй таблицы
            ELSIF action.tab_type = 2 THEN
                SELECT * INTO tab2_record FROM Tab2Log WHERE id = action.tab_id;

                IF tab2_record.operation = 'INSERT' THEN
                    DELETE FROM Tab2 WHERE id = tab2_record.n_id;
                END IF;

                IF tab2_record.operation = 'UPDATE' THEN
                    UPDATE Tab2 SET
                        id = tab2_record.o_id,
                        name = tab2_record.o_name,
                        time = tab2_record.o_time
                    WHERE id = tab2_record.n_id;
                END IF;

                IF tab2_record.operation = 'DELETE' THEN
                    INSERT INTO Tab2 VALUES (tab2_record.o_id, tab2_record.o_name, tab2_record.o_time);
                END IF;
            -- Если действие для третьей таблицы
            ELSIF action.tab_type = 3 THEN
                SELECT * INTO tab3_record FROM Tab2Log WHERE id = action.tab_id;

                IF tab3_record.operation = 'INSERT' THEN
                    DELETE FROM Tab3 WHERE id = tab3_record.n_id;
                END IF;

                IF tab3_record.operation = 'UPDATE' THEN
                    UPDATE Tab3 SET
                        id = tab3_record.o_id,
                        name = tab3_record.o_name,
                        Tab1_Id = tab3_record.o_fk
                    WHERE id = tab3_record.n_id;
                END IF;

                IF tab3_record.operation = 'DELETE' THEN
                    INSERT INTO Tab3 VALUES (tab3_record.o_id, tab3_record.o_name, tab3_record.o_fk);
                END IF;
            END IF;
        END LOOP;

        DELETE FROM Tab1Log WHERE op_time > p_datetime;
        DELETE FROM Tab2Log WHERE op_time > p_datetime;
        DELETE FROM Tab3Log WHERE op_time > p_datetime;
    END Tabs_recovery;


    PROCEDURE Tabs_recovery(p_seconds INT) AS
    BEGIN
        Tabs_recovery(CURRENT_TIMESTAMP - INTERVAL '1' SECOND * p_seconds);
    END Tabs_recovery;

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

        UPDATE LastOtchet SET Time = p_datetime WHERE TableName = 'Tab1';
    END Tab1_report;


    -- Получение отчета с момента последнего отчета
    PROCEDURE Tab1_report AS
        v_time TIMESTAMP;
    BEGIN
        SELECT Time INTO v_time FROM LastOtchet WHERE TableName = 'Tab1';

        IF v_time IS NULL THEN
            SELECT MIN(op_time) INTO v_time FROM Tab1Log;
        END IF;

        Tab1_report(v_time);
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
        v_time TIMESTAMP;
    BEGIN
        SELECT Time INTO v_time FROM LastOtchet WHERE TableName = 'Tab2';

        IF v_time IS NULL THEN
            SELECT MIN(op_time) INTO v_time FROM Tab2Log;
        END IF;

        Tab2_report(v_time);
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

        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tab3.html', 'W');
        UTL_FILE.PUT_LINE(v_file_handle, result);
        UTL_FILE.FCLOSE(v_file_handle);
    END Tab3_report;


    -- Получение отчета с момента последнего отчета
    PROCEDURE Tab3_report AS
        v_time TIMESTAMP;
    BEGIN
        SELECT Time INTO v_time FROM LastOtchet WHERE TableName = 'Tab3';

        IF v_time IS NULL THEN
            SELECT MIN(op_time) INTO v_time FROM Tab3Log;
        END IF;

        Tab3_report(v_time);
    END Tab3_report;
END;
/
