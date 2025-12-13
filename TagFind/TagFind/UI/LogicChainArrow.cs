using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagFind.Classes;
using Windows.Foundation;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class LogicChainArrow : Control
    {
        private TextBlock? _textBlock;
        private Canvas? _canvas;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                if (_textBlock != null)
                {
                    _textBlock.Text = _text;
                    RedrawArrow();
                }
            }
        }

        private string _text = string.Empty;

        public LogicChainArrow()
        {
            DefaultStyleKey = typeof(LogicChainArrow);
            this.Loaded += LogicChainArrow_Loaded;
        }

        private void LogicChainArrow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += LogicChainArrow_Unloaded;
            this.ActualThemeChanged += LogicChainArrow_ActualThemeChanged;
        }

        private void LogicChainArrow_Unloaded(object sender, RoutedEventArgs e)
        {
            //this.Loaded -= LogicChainArrow_Loaded;
            this.Unloaded -= LogicChainArrow_Unloaded;
            this.ActualThemeChanged -= LogicChainArrow_ActualThemeChanged;
        }

        private void LogicChainArrow_ActualThemeChanged(FrameworkElement sender, object args)
        {
            RedrawArrow();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock ?? new();
            _canvas = GetTemplateChild("PART_Canvas") as Canvas ?? new();

            _textBlock.Text = Text;
            RedrawArrow();
        }

        public void RedrawArrow()
        {
            if (_textBlock == null
                || _canvas == null)
            {
                return;
            }
            _textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _canvas.Height = 8;
            _canvas.Width = _textBlock.DesiredSize.Width + 10;
            _canvas.Children.Clear();

            Brush brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
            TextBlock tb = new();
            brush = tb.Foreground;
            //if (this.TryFindResource(nameof(Consts.ResourceKeys.ThemeResourceKeys.TextBoxForegroundThemeBrush), out object? themeBrush))
            //{
            //    if (themeBrush is Brush b)
            //    {
            //        brush = b;
            //    }
            //}
            //brush = Application.Current.Resources[nameof(Consts.ResourceKeys.ThemeResourceKeys.TextBoxForegroundThemeBrush)] as Brush;

            Line _arrowLine = new();
            _arrowLine.X1 = 0;
            _arrowLine.Y1 = 0;
            _arrowLine.X2 = _textBlock.DesiredSize.Width + 10;
            _arrowLine.Y2 = 0;
            _arrowLine.StrokeThickness = 1;
            _arrowLine.Stroke = brush;
            _canvas.Children.Add(_arrowLine);
            Line _arrowUpper = new();
            _arrowUpper.X1 = _textBlock.DesiredSize.Width + 10;
            _arrowUpper.Y1 = 0;
            _arrowUpper.X2 = _textBlock.DesiredSize.Width + 10 - 6;
            _arrowUpper.Y2 = -4;
            _arrowUpper.StrokeThickness = 1;
            _arrowUpper.Stroke = brush;
            Line _arrowLower = new();
            _canvas.Children.Add(_arrowUpper);
            _arrowLower.X1 = _textBlock.DesiredSize.Width + 10;
            _arrowLower.Y1 = 0;
            _arrowLower.X2 = _textBlock.DesiredSize.Width + 10 - 6;
            _arrowLower.Y2 = 4;
            _arrowLower.StrokeThickness = 1;
            _arrowLower.Stroke = brush;
            _canvas.Children.Add(_arrowLower);
        }
    }
}
