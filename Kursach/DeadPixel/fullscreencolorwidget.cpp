#include "fullscreencolorwidget.h"

FullScreenColorWidget::FullScreenColorWidget(QWidget *parent) : QWidget(parent) {
    setWindowFlags(Qt::Window | Qt::FramelessWindowHint); // Отображение без границ и заголовка
    setAttribute(Qt::WA_TranslucentBackground); // Делаем фон прозрачным
    setAttribute(Qt::WA_NoSystemBackground, false); // Фон не будет системным (не обязательно)
    setAutoFillBackground(true); // Включаем автоматическое заполнение фона

    // Задаем цвет фона по умолчанию
    setBackgroundColor(Qt::white);
}

void FullScreenColorWidget::setBackgroundColor(const QColor &color) {
    QPalette pal = palette();
    pal.setColor(QPalette::Window, color);
    setPalette(pal);
}

void FullScreenColorWidget::showEvent(QShowEvent *event) {
    QWidget::showEvent(event);
    setGeometry(QGuiApplication::primaryScreen()->geometry()); // Устанавливаем размеры по размерам основного экрана
}
