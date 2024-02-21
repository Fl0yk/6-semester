#!/bin/bash

#объявляем переменную массивом (она будет глобальной)
declare -a board=(" " " " " " " " " " " " " " " " " ")
declare -i turn_id

function try_win {
    local marker="X"
    echo win
    # Попытка победить
    for (( k=0; k<9; k++ ))
    do
        if [ "${board[$k]}" == " " ]
        then
            board[$k]="X" 
            check_win "X"
            tmp=$?
            
            if [[ $tmp -eq 0 ]]
            then
                board[$k]=$marker
                #echo "can win"
                return
            else
                board[$k]=" "
            fi
        fi
    done
    return 1
}

function try_block {
    local marker="X"
    #echo block
    # Попытка заблокировать игрока
    for (( k=0; k<9; k++ ))
    do
        if [ "${board[$k]}" == " " ]
        then
            board[$k]="O" 
            check_win "O"
            tmp=$?
            
            if [[ $tmp -eq 0 || $tmp -eq 2 ]]
            then
                board[$k]=$marker
                return
            else
                board[$k]=" "
            fi
        fi
    done
    return 1
}

function rand_turn {
    local marker="X"
    echo rand
    if [ "${board[4]}" == " " ]
    then
        board[4]=$marker
        return
    fi
    rand_id=$RANDOM
    let "number %= 9"
    #let "number += 1"
    while [ "${board[$rand_id]}" != " " ]
    do
        rand_id=$RANDOM
        let "number %= 9"
        #let "number += 1"
    done

    board[$rand_id]=$marker
}

function print_board {
    echo " ${board[0]} | ${board[1]} | ${board[2]} "
    echo "---|---|---"
    echo " ${board[3]} | ${board[4]} | ${board[5]} "
    echo "---|---|---"
    echo " ${board[6]} | ${board[7]} | ${board[8]} "
    echo "---|---|---"
}

function make_screenshot {
    echo " ${board[0]} | ${board[1]} | ${board[2]} " > result.txt
    echo "---|---|---"  >> result.txt
    echo " ${board[3]} | ${board[4]} | ${board[5]} "  >> result.txt
    echo "---|---|---"  >> result.txt
    echo " ${board[6]} | ${board[7]} | ${board[8]} "  >> result.txt
    echo "---|---|---"  >> result.txt
}

#Функция вывода сообщения при окончании игры
function end_game {
    local message=$1
    clear
    echo " $message"
    print_board
    make_screenshot
    #Используется для завершения сценария
    exit
}

#В функцию передается символ (X или O)
function check_win {
    local marker=$1
    
    #Проверяем строки
    for((i=0; i < 3; i++ )) 
    do
        if [[ ${board[$((i*3))]} == $marker && ${board[$((i*3+1))]} == $marker && ${board[$((i*3+2))]} == $marker ]]
        then
            return 0
        fi
    done

    #Проверяем столбики
    for((i=0; i < 3; i++ )) 
    do
        if [[ ${board[$((i))]} == $marker && ${board[$((i+3))]} == $marker && ${board[$((i+6))]} == $marker ]]
        then
            return 0
        fi
    done

    #Проверяем диагонали
    if [[ ${board[$((0))]} == $marker  &&  ${board[$((4))]} == $marker  &&  ${board[$((8))]} == $marker ]]
    then
        return 0
    fi

    if [[ ${board[$((2))]} == $marker && ${board[$((4))]} == $marker && ${board[$((6))]} == $marker ]]
    then
        return 0
    fi

    #Проверка на ничью(должна быть хоть одна пустая ячейка)
    for cell in "${board[@]}"
    do
        if [ "$cell" == " " ]
        then
            return 1
        fi
    done

    #Раз нет пустых и никто не выйграл, то ничья поучается
    #end_game "Ничья!"
    return 2
}

function take_turn {
    local marker=$1

    read -p "Игрок, выберите ход(1-9): " position

    #Проверяем регуляркой, что ввели цифру
    if [[ ! $position =~ ^[1-9]$ ]] || [[ ${board[$((position-1))]} != " " ]]
    then
        echo "Некорректный ввод. Повторите попытку."
        take_turn $player $marker
    else
        board[$((position-1))]=$marker
    fi
}

function bot_turn {
    try_win || try_block || rand_turn
        
    check_win "X"
    tmp=$?
    if [ $tmp -eq 0 ]
    then
        end_game "Победил бот"
    fi
    if [ $tmp -eq 2 ]
    then
        end_game "Ничья"
    fi
}

function player_turn {
    take_turn "O"
    check_win "O"
    tmp=$?
    if [ $tmp -eq 0 ]
    then
        end_game "Победил человек"
    fi
    if [ $tmp -eq 2 ]
    then
        end_game "Ничья"
    fi
}

function game {
    turn_id=$RANDOM
    let "turn_id %= 2"

    while true
    do
        clear
        print_board
        make_screenshot

        if ((turn_id % 2 == 0))
        then
            player_turn
        else
            bot_turn
        fi
        let "turn_id += 1"
    done
}


game