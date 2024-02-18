
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

CREATE SEQUENCE group_id_seq
START WITH 1
INCREMENT BY 1;



--Task 3--


--Task 4--


--Task 5--


--Task 6--