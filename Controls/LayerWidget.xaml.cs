using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint.WPF.Controls
{
    /// <summary>
    /// Контрол для отображения слоев в каком-либо списке
    /// </summary>
    public partial class LayerWidget : UserControl
    {
        /// <summary>
        /// Ссылка на экземпляр родительского слоя
        /// </summary>
        public LayerControl ThisLayer;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="layerControl"></param>
        public LayerWidget(LayerControl layerControl)
        {
            ThisLayer = layerControl;
            this.DataContext = ThisLayer;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            xSize.Text = ThisLayer.visualHost.SpaceSize.Width.ToString();
            ySize.Text = ThisLayer.visualHost.SpaceSize.Height.ToString();
        }

        /// <summary>
        /// Скрытие и раскрытие панели свойств текущего слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (contextPanel.Visibility == Visibility.Hidden)
            {
                contextPanel.Visibility = Visibility.Visible;
                contextRow.Height = new GridLength(130);
            }
            else if (contextPanel.Visibility == Visibility.Visible)
            {
                contextPanel.Visibility = Visibility.Hidden;
                contextRow.Height = new GridLength(0);
            }
        }

        /// <summary>
        /// Открытие состояния редактирования названия слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            editBox.Text = widgetText.Text;
            widgetText.Visibility = Visibility.Hidden;
            editBox.Visibility = Visibility.Visible;
            editBox.Focus();
            editBox.SelectAll();
        }

        /// <summary>
        /// Закрытие состояния редактирования названия слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                widgetText.Text = editBox.Text;
                editBox.Visibility = Visibility.Hidden;
                widgetText.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Изменение фоновй заливки слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeFillColor(object sender, RoutedEventArgs e)
        {
            ThisLayer.visualHost.ChangeFill(((Button)sender).Background);
        }

        /// <summary>
        /// Событие, для взаимодействия с текстбоксами размера холста
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeChanged(object sender, TextChangedEventArgs e)
        {
            if (xSize.Text != "" && ySize.Text != "")
            {
                double x = 0,
                    y = 0;
                if (double.TryParse(xSize.Text, out x) && double.TryParse(ySize.Text, out y))
                    if (x > 0 && y > 0)
                        ThisLayer.visualHost.ChangeSize(new Size(x, y));
            }
        }


    }
}
