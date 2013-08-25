using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Paint.WPF.Controls;
using Paint.WPF.Tools;

namespace Paint.WPF
{
    /// <summary>
    ///     Главное окно графического редактора
    /// </summary>
    public partial class MainWindow : Window
    {
        // Коллекция для биндинга виджетов созданных слоев к списку

        /// <summary>
        ///     Конструктор
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LayersWidgets = new ObservableCollection<LayerWidget>();
            LayerList.DataContext = this;

            GlobalState.ChangeInstrument += SetCursorStyle;
            GlobalState.ChangeColor += SetColorSample;

            GlobalState.Color = Brushes.Black;
            GlobalState.BrushSize = new Size(5, 5);
        }

        public ObservableCollection<LayerWidget> LayersWidgets { get; set; }

        private void Load(object sender, RoutedEventArgs e)
        {
            LayerAdd_Click(null, null);
            Arrow_Selected(null, null);
        }

        /// <summary>
        ///     Добавление нового слоя на холст и обновление коллекции виджетов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerAdd_Click(object sender, RoutedEventArgs e)
        {
            var layer = new LayerControl();
            MainCanvas.Children.Add(layer);
            LayersWidgets.Add(layer.Widget);

            // Перемещение элемента в самый верх списка, для наглядности отображения верхних слоев пользователю
            LayerWidget last = LayersWidgets.Last();
            for (int i = LayersWidgets.Count - 1; i > 0; i--)
            {
                LayersWidgets[i] = LayersWidgets[i - 1];
            }
            LayersWidgets[0] = last;

            GlobalState.LayersIndexes++;

            if (LayerList.Items.Count > 0)
                LayerList.SelectedIndex = 0;

            layer.CheckedChanged += SelectLayer;
            layer.Delete += DeleteLayer;
        }

        /// <summary>
        ///     Удаление слоя и обновление ZIndex у всех стоящих выше слоев
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteLayer(Object sender, EventArgs e)
        {
            if (sender != null)
            {
                LayersWidgets.Remove(((LayerControl) sender).Widget);
                MainCanvas.Children.Remove((LayerControl) sender);
                for (int i = ((LayerControl) sender).LayerIndex; i < MainCanvas.Children.Count; i++)
                {
                    var upperLayer = (LayerControl) MainCanvas.Children[i];
                    upperLayer.LayerIndex--;
                    int curZIndex = Panel.GetZIndex(upperLayer);
                    Panel.SetZIndex(upperLayer, --curZIndex);
                }
            }
        }

        /// <summary>
        ///     Выделение слоя в прибиндинном списке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectLayer(Object sender, LayerControl.CheckedEventArgs e)
        {
            if (sender != null && e.IsChecked)
            {
                for (int i = 0; i < LayersWidgets.Count; i++)
                {
                    if (LayersWidgets[i].ThisLayer.LayerIndex == ((LayerControl) sender).LayerIndex)
                    {
                        LayerList.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Работа с палитрой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeBrushColor(object sender, RoutedEventArgs e)
        {
            GlobalState.Color = ((Button) sender).Background;
            Brush_Selected(null, null);
        }


        /// <summary>
        ///     Сбросить выделение всех выбранных инструментов
        /// </summary>
        private void ClearToolsBorders()
        {
            ArrowButton.BorderThickness = new Thickness(0);
            BrushButton.BorderThickness = new Thickness(0);
        }

        /// <summary>
        ///     Изменение курсора мыши в зависимости от выбранного инструмента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetCursorStyle(Object sender, EventArgs e)
        {
            switch (GlobalState.CurrentTool)
            {
                case Instruments.Brush:
                    MainCanvas.Cursor = Cursors.Cross;
                    break;
                default:
                    MainCanvas.Cursor = Cursors.Arrow;
                    break;
            }
        }

        /// <summary>
        ///     Показать текущий выбранный цвет
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetColorSample(Object sender, EventArgs e)
        {
            CurBrushSample.Fill = GlobalState.Color;
        }

        /// <summary>
        ///     Повышение Zindex выбранного слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerUp_Click(object sender, RoutedEventArgs e)
        {
            if (LayerList.SelectedIndex > 0)
                SwapLayers(LayerList.SelectedIndex, LayerList.SelectedIndex - 1);
        }

        /// <summary>
        ///     Понижение Zindex выбранного слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerDown_Click(object sender, RoutedEventArgs e)
        {
            if (LayerList.SelectedIndex < LayerList.Items.Count - 1)
                SwapLayers(LayerList.SelectedIndex, LayerList.SelectedIndex + 1);
        }

        /// <summary>
        ///     Обмен слоев местами
        /// </summary>
        /// <param name="curIndx">Индекс выделенного слоя</param>
        /// <param name="nextIndx">Индекс обмениваемого слоя</param>
        private void SwapLayers(int curIndx, int nextIndx)
        {
            LayerWidget curWidget = LayersWidgets[curIndx];
            LayerWidget nextWidget = LayersWidgets[nextIndx];

            LayersWidgets[curIndx] = LayersWidgets[nextIndx];
            LayersWidgets[nextIndx] = curWidget;
            LayerList.SelectedIndex = nextIndx;

            int curZIndex = Panel.GetZIndex(MainCanvas.Children[curWidget.ThisLayer.LayerIndex]);
            int nextZIndex = Panel.GetZIndex(MainCanvas.Children[nextWidget.ThisLayer.LayerIndex]);

            Panel.SetZIndex(MainCanvas.Children[curWidget.ThisLayer.LayerIndex], nextZIndex);
            Panel.SetZIndex(MainCanvas.Children[nextWidget.ThisLayer.LayerIndex], curZIndex);
        }


        /// <summary>
        ///     Увеличение рабочей области холста, при сворачивании списка слоев
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayersExpanded(object sender, RoutedEventArgs e)
        {
            if (MainCanvas == null) return;

            MainCanvas.ClipToBounds = LayersExpander.IsExpanded;
        }

        /// <summary>
        ///     Изменение фокуса слоя, выделенного в отображаемом списке виджетов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayerList.SelectedItems.Count > 0)
            {
                LayerWidget selectedWidget = LayersWidgets[LayerList.SelectedIndex];
                UIElement layer = MainCanvas.Children[selectedWidget.ThisLayer.LayerIndex];
                layer.Focus();

                foreach (LayerControl child in MainCanvas.Children)
                {
                    if (child != layer)
                        child.NonFocus(null, null);
                }
            }
        }

        /// <summary>
        ///     Скрыть фон и границы для всех слоев
        /// </summary>
        private void HideAllWorkSpaces()
        {
            foreach (LayerControl layer in MainCanvas.Children)
            {
                layer.VisualHost.HideWorkSpace();
            }
        }

        /// <summary>
        ///     Восстановить предыдущий фон и границы
        /// </summary>
        private void RestoreAllWorkSpaces()
        {
            foreach (LayerControl layer in MainCanvas.Children)
            {
                layer.VisualHost.RestoreWorkSpace();
            }
        }

        #region Методы экспорта

        private void SavePicture(object sender, RoutedEventArgs e)
        {
            var saveDlg = new SaveFileDialog
                {
                    FileName = "Masterpiece",
                    DefaultExt = ".png",
                    Filter = "PNG (.png)|*.png"
                };

            if (saveDlg.ShowDialog() == true)
            {
                HideAllWorkSpaces();

                SaveCanvas(MainCanvas, 96, saveDlg.FileName);

                RestoreAllWorkSpaces();
            }
        }

        private void SaveCanvas(Canvas canvas, int dpi, string filename)
        {
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            var size = new Size(width, height);
            canvas.Measure(size);

            var rtb = new RenderTargetBitmap(
                (int) width,
                (int) height,
                dpi, //dpi x 
                dpi, //dpi y 
                PixelFormats.Pbgra32 // pixelformat 
                );
            rtb.Render(canvas);

            SaveAsPng(rtb, filename);
        }

        private static void SaveAsPng(RenderTargetBitmap bmp, string filename)
        {
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));

            using (FileStream stm = File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        #endregion

        #region Выбор инструментов рисования

        private void Brush_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = Instruments.Brush;
            ClearToolsBorders();
            BrushButton.BorderThickness = new Thickness(0.5);
        }

        private void Arrow_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = Instruments.Arrow;
            ClearToolsBorders();
            ArrowButton.BorderThickness = new Thickness(0.5);
        }

        #endregion
    }
}