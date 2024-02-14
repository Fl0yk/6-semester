DROP TABLE MyTable;

/*Задание 1*/
/*Созадем таблицу с id и val*/
CREATE TABLE MyTable(
    id NUMBER PRIMARY KEY,
    val NUMBER NOT NULL
);


/*Задание 2*/
/*Пишем анонимный блок для заполнение таблицы*/
/*В цикле вставляем индекс цикла как id и генерируем случайное целое число*/
BEGIN
    FOR i IN 1..10001 LOOP
        INSERT INTO MyTable(id, val) VALUES (i, ROUND(DBMS_RANDOM.VALUE(1, 100000)));
    END LOOP;
END;

/*Для проверки выводим первые 10*/
SELECT * FROM MyTable
WHERE id < 10;


/*Задание 3*/
/*Создаем функцию ,которая проверяет, больше ли четных чисел*/
/*Возвращает значение строкой*/
CREATE OR REPLACE FUNCTION CompareEvenAndOd RETURN VARCHAR2 IS
    even_count NUMBER;
    odd_count NUMBER;
BEGIN
    SELECT COUNT(1)
    INTO even_count
    FROM MyTable
    WHERE MOD(val, 2) = 0;

    SELECT COUNT(1)
    INTO odd_count
    FROM MyTable
    WHERE MOD(val, 2) = 1;

    IF even_count > odd_count THEN
        RETURN 'TRUE';
    ELSIF even_count < odd_count THEN
        RETURN 'FALSE';
    ELSE
        RETURN 'EQUAL';
    END IF;
END CompareEvenAndOd;

/*Для проверки функции*/
SELECT * FROM MyTable;
DELETE FROM MyTable;
BEGIN
    FOR i IN 1..10 LOOP
        INSERT INTO MyTable(id, val) VALUES (i, ROUND(DBMS_RANDOM.VALUE(1, 100000)));
    END LOOP;
    
    dbms_output.put_line(CompareEvenAndOd());
END;


/*Задание 4*/
/*Выводит строку для вставки в таблицу значения с конкретным id*/
CREATE OR REPLACE FUNCTION GetInsertCommand(p_id INTEGER) RETURN STRING IS
    v_val NUMBER;
BEGIN
    SELECT val INTO v_val FROM MyTable WHERE id = p_id;

    RETURN 'INSERT INTO MyTable(id, val) VALUES ('|| p_id ||', '|| v_val ||');';
END;

/*Для проверки*/
BEGIN
    dbms_output.put_line(GetInsertCommand(12));
END;


/*Задание 5*/
/*Вставка, изменение, удаление при помощи процедур*/
CREATE OR REPLACE PROCEDURE InsertVal(p_id INTEGER, p_val INTEGER) IS
BEGIN
    INSERT INTO MyTable(id, val) VALUES (p_id, p_val);
END InsertVal;

CREATE OR REPLACE PROCEDURE UpdateVal(p_id INTEGER, p_val INTEGER) IS
BEGIN
    UPDATE MyTable SET val = p_val WHERE id = p_id;
END UpdateVal;

CREATE OR REPLACE PROCEDURE DeleteVal(p_id INTEGER) IS
BEGIN
    DELETE FROM MyTable WHERE id = p_id;
END DeleteVal;

/*Для проверки*/
EXEC InsertVal(5, 15);
SELECT * FROM MyTable;
EXEC UpdateVal(5, 20);
SELECT * FROM MyTable;
EXEC DeleteVal(5);
SELECT * FROM MyTable;


/*Задание 6*/
/*Считаем общее вознаграждение за год по формуле:*/
/*(1+процент_премиальных)*12*значение_зп*/
/*Проверить переданные проценты, чтоб были целыми и от 0 до 100*/
CREATE OR REPLACE FUNCTION RewardForYear(p_salary NUMBER, p_bonus INTEGER) RETURN NUMBER
IS
    salary_ex EXCEPTION;
    bonus_exception EXCEPTION;
BEGIN
    IF p_bonus < 0 OR p_bonus > 100 THEN
        RAISE bonus_exception;
    END IF;
    IF p_salary < 0 THEN
        RAISE salary_ex;
    END IF;

    RETURN (1 + p_bonus / 100) * 12 * p_salary;
EXCEPTION
    WHEN bonus_exception THEN
        dbms_output.put_line('Некорректное значение процента');5
        RAISE bonus_exception;
    WHEN salary_ex THEN
        dbms_output.put_line('Некорректное значение зарплаты');5
        RAISE salary_ex;
END;

/*Для проверки*/
BEGIN
    dbms_output.put_line(RewardForYear(1200, 8));
END;