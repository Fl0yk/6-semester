#include <iostream>
#include <pthread.h>
#include <semaphore.h>
#include <sys/time.h>
#include <sys/sem.h>
#include <sys/types.h>
#include <sys/ipc.h>
#include <unistd.h>

using namespace std;

// Функция, которая является критическим ресурсом для синхронизации
void critical_function(int thread_id) {
    cout << "Thread " << thread_id << " accessed critical resource." << endl;
}

// Получение времени в секундах с точностью до микросекунд
double get_time() {
    // Описывает структуру с секундами и микросекундами
    struct timeval tv;
    gettimeofday(&tv, NULL);

    // Складываем секудны и микросекунды, переведенные в секунды
    return tv.tv_sec + tv.tv_usec / 1000000.0;
}

// Потоковая функция для мьютекса
void* mutex_thread(void* arg) {
    pthread_mutex_t* mutex = (pthread_mutex_t*)arg;
    pthread_t tid = pthread_self();
    
    //Блокировка: теперь к ресурсам имеет доступ только один
    //поток, который владеет мьютексом. Он же единственный, 
    //кто может его разблокировать
    pthread_mutex_lock(mutex);
    critical_function(tid);
    pthread_mutex_unlock(mutex);

    return NULL;
}

// Потоковая функция для POSIX семафора
void* sem_posix_thread(void* arg) {
    sem_t* sem = (sem_t*)arg;
    pthread_t tid = pthread_self();

    // Выполнение операции блокировки семафора
    sem_wait(sem);
    critical_function(tid);
    // Освобождение семафора
    sem_post(sem);

    return NULL;
}

// Потоковая функция для System V семафора
void* sem_sysv_thread(void* arg) {
    int sem_id = *(int*)arg;
    pthread_t tid = pthread_self();

    struct sembuf op;
    op.sem_num = 0;
    op.sem_op = -1; // Операция семафора
    op.sem_flg = 0;
    semop(sem_id, &op, 1);

    critical_function(tid);

    op.sem_op = 1;
    semop(sem_id, &op, 1);

    return NULL;
}

int main(int argc, char* argv[]) {
    int num_threads = 20; // Default number of threads

    if (argc > 1) {
        num_threads = atoi(argv[1]);
    }

    pthread_t threads[num_threads];
    int thread_ids[num_threads];

    // Mutex test
    cout << "Start Mutex test\n";
    // Статическое создание мьютекса по умолчанию при помощи макроса
    pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
    double mutex_start = get_time();
    for (int i = 0; i < num_threads; ++i) {
        thread_ids[i] = i;
        pthread_create(&threads[i], NULL, mutex_thread, &mutex);
    }
    for (int i = 0; i < num_threads; ++i) {
        pthread_join(threads[i], NULL);
    }
    double mutex_end = get_time();
    cout <<"End Mutex test\n\n";

    // POSIX semaphore test
    cout <<"Start POSIX semaphore test\n";
    sem_t sem_posix;
    sem_init(&sem_posix, 0, 1);
    double sem_posix_start = get_time();
    for (int i = 0; i < num_threads; ++i) {
        thread_ids[i] = i;
        pthread_create(&threads[i], NULL, sem_posix_thread, &sem_posix);
    }
    for (int i = 0; i < num_threads; ++i) {
        pthread_join(threads[i], NULL);
    }
    double sem_posix_end = get_time();
    cout <<"End POSIX semaphore test\n\n";

    // System V semaphore test
    cout <<"Start System V semaphore test\n";
    // Создаем IPC ключ. s - ключ для идентификации System V ресура
    key_t key = ftok(".", 's');
    // Создаем семафор, если он еще не создан, и допускаем чтение и запись 
    int sem_id = semget(key, 1, IPC_CREAT | 0666);
    semctl(sem_id, 0, SETVAL, 1);
    double sem_sysv_start = get_time();
    for (int i = 0; i < num_threads; ++i) {
        thread_ids[i] = i;
        pthread_create(&threads[i], NULL, sem_sysv_thread, &sem_id);
    }
    for (int i = 0; i < num_threads; ++i) {
        pthread_join(threads[i], NULL);
    }
    double sem_sysv_end = get_time();
    cout <<"End System V semaphore test\n\n";

    cout << "Mutex time: " << mutex_end - mutex_start << endl;
    cout << "POSIX semaphore time: " << sem_posix_end - sem_posix_start << endl;
    cout << "System V semaphore time: " << sem_sysv_end - sem_sysv_start << endl;

    // Clean up System V semaphore
    semctl(sem_id, 0, IPC_RMID);

    return 0;
}
