#ifndef COLORSELECTIONWIDGET_H
#define COLORSELECTIONWIDGET_H

#include <QWidget>
#include <QScreen>
#include <QVBoxLayout>
#include <QPushButton>
#include <QColorDialog>
#include <QMouseEvent>

class ColorSelectionWidget : public QWidget
{
    Q_OBJECT
public:
    ColorSelectionWidget(QWidget *parent = nullptr);

signals:
    void colorSelected(const QColor &color);

private slots:
    void selectColor();
    void closeEvent(QCloseEvent *event) override;

protected:
    void mousePressEvent(QMouseEvent *event) override;
    void mouseMoveEvent(QMouseEvent *event) override;
    void mouseReleaseEvent(QMouseEvent *event) override;

private:
    QPoint m_dragStartPosition;
};


#endif // COLORSELECTIONWIDGET_H
