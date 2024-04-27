#include "colorselectionwidget.h"

ColorSelectionWidget::ColorSelectionWidget(QWidget *parent) : QWidget(parent) {
    QVBoxLayout *mainLayout = new QVBoxLayout(this);

    // Создаем грид для кнопок выбора цвета
    QGridLayout *colorGrid = new QGridLayout;
    QStringList colorNames = {"Белый", "Черный", "Зеленый", "Красный", "Желтый", "Синий"};
    QStringList colorValues = {"#FFFFFF", "#000000", "#00FF00", "#FF0000", "#FFFF00", "#0000FF"};

    int row = 0;
    int col = 0;
    for (int i = 0; i < colorNames.size(); ++i) {
        QPushButton *colorButton = new QPushButton(this);

        colorButton->setFixedSize(50, 50); // Устанавливаем фиксированный размер кнопки
        colorButton->setStyleSheet(QString("background-color: %1").arg(colorValues[i])); // Устанавливаем цвет кнопки

        connect(colorButton, &QPushButton::clicked, this, [this, colorValues, i](){
            emit colorSelected(QColor(colorValues[i])); // Отправляем сигнал с выбранным цветом
        });

        colorGrid->addWidget(colorButton, row, col);
        col++;

        if (col == 3) {
            col = 0;
            row++;
        }
    }

    mainLayout->addLayout(colorGrid);

    // Убираем стандартные кнопки закрытия и минимизации
    setWindowFlags(Qt::Window | Qt::FramelessWindowHint);
    setWindowFlags(Qt::WindowStaysOnTopHint | windowFlags());

    QPushButton *colorButton = new QPushButton("Выбрать цвет", this);
    connect(colorButton, &QPushButton::clicked, this, &ColorSelectionWidget::selectColor);
    mainLayout->addWidget(colorButton);

    // Добавляем собственную кнопку закрытия
    QPushButton *closeButton = new QPushButton("Закрыть", this);
    connect(closeButton, &QPushButton::clicked, this, &ColorSelectionWidget::close);
    mainLayout->addWidget(closeButton);
}

void ColorSelectionWidget::selectColor() {
    QColor color = QColorDialog::getColor(Qt::white, this, "Выберите цвет");
    if (color.isValid()) {
        emit colorSelected(color);
    }
}

void ColorSelectionWidget::closeEvent(QCloseEvent *event) {
    // При закрытии окна с выбором цвета закрываем все приложение
    QCoreApplication::quit();
}

void ColorSelectionWidget::mousePressEvent(QMouseEvent *event)
{
    if (event->button() == Qt::LeftButton) {
        // Захватываем окно, чтобы можно было его перемещать
        m_dragStartPosition = event->globalPos() - frameGeometry().topLeft();
        event->accept();
    }
}

void ColorSelectionWidget::mouseMoveEvent(QMouseEvent *event)
{
    if (event->buttons() & Qt::LeftButton) {
        // Перемещаем окно
        move(event->globalPos() - m_dragStartPosition);
        event->accept();
    }
}

void ColorSelectionWidget::mouseReleaseEvent(QMouseEvent *event)
{
    // При отпускании кнопки мыши необходимо отпустить захват окна
    Q_UNUSED(event)
}
