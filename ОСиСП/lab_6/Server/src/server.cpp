#include <stdio.h>
#include <string.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <pthread.h>

#include "functions.h"

#define MAX_CLIENTS 100
#define BUF_SIZE 2000

void *handle_client(void *arg) {
    int socket = *((int *)arg);
    char buf[BUF_SIZE];
    int len;

    while((len = recv(socket, buf, BUF_SIZE, 0)) > 0) 
    {
        buf[len] = '\0';
        std::string answ = encode(std::string(buf));
        send(socket, answ.c_str(), answ.length(), 0);
    }

    close(socket);

    return NULL;
}

int main(int argc, char *argv[]) {
    int server_sock, client_sock, c;
    struct sockaddr_in server, client;
    pthread_t client_thread;

    if(argc != 2) {
        printf("Usage: %s <port>\n", argv[0]);
        return 1;
    }

    // Создаем интернетный потоковый сокет
    server_sock = socket(AF_INET, SOCK_STREAM, 0);
    if(server_sock < 0) {
        printf("Could not create socket");
        return 1;
    }

    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;
    // Чтоб наверняка перевести в big endian
    server.sin_port = htons(atoi(argv[1]));

    // Привязываем сокет к адресу
    if(bind(server_sock, (struct sockaddr *)&server, sizeof(server)) < 0) {
        printf("Bind failed");
        return 1;
    }

    listen(server_sock, 3);

    printf("Waiting for incoming connections...\n");
    c = sizeof(struct sockaddr_in);

    // При подключении клиента заносим в переменную дескриптор файла
    while((client_sock = accept(server_sock, (struct sockaddr *)&client, (socklen_t*)&c))) {
        printf("Connection accepted\n");

        int *new_sock = (int *)malloc(sizeof(int));
        *new_sock = client_sock;
        
        if (pthread_create(&client_thread, NULL, handle_client, (void *)new_sock) < 0)
        {
            printf("Error on create thread\n");
            return 1;
        }
    }

    if(client_sock < 0) {
        printf("Accept failed");
        return 1;
    }

    return 0;
}