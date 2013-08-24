using System;
using System.Collections.Generic;
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
using System.Windows.Input;
using Paint.WPF.Controls;
using Paint.WPF.Tools;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Annotations;
using System.IO;

namespace Paint.WPF
{
    /// <summary>
    /// Главное окно графического редактора
    /// </summary>
    public partial class MainWindow : Window
    {
        // Коллекция для биндинга виджетов созданных слоев к списку
        public ObservableCollection<LayerWidget> LayersWidgets { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LayersWidgets = new ObservableCollection<LayerWidget>();
            layerList.DataContext = this;

            GlobalState.ChangeInstrument += SetCursorStyle;
            GlobalState.ChangeColor += SetColorSample;

            GlobalState.Color = Brushes.Black;
            GlobalState.BrushSize = new Size(5, 5);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            LayerAdd_Click(null, null);
            Arrow_Selected(null, null);
        }

        /// <summary>
        /// Добавление нового слоя на холст и обновление коллекции виджетов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerAdd_Click(object sender, RoutedEventArgs e)
        {
            LayerControl layer = new LayerControl();
            mainCanvas.Children.Add(layer);
            LayersWidgets.Add(layer.Widget);
            
            // Перемещение элемента в самый верх списка, для наглядности отображения верхних слоев пользователю
            var last = LayersWidgets.Last();
            for (int i = LayersWidgets.Count - 1; i > 0; i--)
            {
                LayersWidgets[i] = LayersWidgets[i - 1];
            }
            LayersWidgets[0] = last;

            GlobalState.LayersIndexes++;

            if (layerList.Items.Count > 0)
                layerList.SelectedIndex = 0;

            layer.CheckedChanged += SelectLayer;
            layer.Delete += DeleteLayer;
        }

        /// <summary>
        /// Удаление слоя и обновление ZIndex у всех стоящих выше слоев
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteLayer(Object sender, EventArgs e)
        {
            if (sender != null)
            {
                LayersWidgets.Remove(((LayerControl)sender).Widget);
                mainCanvas.Children.Remove((LayerControl)sender);
                for (int i = ((LayerControl)sender).LayerIndex; i < mainCanvas.Children.Count; i++)
                {
                    var upperLayer = (LayerControl)mainCanvas.Children[i];
                    upperLayer.LayerIndex--;
                    var curZIndex = Canvas.GetZIndex(upperLayer);
                    Canvas.SetZIndex(upperLayer, --curZIndex);
                }
            }
        }

        /// <summary>
        /// Выделение слоя в прибиндинном списке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectLayer(Object sender, LayerControl.CheckedEventArgs e)
        {
            if (sender != null && e.isChecked)
            {
                for (int i = 0; i < LayersWidgets.Count; i++)
                {
                    if (LayersWidgets[i].ThisLayer.LayerIndex == ((LayerControl)sender).LayerIndex)
                    {
                        layerList.SelectedIndex = i;
                        break;
                    }

                }
            }
        }


        #region Выбор инструментов рисования

        private void Brush_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = Instruments.Brush;
            ClearToolsBorders();
            brushButton.BorderThickness = new Thickness(0.5);
        }

        private void Arrow_Selected(object sender, RoutedEventArgs e)
        {
            GlobalState.CurrentTool = Instruments.Arrow;
            ClearToolsBorders();
            arrowButton.BorderThickness = new Thickness(0.5);
        }
        #endregion

        
        /// <summary>
        /// Работа с палитрой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeBrushColor(object sender, RoutedEventArgs e)
        {
            GlobalState.Color = ((Button)sender).Background;
            Brush_Selected(null, null);
        }
        

        /// <summary>
        /// Сбросить выделение всех выбранных инструментов
        /// </summary>
        private void ClearToolsBorders()
        {
            arrowButton.BorderThickness = new Thickness(0);
            brushButton.BorderThickness = new Thickness(0);
        }

        /// <summary>
        /// Изменение курсора мыши в зависимости от выбранного инструмента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetCursorStyle(Object sender, EventArgs e)
        {
            switch (GlobalState.CurrentTool)
            {
                case Instruments.Brush:
                    mainCanvas.Cursor = Cursors.Cross;
                    break;
                default: mainCanvas.Cursor = Cursors.Arrow;
                    break;
            }
        }

        /// <summary>
        /// Показать текущий выбранный цвет
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetColorSample(Object sender, EventArgs e)
        {
            curBrushSample.Fill = GlobalState.Color;
        }

        /// <summary>
        /// Повышение Zindex выбранного слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerUp_Click(object sender, RoutedEventArgs e)
        {
            if (layerList.SelectedIndex > 0)
                SwapLayers(layerList.SelectedIndex, layerList.SelectedIndex - 1);
        }

        /// <summary>
        /// Понижение Zindex выбранного слоя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayerDown_Click(object sender, RoutedEventArgs e)
        {
            if (layerList.SelectedIndex < layerList.Items.Count - 1)
                SwapLayers(layerList.SelectedIndex, layerList.SelectedIndex + 1);
        }

        /// <summary>
        /// Обмен слоев местами
        /// </summary>
        /// <param name="curIndx">Индекс выделенного слоя</param>
        /// <param name="nextIndx">Индекс обмениваемого слоя</param>
        private void SwapLayers(int curIndx, int nextIndx)
        {
            var curWidget = LayersWidgets[curIndx];
            LayerWidget nextWidget = LayersWidgets[nextIndx];

            LayersWidgets[curIndx] = LayersWidgets[nextIndx];
            LayersWidgets[nextIndx] = curWidget;
            layerList.SelectedIndex = nextIndx;

            var curZIndex = Canvas.GetZIndex(mainCanvas.Children[curWidget.ThisLayer.LayerIndex]);
            var nextZIndex = Canvas.GetZIndex(mainCanvas.Children[nextWidget.ThisLayer.LayerIndex]);

            Canvas.SetZIndex(mainCanvas.Children[curWidget.ThisLayer.LayerIndex], nextZIndex);
            Canvas.SetZIndex(mainCanvas.Children[nextWidget.ThisLayer.LayerIndex], curZIndex);
        }


        /// <summary>
        /// Увеличение рабочей области холста, при сворачивании списка слоев
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayersExpanded(object sender, RoutedEventArgs e)
        {
            if (mainCanvas == null) return;

            if (layersExpander.IsExpanded)
            {
                mainCanvas.ClipToBounds = true;
                layersSplitter.Visibility = Visibility.Visible;
            }
            else
            {
                mainCanvas.ClipToBounds = false;
                layersSplitter.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Изменение фокуса слоя, выделенного в отображаемом списке виджетов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void layerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (layerList.SelectedItems.Count > 0)
            {
                var selectedWidget = LayersWidgets[layerList.SelectedIndex];
                var layer = mainCanvas.Children[selectedWidget.ThisLayer.LayerIndex];
                layer.Focus();

                foreach (LayerControl child in mainCanvas.Children)
                {
                    if (child != layer)
                        child.NonFocus(null, null);
                }
            }
        }

        #region Методы экспорта

        private void SavePicture(object sender, RoutedEventArgs e)
        {
            var saveDlg = new Microsoft.Win32.SaveFileDialog{
                FileName = "Masterpiece",
                DefaultExt = ".png",
                Filter = "PNG (.png)|*.png"
            };

            if (saveDlg.ShowDialog() == true)
            {
                HideAllWorkSpaces();

                SaveCanvas(mainCanvas, 96, saveDlg.FileName);

                RestoreAllWorkSpaces();
            }
        }

        public void SaveCanvas(Canvas canvas, int dpi, string filename)
        {
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            Size size = new Size(width, height);
            canvas.Measure(size); 

            var rtb = new RenderTargetBitmap( 
                (int)width,
                (int)height,
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

            using (var stm = File.Create(filename)) 
            { 
                enc.Save(stm); 
            }
        }
        #endregion

        /// <summary>
        /// Скрыть фон и границы для всех слоев
        /// </summary>
        private void HideAllWorkSpaces()
        {
            foreach (LayerControl layer in mainCanvas.Children)
            {
                layer.visualHost.HideWorkSpace();
            }
        }

        /// <summary>
        /// Восстановить предыдущий фон и границы
        /// </summary>
        private void RestoreAllWorkSpaces()
        {
            foreach (LayerControl layer in mainCanvas.Children)
            {
                layer.visualHost.RestoreWorkSpace();
            }
        }
    }
}
