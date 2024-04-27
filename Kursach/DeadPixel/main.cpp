#include "mainwindow.h"
#include "colorselectionwidget.h"
#include "fullscreencolorwidget.h"

int main(int argc, char *argv[]) {
    QApplication app(argc, argv);

    FullScreenColorWidget fullscreenWidget;
    fullscreenWidget.showFullScreen();

    ColorSelectionWidget colorSelector;
    QObject::connect(&colorSelector, &ColorSelectionWidget::colorSelected, [&app, &fullscreenWidget](const QColor &color){
        fullscreenWidget.setBackgroundColor(color);
    });
    colorSelector.setGeometry(100, 100, 200, 100);
    colorSelector.show();

    return app.exec();
}

//#include "main.moc"

/*int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    MainWindow w;
    w.showFullScreen();
    return a.exec();
}*/
