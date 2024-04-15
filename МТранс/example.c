void printMessage() {
    printf("Пример использования различных типов данных и массивов в C\n");
}

int main() {
    char charVar = 'A';
    int intArray[5] = {1, 2, 3, 4, 5};
    short shortArray[3] = {10, 20, 30};
    long longVar = 1234567890;
    float floatArray[4] = {1.1, 2.2, 3.3, 4.4};
    double doubleArray[2] = {5.5, 6.6};
    printMessage();

    printf("charVar: %c\n", charVar);

    printf("intArray: ");
    for (int i = 0; i < 5; i++) {
        printf("%d ", intArray[i]);
    }
    printf("\n");

    printf("shortArray: ");
    for (int i = 0; i < 3; i++) {
        printf("%d ", shortArray[i]);
    }
    printf("\n");

    printf("longVar: %ld\n", longVar);

    printf("floatArray: ");
    for (int i = 0; i < 4; i++) {
        printf("%f ", floatArray[i]);
    }
    printf("\n");

    printf("doubleArray: ");
    for (int i = 0; i < 2; i++) {
        printf("%lf ", doubleArray[i]);
    }
    printf("\n");

    return 0;
}
