#include "mainwindow.h"
#include "./ui_mainwindow.h"

MainWindow::MainWindow(QWidget *parent)
    : QWidget(parent)
    , ui(new Ui::MainWindow)
{

    setWindowFlags(Qt::Window | Qt::FramelessWindowHint);
    setAttribute(Qt::WA_TranslucentBackground);
    setAttribute(Qt::WA_NoSystemBackground, false);
    setAutoFillBackground(true);

    // Создание кнопок цвета
    for (int i = 0; i < 6; ++i) {
        colorButtons[i] = new QPushButton(this);
        colorButtons[i]->setGeometry(10, 30 + i * 30, 130, 25);

        // Установка цвета кнопки
        switch (i) {
        case 0:
            colorButtons[i]->setText("Белый");
            colorButtons[i]->setStyleSheet("background-color: white");
            break;
        case 1:
            colorButtons[i]->setText("Черный");
            colorButtons[i]->setStyleSheet("background-color: black");
            break;
        case 2:
            colorButtons[i]->setText("Синий");
            colorButtons[i]->setStyleSheet("background-color: blue");
            break;
        case 3:
            colorButtons[i]->setText("Красный");
            colorButtons[i]->setStyleSheet("background-color: red");
            break;
        case 4:
            colorButtons[i]->setText("Желтый");
            colorButtons[i]->setStyleSheet("background-color: yellow");
            break;
        case 5:
            colorButtons[i]->setText("Зеленый");
            colorButtons[i]->setStyleSheet("background-color: green");
            break;
        }

        // Подключение к сигналу нажатия кнопки
        connect(colorButtons[i], &QPushButton::clicked, this, &MainWindow::handleColorChange);
    }

    // Установка начального цвета (белый)
    currentColor = Qt::white;

    QPalette pal = palette();
    pal.setColor(QPalette::Window, Qt::blue); // Замените Qt::blue на нужный цвет
    setPalette(pal);

    //ui->setupUi(this);
}

void MainWindow::showEvent(QShowEvent *event) {
    QWidget::showEvent(event);
    setGeometry(QGuiApplication::primaryScreen()->geometry());
}

void MainWindow::handleColorChange(const QColor& color) {
    currentColor = color;

    // Заливка экрана выбранным цветом
    QPalette palette = QApplication::palette();
    palette.setColor(QPalette::Window, currentColor);
    QApplication::setPalette(palette);
}

MainWindow::~MainWindow()
{
    delete ui;
}
