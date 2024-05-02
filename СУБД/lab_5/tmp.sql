CREATE TABLE LastOtchet (
    TableName VarChar2(40) PRIMARY KEY,
    Time TIMESTAMP
);


INSERT INTO LastOtchet (TableName) VALUES ('Tab1');
INSERT INTO LastOtchet (TableName) VALUES ('Tab2');
INSERT INTO LastOtchet (TableName) VALUES ('Tab3');



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