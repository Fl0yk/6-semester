#!/bin/bash

#объявляем переменную массивом (она будет глобальной)
declare -a board=(" " " " " " " " " " " " " " " " " ")

function block_player {
    local marker="X"

    # Попытка заблокировать игрока
    for (( k=0; k<9; k++ ))
    do
        if [ "${board[$k]}" == " " ]
        then
            board[$k]="O" 
            check_win 1 "O"
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

    if [ "${board[4]}" == " " ]
    then
        board[4]=$marker
        return
    fi
    rand_id=$RANDOM
    let "number %= 8"
    let "number += 1"
    while [ "${board[$rand_id]}" != " " ]
    do
        rand_id=$RANDOM
        let "number %= 8"
        let "number += 1"
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

#В функцию передается номер игрока и его символ (X или O)
function check_win {
    local player=$1
    local marker=$2
    
    #Проверяем строки
    for((i=0; i < 3; i++ )) 
    do
        if [[ ${board[$((i*3))]} == $marker && ${board[$((i*3+1))]} == $marker && ${board[$((i*3+2))]} == $marker ]]
        then
            #end_game "Победил игрок $player"
            return 0
        fi
    done

    #Проверяем столбики
    for((i=0; i < 3; i++ )) 
    do
        if [[ ${board[$((i))]} == $marker && ${board[$((i+3))]} == $marker && ${board[$((i+6))]} == $marker ]]
        then
            #end_game "Победил игрок $player"
            return 0
        fi
    done

    #Проверяем диагонали
    if [[ ${board[$((0))]} == $marker  &&  ${board[$((4))]} == $marker  &&  ${board[$((8))]} == $marker ]]
    then
        #end_game "Победил игрок $player"
        return 0
    fi

    if [[ ${board[$((2))]} == $marker && ${board[$((4))]} == $marker && ${board[$((6))]} == $marker ]]
    then
        #end_game "Победил игрок $player"
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
    local player=$1
    local marker=$2

    read -p "Игрок $player, выберите ход(1-9): " position

    #Проверяем регуляркой, что ввели цифру
    if [[ ! $position =~ ^[1-9]$ ]] || [[ ${board[$((position-1))]} != " " ]]
    then
        echo "Некорректный ввод. Повторите попытку."
        take_turn $player $marker
    else
        board[$((position-1))]=$marker
        #make_screenshot
        #check_win $player $marker
    fi
}

function game {
    while true
    do
        clear
        print_board
        make_screenshot

        #Ход компьютера
        block_player || rand_turn
        
        check_win 1 "X"
        tmp=$?
        if [ $tmp -eq 0 ]
        then
            end_game "Победил бот"
        fi
        if [ $tmp -eq 2 ]
        then
            end_game "Ничья"
        fi

        clear
        print_board
        make_screenshot

        #Ход игрока
        take_turn 2 "O"
        check_win 2 "O"
        tmp=$?
        if [ $tmp -eq 0 ]
        then
            end_game "Победил человек"
        fi
        if [ $tmp -eq 2 ]
        then
            end_game "Ничья"
        fi
    done
}


game