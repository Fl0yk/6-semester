int main() {
    int whileCounter = 0;
    while (whileCounter < 5) {
        printf("Цикл while: %d\n", whileCounter);
        whileCounter++;
    }

    for (int forCounter = 0; forCounter < 5; forCounter++) {
        printf("Цикл for: %d\n", forCounter);
    }

    int doWhileCounter = 0;
    do {
        printf("Цикл do-while: %d\n", doWhileCounter);
        doWhileCounter++;
    } while (doWhileCounter < 5);

    int num = 10;
    if (num > 0) {
        printf("Число положительное\n");
    } else if (num < 0) {
        printf("Число отрицательное\n");
    } else {
        printf("Число равно нулю\n");
    }

    char grade = 'B';
    switch (grade) {
        case 'A':
            printf("Отлично!\n");
            break;
        case 'B':
            printf("Хорошо!\n");
            break;
        case 'C':
            printf("Удовлетворительно\n");
            break;
        default:
            printf("Неизвестная оценка\n");
    }

    for (int i = 0; i < 10; i++) {
        if (i == 5) {
            break;
        }
        if (i % 2 == 0) {
            continue; 
        }
        printf("Текущее значение i: %d\n", i);
    }

    return 0;
}
