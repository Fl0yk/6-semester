declare -a products
FILE_NAME="tmp.txt"
PRODUCT_PATTERN='^"[A-Za-z0-9 .]+" | [[:digit:]]+ | [[:digit:]]+$'

#Проверяет все строки файла на совпадение с шаблоном
#Название(в кавычках), количество(целое число), цена(число с точкой)
function check_file {

    awk 'BEGIN{FS="\|";}/^"[A-Za-z0-9 .]+"\|[[:digit:]]+\|[[:digit:]]+$/{
        print $1; 
        print $2
        }' products1.txt
}

#После проверки файла добавляет все строки во временный файл
#По окончанию функции в файле должны быть отсортированные продукты
function concat_files {
    echo a
}

#Проходит по временному файлу, ищет дубликаты,
#считает среднюю стоимость, кол-во и добавляет в итоговую коллекцию
function find_duplicate {
    echo a
}

#Выводит итоговую коллекцию продуктов
function print_result {
    echo a
}

echo start
check_file