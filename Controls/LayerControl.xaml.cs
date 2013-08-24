using Paint.WPF.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Paint.WPF.Controls
{
    /// <summary>
    /// Контрол, представляющий экземпляр слоя для рисования
    /// </summary>
    public partial class LayerControl : UserControl, INotifyPropertyChanged
    {
        #region Свойства, переменные, события

        public event EventHandler Delete;

        public event EventHandler<CheckedEventArgs> CheckedChanged;
        public class CheckedEventArgs : EventArgs
        {
            public bool isChecked { get; set; }
        }

        private string _layerName;
        public string LayerName
        {
            get
            {
                return _layerName;
            }
            set
            {
                _layerName = value;
                OnPropertyChanged("LayerName");
            }
        }

        public int LayerIndex { get; set; }
        public bool LayerFocus { get; set; }
        internal LayerWidget Widget { get; set; }

        private Point clickPosition;
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        public LayerControl()
        {
            InitializeComponent();
            LayerIndex = GlobalState.LayersCount++;
            // Устанавливаем самый большой индекс, для отображения поверх всех существующих слоев
            Canvas.SetZIndex(this, LayerIndex);

            Widget = new LayerWidget(this);
            LayerName = String.Format("{0}_{1}", "NewLayer", GlobalState.LayersIndexes + 1);

            Widget.widgetCheckBox.Checked += SetLayerVisibility;
            Widget.widgetCheckBox.Unchecked += SetLayerVisibility;
            Widget.widgetDel.Click += Del;

        }

        /// <summary>
        /// Восстановить видимость слоя, в зависимости от состояния виджета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetLayerVisibility(Object sender, EventArgs e)
        {
            switch (Widget.widgetCheckBox.IsChecked)
            {
                case true:
                    this.Visibility = Visibility.Visible;
                    CheckedChanged(this, new CheckedEventArgs { isChecked = true });
                    break;
                case false:
                    this.Visibility = Visibility.Hidden;
                    CheckedChanged(this, new CheckedEventArgs { isChecked = false });
                    break;
            }

        }

        private void Del(Object sender, EventArgs e)
        {
            GlobalState.LayersCount--;
            Delete(this, e);
        }

        /// <summary>
        /// Фиксация состояния, для начала перемещения слоя по холсту
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GlobalState.CurrentTool == Instruments.Arrow)
            {
                GlobalState.PressLeftButton = true;
                var draggableControl = (UserControl)sender;
                clickPosition = e.GetPosition(this);
                draggableControl.CaptureMouse();
            }
        }

        /// <summary>
        /// Фиксация состояния при окончании перемещения слоя по холсту
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GlobalState.CurrentTool == Instruments.Arrow)
            {
                GlobalState.PressLeftButton = false;
                var draggable = (UserControl)sender;
                draggable.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Перемещение слоя при зажатой левой кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (GlobalState.CurrentTool == Instruments.Arrow)
            {
                var draggableControl = (UserControl)sender;

                if (GlobalState.PressLeftButton && draggableControl != null)
                {
                    Point currentPosition = e.GetPosition((UIElement)this.Parent);

                    var transform = draggableControl.RenderTransform as TranslateTransform;
                    if (transform == null)
                    {
                        transform = new TranslateTransform();
                        draggableControl.RenderTransform = transform;
                    }

                    transform.X = currentPosition.X - clickPosition.X;
                    transform.Y = currentPosition.Y - clickPosition.Y;
                }
            }
        }

        /// <summary>
        /// Выделение границ слоя при фокусе виджета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            visualHost.IsFocused = true;
            visualHost.FocusSpace();
        }

        /// <summary>
        /// Возвращение состояния границ слоя при потере фокуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NonFocus(object sender, RoutedEventArgs e)
        {
            visualHost.IsFocused = false;
            visualHost.UnFocusSpace();
        }

        #region Реализация интерфейса INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
