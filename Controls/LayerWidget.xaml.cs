using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        public readonly LayerControl ThisLayer;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="layerControl"></param>
        public LayerWidget(LayerControl layerControl)
        {
            ThisLayer = layerControl;
            DataContext = ThisLayer;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            XSize.Text = ThisLayer.VisualHost.SpaceSize.Width.ToString();
            YSize.Text = ThisLayer.VisualHost.SpaceSize.Height.ToString();
        }

        /// <summary>
        /// Скрытие и раскрытие панели свойств текущего слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ContextPanel.Visibility == Visibility.Hidden)
            {
                ContextPanel.Visibility = Visibility.Visible;
                ContextRow.Height = new GridLength(130);
            }
            else if (ContextPanel.Visibility == Visibility.Visible)
            {
                ContextPanel.Visibility = Visibility.Hidden;
                ContextRow.Height = new GridLength(0);
            }
        }

        /// <summary>
        /// Открытие состояния редактирования названия слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditBox.Text = WidgetText.Text;
            WidgetText.Visibility = Visibility.Hidden;
            EditBox.Visibility = Visibility.Visible;
            EditBox.Focus();
            EditBox.SelectAll();
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
                WidgetText.Text = EditBox.Text;
                EditBox.Visibility = Visibility.Hidden;
                WidgetText.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Изменение фоновй заливки слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeFillColor(object sender, RoutedEventArgs e)
        {
            ThisLayer.VisualHost.ChangeFill(((Button)sender).Background);
        }

        /// <summary>
        /// Событие, для взаимодействия с текстбоксами размера холста
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeChanged(object sender, TextChangedEventArgs e)
        {
            if (XSize.Text != "" && YSize.Text != "")
            {
                double x, y;
                if (double.TryParse(XSize.Text, out x) && double.TryParse(YSize.Text, out y))
                    if (x > 0 && y > 0)
                        ThisLayer.VisualHost.ChangeSize(new Size(x, y));
            }
        }


    }
}
