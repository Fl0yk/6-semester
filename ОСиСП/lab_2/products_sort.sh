#!/bin/bash
#Формат входных данных:
#   наименование товара в кавычках
#   все поля разделяются |
#   без лишних пробелов и т.п.

#Формат выходных данных:
#   наименование|общее кол-во|средняя цена|суммарная стоимость

FILE_NAME="tmp.txt"
PRODUCT_PATTERN='^"[A-Za-z0-9 .]+" | [[:digit:]]+ | [[:digit:]]+$'

function delete_tmp {
    #Проверяем на любой тип файла
    if [ -e "$FILE_NAME" ]
    then
        rm "$FILE_NAME"
        
    fi
}

echo "Start script $0"

#Проверяем, есть ли временный файл. Если есть, то удаляем и создаем заново
delete_tmp
touch "$FILE_NAME"

#Берем последний байт файла. Если это переход на новую строку,
#то результат равен 1. Если нету перехода на новую строку, то 0
[[ $(tail -c 1 products1.txt | wc -l) -eq 0 ]] && echo >> products1.txt

#Затем соединяем оба файла во временный и сортируем
cat products1.txt prodicts2.txt | sort >> "$FILE_NAME"

#awk работает только со строками, соответствующими шаблону
#Изначально устанавливаем в качестве разделителя | и
#счетчик одного продукта равного нулю
awk 'BEGIN{
    FS="\|"; 
    total_count=0; 
    last_prod="";
    count=0;
    sum_price=0;
    total_price=0;
    print "Name|Count|Avg price|Sum price";
    }/^"[A-Za-z0-9 .]+"\|[[:digit:]]+\|[[:digit:]]+(,[0-9]?[0-9]?)?$/{
        if (last_prod != "" && last_prod != $1)
        {
            avg_price = sum_price / count;
            print last_prod "|" total_count "|" avg_price "|" total_price;
            total_count = 0;
            sum_price = 0;
            total_price = 0;
            count = 0;
            last_prod = "";
        }
        last_prod = $1;
        total_count = total_count + $2;
        sum_price = sum_price + $3;
        total_price = total_price + $2 * $3;
        count++;
    }
    END{
        avg_price = sum_price / count;
        print last_prod, "|", total_count, "|", avg_price, "|", total_price;
    }' "$FILE_NAME" > "result.txt"

delete_tmp

echo "End script. Result in \"result.txt\""

exit 0