#include <iostream>
#include <fstream>
#include <sys/types.h>
#include <unistd.h>
#include <csignal>

void sigHandler(int signum);

int main()
{
    if (signal(SIGHUP, sigHandler) == SIG_ERR)
    {
        std::cout << "Ошибка при привязке обработчика сигнала\n";
        return 0;
    }

    std::fstream out; // поток для записи
    out.open("process.txt", std::ios::out);

    if(!out)
    {
        std::cout << "File not exist and can't create" << std::endl;
        return 0;
    }

    for(int i = 0; i < 10; i++)
    {
        if(i == 4)
        {
            raise(SIGHUP);
        }
        sleep(1);
        out << i <<  std::endl;
    }

    out.close();
}

void sigHandler(int signum)
{
    std::cout <<"Получили сигнал " <<signum <<std::endl;
    pid_t pid;

    //Создаем идентичный процесс при помощи fork
    switch(pid = fork())
    {
        // При вызове fork() возникла ошибка
        case -1:
            std::cout <<"Ошибка при создании процесса.\n";
            break;
        // Это код потомка
        case 0 :
            std::cout <<"Процесс успешно создан\n";
            break;
        // Это код родительского процесса
        default :
            std::cout << "Родитель прекращает работу\n";
            exit(EXIT_SUCCESS);
            break;
    }


}