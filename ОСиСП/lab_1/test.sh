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


function robot_turn{
    if [ $turn_id -eq 1 ]
    then
        board[0]="X"
        (( turn_id++ ))
    elif [ $turn_id -eq 2 ]
    then
        if [ ${board[8]} != " " ]
        then
            board[2]="X"
        else
            board[8]="X"
        fi
    elif [ $turn_id -eq 3 ]
    #Возможные ходы человека: 1 2 3 5 6 7
        if [ ${board[1]} != " " ]
        then
            board[7]="X"
        elif [ ${board[2]} != " " ]
            board[6]="X"
        elif [ ${board[3] != " "} ]
            board[5]="X"
        elif [ ${board[5] != " "} ]
            board[3]="X"
        elif [ ${board[6] != " "} ]
            board[2]="X"
        elif [ ${board[7] != " "} ]
            board[1]="X"
        fi
    fi
}