TARGET = Morse

PREF_SRC = ./src/
PREF_OBJ = ./obj/
PREF_TEST = ./tests/

SRC = $(wildcard $(PREF_SRC)*.cpp)
OBJ = $(patsubst $(PREF_SRC)%.cpp, $(PREF_OBJ)%.o, $(SRC))

TST_COR = tst_cor_
TST_RES = tst_res_
TST_TEXT = tst_text_

$(TARGET) : $(OBJ)
	$(CXX) $(OBJ) -o $(TARGET)

$(PREF_OBJ)%.o : $(PREF_SRC)%.cpp
	$(CXX) -c $< -o $@

clean :
	rm $(TARGET) $(PREF_OBJ)*.o

clean_test:
	rm $(PREF_TEST)*.txt

tests : test1 test2

test1 :
	echo SOS > $(PREF_TEST)$(TST_TEXT)1.txt
	echo "Encoded message: ... --- ... " > $(PREF_TEST)$(TST_COR)1.txt
	./Morse $(PREF_TEST)$(TST_RES)1.txt < $(PREF_TEST)$(TST_TEXT)1.txt
	diff $(PREF_TEST)$(TST_RES)1.txt $(PREF_TEST)$(TST_COR)1.txt
	rm $(PREF_TEST)*.txt

test2 :
	echo "Hello. My name is Tom, I have many friends." > $(PREF_TEST)$(TST_TEXT)2.txt
	echo "Encoded message: .... . .-.. .-.. ---/-- -.--/-. .- -- ./.. .../- --- --/../.... .- ...- ./-- .- -. -.--/..-. .-. .. . -. -.. ... " > $(PREF_TEST)$(TST_COR)2.txt
	./Morse $(PREF_TEST)$(TST_RES)2.txt < $(PREF_TEST)$(TST_TEXT)2.txt
	diff $(PREF_TEST)$(TST_RES)2.txt $(PREF_TEST)$(TST_COR)2.txt
	rm $(PREF_TEST)*.txt