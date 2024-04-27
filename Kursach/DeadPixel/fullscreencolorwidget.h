#ifndef FULLSCREENCOLORWIDGET_H
#define FULLSCREENCOLORWIDGET_H

#include <QGuiApplication>
#include <QWidget>
#include <QScreen>
#include <QVBoxLayout>
#include <QPushButton>
#include <QColorDialog>

class FullScreenColorWidget : public QWidget
{
public:
    FullScreenColorWidget(QWidget *parent = nullptr);
    void showEvent(QShowEvent *event);
    void setBackgroundColor(const QColor &color);

};

#endif // FULLSCREENCOLORWIDGET_H
