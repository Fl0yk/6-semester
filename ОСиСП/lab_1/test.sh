val=3
val2=-3
sum=$(( $val+$val2 ))

if [[ $val+$val2 ]]
then
    echo "Ok"
    echo "$sum"
else
    echo "${val[1]}"
fi

function is_ok {
    return 1
}

is_ok

a=$(( ! true ))
echo $a

if ! true
then
    echo "is ok. Res: $is_ok"
fi

#Проверяем, есть ли файл и обычный ли он
#Второе, что файл не пустой
if [[ -f ./test.txt && -s ./test.txt ]]
then
    echo "Est"
fi

text= cat "test.txt"
echo "${text[0]}"

function get_data {
    str=$1
    name="Abobus"
    count=12
    return 0
}

if get_data
then
    echo $name $count
else
    echo Err
    exit
fi
echo ================
patter='^"[A-Za-z ]+" [[:digit:]]+$'
awk "/$patter/" test.txt