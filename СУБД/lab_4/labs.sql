CREATE OR REPLACE PROCEDURE EXECUTE_SELECT(
    json_data IN CLOB,
    v_sql_query OUT VARCHAR2
) IS
    temp_string VARCHAR2(1000);
BEGIN
    -- Находим знаяение по данному выражению
    v_sql_query := JSON_VALUE(json_data, '$.type') || ' ';

    FOR rec IN (
        -- Переводим json в реляционную со столбцом value
        SELECT value AS column_name
        FROM JSON_TABLE(json_data, '$.columns[*]' COLUMNS (value PATH '$'))
    )
    LOOP
        -- Добавляем к результирующей наши колонки
        v_sql_query := v_sql_query || rec.column_name || ', ';
    END LOOP;

    -- Удлаяем последнюю запятую и добавляем from
    v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 2) || ' FROM ';

    -- Проходимся по таблицам
    FOR rec IN (
        SELECT value AS table_name
        FROM JSON_TABLE(json_data, '$.tables[*]' COLUMNS (value PATH '$'))
    )
    LOOP
        v_sql_query := v_sql_query || rec.table_name || ', ';
    END LOOP;
    -- Удаляем последнюю запятую и добавляем переход на новую строку
    v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 2) || CHR(10);

    -- Объединение таблиц
    FOR rec IN (
        -- Преобразуем наш json с join в таблицу 
        SELECT join_table.*
        FROM JSON_TABLE(json_data, '$.joins[*]'
            COLUMNS (
                table_name VARCHAR2(100) PATH '$.table',
                operator VARCHAR2(100) PATH '$.operator',
                condition JSON PATH '$.condition'
            )
        ) join_table
    )
    LOOP
        v_sql_query := v_sql_query || 'JOIN ' || rec.table_name || ' ON ';

        -- Добавляем условия
        FOR cond_rec IN (
            SELECT value AS condition
            FROM JSON_TABLE(rec.condition, '$[*]' COLUMNS (value VARCHAR2(100) PATH '$'))
        )
        LOOP
            IF rec.operator IS NULL THEN
                v_sql_query := v_sql_query || cond_rec.condition || ' AND ';
            ELSE
                v_sql_query := v_sql_query || cond_rec.condition || ' ' || rec.operator || ' ';
            END IF;
        END LOOP;

        IF rec.operator IS NULL THEN
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 5);
        ELSE
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - (2 + LENGTH(rec.operator)));
        END IF;

        v_sql_query := v_sql_query || CHR(10);
    END LOOP;
    
    -- Проходим по фильтрам. В теле лежат все условия
    FOR rec IN (
        SELECT filter_table.*
        FROM JSON_TABLE(json_data, '$.filters'
            COLUMNS (
                filter_type VARCHAR2(100) PATH '$.type',
                operator VARCHAR2(100) PATH '$.operator',
                filter_body VARCHAR2(4000) FORMAT JSON PATH '$.body' 
            )
        ) filter_table
    )
    LOOP
        v_sql_query := v_sql_query || rec.filter_type || ' ';

        FOR body_rec IN ( 
            SELECT value AS element
            FROM JSON_TABLE(rec.filter_body, '$[*]' COLUMNS (value VARCHAR2(4000) FORMAT JSON PATH '$'))
        )
        LOOP
            -- Если в теле фильтра лежит тип, то это вложенное условие
            IF JSON_EXISTS(body_rec.element, '$.type') THEN
            -- Для экзистса
                IF JSON_EXISTS(body_rec.element, '$.value') THEN
                    v_sql_query := v_sql_query || JSON_VALUE(JSON_QUERY(body_rec.element, '$.body'), '$.value');
                END IF;
                v_sql_query := v_sql_query ||  ' ' || JSON_VALUE(body_rec.element, '$.type') || ' ';
                EXECUTE_SELECT(JSON_QUERY(JSON_QUERY(body_rec.element, '$.body'), '$.condition'), temp_string);
                v_sql_query := v_sql_query || '(' || temp_string || ')';
            ELSE
                v_sql_query := v_sql_query || REPLACE(body_rec.element, '"', '');
            END IF;

            IF rec.operator IS NULL THEN
                v_sql_query := v_sql_query || ' AND ';
            ELSE
                v_sql_query := v_sql_query || ' ' || rec.operator || ' ';
            END IF;
        END LOOP;

        IF rec.operator IS NULL THEN
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 5);
        ELSE
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - (2 + LENGTH(rec.operator)));
        END IF;

        v_sql_query := v_sql_query || CHR(10);
    END LOOP;

    v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 1);
END;
/


CREATE OR REPLACE PROCEDURE EXECUTE_INSERT(
    json_data IN CLOB,
    v_sql_query OUT VARCHAR2
) IS
    temp_string VARCHAR2(1000);
BEGIN
    v_sql_query := JSON_VALUE(json_data, '$.type') || ' INTO ' || JSON_VALUE(json_data, '$.table') || ' (';

    FOR rec IN (
        SELECT value AS column_name
        FROM JSON_TABLE(json_data, '$.columns[*]' COLUMNS (value PATH '$'))
    )
    LOOP
        v_sql_query := v_sql_query || rec.column_name || ', ';
    END LOOP;
    v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 2) || ') VALUES ';

    FOR rec IN (
        SELECT column_value AS item
        FROM JSON_TABLE(json_data, '$.values[*]' COLUMNS (
            column_value VARCHAR2(4000) FORMAT JSON PATH '$'
        ))
    )
    LOOP
        temp_string := REPLACE(rec.item, '[', '');
        v_sql_query := v_sql_query || '(' || REPLACE(REPLACE(temp_string, ']', ''), '"', '''') || '), ';
    END LOOP;
    v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 2);
END;
/


CREATE OR REPLACE PROCEDURE EXECUTE_UPDATE(
    json_data IN CLOB,
    v_sql_query OUT VARCHAR2
) IS
    temp_string VARCHAR2(1000);
    v_number NUMBER;
BEGIN
    v_sql_query := JSON_VALUE(json_data, '$.type') || ' ' || JSON_VALUE(json_data, '$.table') || CHR(10) || 'SET ';

    FOR rec IN (
        SELECT * 
        FROM JSON_TABLE(json_data, '$.set[*]' COLUMNS (
            column_name VARCHAR2(4000) PATH '$.column',
            column_value VARCHAR2(4000) PATH '$.value'
        ))
    )
    LOOP
        BEGIN
            v_number := TO_NUMBER(rec.column_value);
            v_sql_query := v_sql_query || rec.column_name || ' = ' || rec.column_value || ', ';
        EXCEPTION
            WHEN OTHERS THEN
                v_sql_query := v_sql_query || rec.column_name || ' = ''' || rec.column_value || ''', ';
        END;
    END LOOP;

    v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 2) || CHR(10);

    FOR rec IN (
        SELECT filter_table.*
        FROM JSON_TABLE(json_data, '$.filters[*]'
            COLUMNS (
                filter_type VARCHAR2(100) PATH '$.type',
                operator VARCHAR2(100) PATH '$.operator',
                filter_body VARCHAR2(4000) FORMAT JSON PATH '$.body' 
            )
        ) filter_table
    )
    LOOP
        v_sql_query := v_sql_query || rec.filter_type || ' ';

        FOR body_rec IN ( 
            SELECT value AS element
            FROM JSON_TABLE(rec.filter_body, '$[*]' COLUMNS (value VARCHAR2(4000) FORMAT JSON PATH '$'))
        )
        LOOP
            IF JSON_EXISTS(body_rec.element, '$.type') THEN
                v_sql_query := v_sql_query || JSON_VALUE(JSON_QUERY(body_rec.element, '$.body'), '$.value') || ' '
                                || JSON_VALUE(body_rec.element, '$.type') || ' ';
                EXECUTE_SELECT(JSON_QUERY(JSON_QUERY(body_rec.element, '$.body'), '$.condition'), temp_string);
                v_sql_query := v_sql_query || '(' || temp_string || ')';
            ELSE
                v_sql_query := v_sql_query || REPLACE(body_rec.element, '"', '');
            END IF;

            IF rec.operator IS NULL THEN
                v_sql_query := v_sql_query || ' AND ';
            ELSE
                v_sql_query := v_sql_query || ' ' || rec.operator || ' ';
            END IF;
        END LOOP;

        IF rec.operator IS NULL THEN
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 5);
        ELSE
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - (2 + LENGTH(rec.operator)));
        END IF;

        v_sql_query := v_sql_query || CHR(10);
    END LOOP;
END;
/


CREATE OR REPLACE PROCEDURE EXECUTE_DELETE(
    json_data IN CLOB,
    v_sql_query OUT VARCHAR2
) IS
    temp_string VARCHAR2(1000);
BEGIN
    v_sql_query := JSON_VALUE(json_data, '$.type') || ' FROM ' || JSON_VALUE(json_data, '$.table') || CHR(10);

    FOR rec IN (
        SELECT filter_table.*
        FROM JSON_TABLE(json_data, '$.filters[*]'
            COLUMNS (
                filter_type VARCHAR2(100) PATH '$.type',
                operator VARCHAR2(100) PATH '$.operator',
                filter_body VARCHAR2(4000) FORMAT JSON PATH '$.body' 
            )
        ) filter_table
    )
    LOOP
        v_sql_query := v_sql_query || rec.filter_type || ' ';

        FOR body_rec IN ( 
            SELECT value AS element
            FROM JSON_TABLE(rec.filter_body, '$[*]' COLUMNS (value VARCHAR2(4000) FORMAT JSON PATH '$'))
        )
        LOOP
            IF JSON_EXISTS(body_rec.element, '$.type') THEN
                v_sql_query := v_sql_query || JSON_VALUE(JSON_QUERY(body_rec.element, '$.body'), '$.value') || ' '
                                || JSON_VALUE(body_rec.element, '$.type') || ' ';
                EXECUTE_SELECT(JSON_QUERY(JSON_QUERY(body_rec.element, '$.body'), '$.condition'), temp_string);
                v_sql_query := v_sql_query || '(' || temp_string || ')';
            ELSE
                v_sql_query := v_sql_query || REPLACE(body_rec.element, '"', '');
            END IF;

            IF rec.operator IS NULL THEN
                v_sql_query := v_sql_query || ' AND ';
            ELSE
                v_sql_query := v_sql_query || ' ' || rec.operator || ' ';
            END IF;
        END LOOP;

        IF rec.operator IS NULL THEN
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 5);
        ELSE
            v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - (2 + LENGTH(rec.operator)));
        END IF;
    END LOOP;
END;
/


CREATE OR REPLACE PROCEDURE EXECUTE_CREATE(
    json_data IN CLOB,
    v_sql_query OUT VARCHAR2,
    trigger_query OUT VARCHAR2
) IS
    table_name VARCHAR2(100);
    primary_name VARCHAR2(100);
BEGIN
    table_name := JSON_VALUE(json_data, '$.table');
    primary_name := JSON_VALUE(json_data, '$.primary');

    v_sql_query := JSON_VALUE(json_data, '$.type') || ' TABLE ' || table_name || ' (' || CHR(10);

    FOR rec IN (
        SELECT *
        FROM JSON_TABLE(json_data, '$.columns[*]'
            COLUMNS (
                column_name VARCHAR2(100) PATH '$.name',
                column_datatype VARCHAR2(100) PATH '$.datatype',
                column_constraint VARCHAR2(100) PATH '$.constraint' 
            )
        ) 
    )
    LOOP
        v_sql_query := v_sql_query || '    ' || rec.column_name || ' ' || rec.column_datatype;
        IF rec.column_constraint IS NOT NULL THEN
            v_sql_query := v_sql_query || ' ' || rec.column_constraint;
        END IF;
        v_sql_query := v_sql_query || ', ' || CHR(10);
    END LOOP;

    v_sql_query := v_sql_query || '    PRIMARY KEY (' || primary_name || '), ' || CHR(10);

    FOR rec IN (
        SELECT *
        FROM JSON_TABLE(json_data, '$.foreign[*]'
            COLUMNS (
                column_name VARCHAR2(100) PATH '$.column',
                refcolumn_name VARCHAR2(100) PATH '$.refcolumn',
                reftable_name VARCHAR2(100) PATH '$.reftable' 
            )
        ) 
    )
    LOOP
        v_sql_query := v_sql_query || '    FOREIGN KEY (' || rec.column_name || ') REFERENCES '
                        || rec.reftable_name || ' (' || rec.refcolumn_name || '), ' || CHR(10);
    END LOOP;

    v_sql_query := SUBSTR(v_sql_query, 1, LENGTH(v_sql_query) - 3);

    v_sql_query := v_sql_query || CHR(10) || ')' || CHR(10);

    trigger_query := 'CREATE OR REPLACE TRIGGER trg_generate_pk_on_' || table_name || CHR(10) ||
                    'BEFORE INSERT ON ' || table_name || CHR(10) ||
                    'FOR EACH ROW' || CHR(10) ||
                    'BEGIN' || CHR(10) ||
                    '    IF :NEW.' || primary_name || ' IS NULL THEN' || CHR(10) ||
                    '        SELECT NVL(MAX(' || primary_name || '), 0) + 1 INTO :NEW.' || primary_name || ' FROM ' || table_name || ';' || CHR(10) ||
                    '    END IF;' || CHR(10) ||
                    'END;' || CHR(10);
END;
/


CREATE OR REPLACE PROCEDURE EXECUTE_DROP(
    json_data IN CLOB,
    v_sql_query OUT VARCHAR2
) IS
BEGIN
    v_sql_query := JSON_VALUE(json_data, '$.type') || ' TABLE ' || JSON_VALUE(json_data, '$.table');
END;
/


CREATE OR REPLACE PROCEDURE JSON_PARSE(
    json_data IN CLOB,
    v_sql_query OUT VARCHAR2,
    v_cursor OUT SYS_REFCURSOR
) IS
    v_sql_type VARCHAR2(100);
    trigger_query VARCHAR2(1000);
BEGIN
    v_sql_type := JSON_VALUE(json_data, '$.type');

    CASE v_sql_type
        WHEN 'SELECT' THEN
            EXECUTE_SELECT(json_data, v_sql_query); 
            OPEN v_cursor FOR v_sql_query;
        WHEN 'INSERT' THEN
            EXECUTE_INSERT(json_data, v_sql_query); 
            EXECUTE IMMEDIATE v_sql_query;
        WHEN 'UPDATE' THEN
            EXECUTE_UPDATE(json_data, v_sql_query);
            EXECUTE IMMEDIATE v_sql_query; 
        WHEN 'DELETE' THEN
            EXECUTE_DELETE(json_data, v_sql_query); 
            EXECUTE IMMEDIATE v_sql_query;
        WHEN 'CREATE' THEN
            EXECUTE_CREATE(json_data, v_sql_query, trigger_query); 
            EXECUTE IMMEDIATE v_sql_query;
            EXECUTE IMMEDIATE trigger_query;
        WHEN 'DROP' THEN
            EXECUTE_DROP(json_data, v_sql_query); 
            EXECUTE IMMEDIATE v_sql_query;
    END CASE;  
END;
/