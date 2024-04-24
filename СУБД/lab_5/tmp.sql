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


    PROCEDURE Tab1_recovery(p_seconds INT) AS
    BEGIN
        Tab1_recovery('');
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
        Tab2_recovery(CURRENT_TIMESTAMP - (p_seconds * INTERVAL '1' SECOND));
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
        Tab3_recovery(CURRENT_TIMESTAMP - (p_seconds * INTERVAL '1' SECOND));
    END Tab3_recovery;

END recovery_package;
/