CREATE OR REPLACE TRIGGER check_student_id
FOR INSERT OR UPDATE ON Students
FOLLOWS generete_student_id
COMPOUND TRIGGER

TYPE t_student_id IS TABLE OF NUMBER INDEX BY PLS_INTEGER;
v_student_id t_student_id;

BEFORE STATEMENT IS
BEGIN
    v_student_id.DELETE;
END BEFORE STATEMENT;

BEFORE EACH ROW IS
BEGIN
    IF INSERTING OR UPDATING THEN
        dbms_output.put_line('before each row');
        v_student_id(v_student_id.COUNT + 1) := :NEW.id;
    END IF;
END BEFORE EACH ROW;

AFTER STATEMENT IS
    v_id_count NUMBER;
    id_exist_ex EXCEPTION;
BEGIN
    dbms_output.put_line('after statement');
    FOR i IN 1 .. v_student_id.COUNT LOOP
        SELECT COUNT(*)
        INTO v_id_count
        FROM Groups
        WHERE id = v_student_id(i);

        IF v_id_count > 1 THEN
            dbms_output.put_line('Count of this id  ' || v_id_count);
            dbms_output.put_line('This student id exists ' || v_student_id(i));
            RAISE id_exist_ex;
        END IF;
    END LOOP;
END AFTER STATEMENT;

END check_student_id;

------------------------------------------------------------

CREATE OR REPLACE TRIGGER check_group_id
FOR INSERT OR UPDATE ON Groups
FOLLOWS generete_group_id
COMPOUND TRIGGER

TYPE t_group_id IS TABLE OF NUMBER INDEX BY PLS_INTEGER;
v_group_id t_group_id;

BEFORE STATEMENT IS
BEGIN
    v_group_id.DELETE;
END BEFORE STATEMENT;

BEFORE EACH ROW IS
BEGIN
    IF INSERTING OR UPDATING THEN
        dbms_output.put_line('before each row');
        v_group_id(v_group_id.COUNT + 1) := :NEW.id;
    END IF;
END BEFORE EACH ROW;

AFTER STATEMENT IS
    v_id_count NUMBER;
    id_exist_ex EXCEPTION;
BEGIN
    dbms_output.put_line('after statement');
    FOR i IN 1 .. v_group_id.COUNT LOOP
        SELECT COUNT(*)
        INTO v_id_count
        FROM Groups
        WHERE id = v_group_id(i);

        IF v_id_count <> 1 THEN
            dbms_output.put_line('This group id exists ' || v_group_id(i));
            RAISE id_exist_ex;
        END IF;
    END LOOP;
END AFTER STATEMENT;

END check_group_id;

---------------------------------------------------------------------------

CREATE OR REPLACE TRIGGER check_group_name
FOR INSERT OR UPDATE ON Groups
COMPOUND TRIGGER

--Создаем тип таблицы, в которой будут храниться имена новых пользователей
TYPE t_group_names IS TABLE OF VARCHAR2(40) INDEX BY PLS_INTEGER;
v_group_names t_group_names;

BEFORE STATEMENT IS
BEGIN
    v_group_names.DELETE;
END BEFORE STATEMENT;

BEFORE EACH ROW IS
BEGIN
    IF INSERTING OR UPDATING THEN
        --Добавляем имя в массив для последующей проверки
        v_group_names(v_group_names.COUNT + 1) := :NEW.name;
    END IF;
END BEFORE EACH ROW;

AFTER STATEMENT IS
    v_name_count NUMBER;
    name_exist_ex EXCEPTION;
BEGIN
    -- Проверяем наличие каждого имени группы в базе данных
    FOR i IN 1 .. v_group_names.COUNT LOOP
        SELECT COUNT(*)
        INTO v_name_count
        FROM Groups
        WHERE name = v_group_names(i);

        IF v_name_count <> 1 THEN
            dbms_output.put_line('This group name exists ' || v_group_names(i));
            RAISE name_exist_ex;
        END IF;
    END LOOP;
END AFTER STATEMENT;

END check_group_name;

