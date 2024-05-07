-- Отчет

CREATE OR REPLACE DIRECTORY my_directory AS '/opt/oracle';
GRANT READ, WRITE ON DIRECTORY my_directory TO PUBLIC; 

CREATE OR REPLACE PACKAGE otchet_package AS
    FUNCTION get_html (p_datetime TIMESTAMP) RETURN VARCHAR2;
    PROCEDURE Tabs_report(p_datetime TIMESTAMP);
    PROCEDURE Tabs_report;
END otchet_package;
/

CREATE OR REPLACE PACKAGE BODY otchet_package AS

    FUNCTION get_html (p_datetime TIMESTAMP) 
RETURN VARCHAR2 IS
    result VARCHAR(4000);
    title VARCHAR2(100);
    insert_count_1 NUMBER;
    update_count_1 NUMBER;
    delete_count_1 NUMBER;
    insert_count_2 NUMBER;
    update_count_2 NUMBER;
    delete_count_2 NUMBER;
    insert_count_3 NUMBER;
    update_count_3 NUMBER;
    delete_count_3 NUMBER;
BEGIN
    --DBMS_OUTPUT.PUT_LINE('Get html');
    --DBMS_OUTPUT.PUT_LINE(p_datetime);
    title := 'Отчет по таблицам с момента времени: ' || p_datetime;

    SELECT COUNT(1) INTO insert_count_1 FROM Tab1Log WHERE operation = 'INSERT' AND p_datetime <= op_time;
    SELECT COUNT(1) INTO update_count_1 FROM Tab1Log WHERE operation = 'UPDATE' AND p_datetime <= op_time;
    SELECT COUNT(1) INTO delete_count_1 FROM Tab1Log WHERE operation = 'DELETE' AND p_datetime <= op_time;

    SELECT COUNT(1) INTO insert_count_2 FROM Tab2Log WHERE operation = 'INSERT' AND p_datetime <= op_time;
    SELECT COUNT(1) INTO update_count_2 FROM Tab2Log WHERE operation = 'UPDATE' AND p_datetime <= op_time;
    SELECT COUNT(1) INTO delete_count_2 FROM Tab2Log WHERE operation = 'DELETE' AND p_datetime <= op_time;

    SELECT COUNT(1) INTO insert_count_3 FROM Tab3Log WHERE operation = 'INSERT' AND p_datetime <= op_time;
    SELECT COUNT(1) INTO update_count_3 FROM Tab3Log WHERE operation = 'UPDATE' AND p_datetime <= op_time;
    SELECT COUNT(1) INTO delete_count_3 FROM Tab3Log WHERE operation = 'DELETE' AND p_datetime <= op_time;

    result := '<!DOCTYPE html>' || CHR(10) ||
                '<html lang="en">' || CHR(10) ||
                '<head>' || CHR(10) ||
                '</head>' || CHR(10) ||
                '<body>' || CHR(10) ||
                '<h2>' || title || '</h2>' || CHR(10) ||

                '<h3>Таблица 1<h3>' || CHR(10) ||
                '<table>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <th>Операция</th>' || CHR(10) ||
                '        <th>Количество</th>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>INSERT</td>' || CHR(10) ||
                '        <td>' || insert_count_1 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>UPDATE</td>' || CHR(10) ||
                '        <td>' || update_count_1 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>DELETE</td>' || CHR(10) ||
                '        <td>' || delete_count_1 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '</table>' || CHR(10) ||
                '<br>' || CHR(10) ||

                '<h3>Таблица 2<h3>' || CHR(10) ||
                '<table>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <th>Операция</th>' || CHR(10) ||
                '        <th>Количество</th>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>INSERT</td>' || CHR(10) ||
                '        <td>' || insert_count_2 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>UPDATE</td>' || CHR(10) ||
                '        <td>' || update_count_2 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>DELETE</td>' || CHR(10) ||
                '        <td>' || delete_count_2 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '</table>' || CHR(10) ||
                '<br>' || CHR(10) ||

                '<h3>Таблица 3<h3>' || CHR(10) ||
                '<table>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <th>Операция</th>' || CHR(10) ||
                '        <th>Количество</th>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>INSERT</td>' || CHR(10) ||
                '        <td>' || insert_count_3 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>UPDATE</td>' || CHR(10) ||
                '        <td>' || update_count_3 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '    <tr>' || CHR(10) ||
                '        <td>DELETE</td>' || CHR(10) ||
                '        <td>' || delete_count_3 || '</td>' || CHR(10) ||
                '    </tr>' || CHR(10) ||
                '</table>' || CHR(10) ||
                '</body>' || CHR(10) ||
                '</html>' || CHR(10);

    DBMS_OUTPUT.PUT_LINE(result);
    RETURN result;
END get_html;

    -- Получение отчета по времени
    PROCEDURE Tabs_report(p_datetime TIMESTAMP) AS
        v_file_handle UTL_FILE.FILE_TYPE;
        result VARCHAR(4000);
    BEGIN
        result := get_html(p_datetime);

        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'tabs_report.html', 'W');
        UTL_FILE.PUT_LINE(v_file_handle, result);
        UTL_FILE.FCLOSE(v_file_handle);

        --DBMS_OUTPUT.PUT_LINE('Update');
        UPDATE LastOtchet SET Time = CURRENT_TIMESTAMP WHERE ROWNUM = 1;
    END Tabs_report;


    -- Получение отчета с момента последнего отчета
    PROCEDURE Tabs_report AS
        v_time TIMESTAMP;
    BEGIN
        SELECT Time INTO v_time FROM LastOtchet WHERE ROWNUM = 1;

        -- Если таблица со временем пустая, то берем минимальное из нынешних логов
        IF v_time IS NULL THEN
            DBMS_OUTPUT.PUT_LINE('If');
            SELECT MIN(op_time) INTO v_time FROM (
                SELECT op_time FROM Tab1Log 
                UNION ALL  
                SELECT op_time FROM Tab2Log 
                UNION ALL 
                SELECT op_time FROM Tab3Log);
        END IF;

        --DBMS_OUTPUT.PUT_LINE('Visov');
        Tabs_report(v_time);
    END Tabs_report;
END;
/
