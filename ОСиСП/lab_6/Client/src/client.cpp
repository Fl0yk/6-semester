#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <pthread.h>

#define MAX_MESSAGE_LENGTH 256


int main(int argc, char *argv[])
{
    int sock;
    struct sockaddr_in server;
    char message[MAX_MESSAGE_LENGTH];

    if(argc < 3)
    {
        printf("Usage: %s <ip address> <port>\n", argv[0]);
        exit(1);
    }

    //Create socket
    if((sock = socket(AF_INET, SOCK_STREAM, 0)) == -1)
    {
        perror("Could not create socket");
        exit(1);
    }

    server.sin_addr.s_addr = inet_addr(argv[1]);
    server.sin_family = AF_INET;
    server.sin_port = htons(atoi(argv[2]));

    //Connect to remote server
    if(connect(sock, (struct sockaddr *)&server, sizeof(server)) < 0)
    {
        perror("Connect failed");
        exit(1);
    }

    puts("Connected to server");

    while(1)
    {
        printf("Enter message: ");
        fgets(message, MAX_MESSAGE_LENGTH, stdin);

        if(send(sock, message, strlen(message), 0) < 0)
        {
            perror("Send failed");
            exit(1);
        }

        if(recv(sock, message, MAX_MESSAGE_LENGTH, 0) <= 0) 
        {
            perror("Answer failed");
            exit(1);
        }

        printf("Received message: %s\n", message);
    }

    close(sock);

    return 0;
}