#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QApplication>
#include <QPushButton>
#include <QColorDialog>
#include <QPalette>
#include <QWidget>
#include <QGuiApplication>
#include <QScreen>


QT_BEGIN_NAMESPACE
namespace Ui {
class MainWindow;
}
QT_END_NAMESPACE

class MainWindow : public QWidget
{
    Q_OBJECT

public:
    MainWindow(QWidget *parent = nullptr);
    void showEvent(QShowEvent *event);
    ~MainWindow();

private:
    QPushButton* colorButtons[6];
    QColor currentColor;

    void handleColorChange(const QColor& color);
    Ui::MainWindow *ui;
};
#endif // MAINWINDOW_H
