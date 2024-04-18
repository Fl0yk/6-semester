DROP TABLE Test2;
DROP TABLE Test1;

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    json_data := '{
    "type": "INSERT",
    "table": "Test1",
    "columns": [
        "Name"
    ],
    "values": [
        [
            "t2"
        ]
    ]
}'; 

    JSON_PARSE(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(50);
BEGIN
    json_data := '{
    "type": "SELECT",
    "columns": [
        "Test1.Id",
        "Test2.Name"
    ],
    "tables": [
        "Test1"
    ],
    "joins": [
        {
            "table": "Test2",
            "condition": [
                "Test1.ID = Test2.Test1ID"
            ]
        }
    ]
}'; 

    JSON_PARSE(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(20);
    price INT;
BEGIN
    json_data := '{
    "type": "SELECT",
    "columns": [
        "*"
    ],
    "tables": [
        "Products"
    ],
    "filters":{
        "type": "WHERE",
        "operator": "AND",
        "body":[
            "Price < 110",
            {
                "type": "NOT EXISTS",
                "body": {
                    "condition":{
                        "type": "SELECT",
                        "columns": "ProductId",
                        "tables": "Orders",
                        "filters":{
                            "type": "WHERE",
                            "body":[
                                "Products.Id = Orders.ProductId"
                            ]
                        }
                    }
                }
            }
        ]
    }
}'; 

    JSON_PARSE(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name, price;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name || ', ' || price);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/
