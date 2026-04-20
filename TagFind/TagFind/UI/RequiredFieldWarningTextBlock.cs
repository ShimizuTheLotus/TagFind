using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagFind.UI
{
    public sealed partial class RequiredFieldWarningTextBlock : Control
    {
        public TextBlock? TextBlockInstance;

        public static readonly DependencyProperty ShowTriggerProperty =
            DependencyProperty.Register(
                nameof(ShowTrigger),
                typeof(bool),
                typeof(RequiredFieldWarningTextBlock),
                new PropertyMetadata(false, OnShowTriggerChanged));
        public bool ShowTrigger
        {
            get => (bool)GetValue(ShowTriggerProperty);
            set => SetValue(ShowTriggerProperty, value);
        }
        public static void OnShowTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RequiredFieldWarningTextBlock)d;
            control.ChangeVisibility();
        }

        public static readonly DependencyProperty FieldValueProperty =
            DependencyProperty.Register(
                nameof(FieldValue),
                typeof(object),
                typeof(RequiredFieldWarningTextBlock),
                new PropertyMetadata(false, OnFieldValueChanged));

        public object? FieldValue
        {
            get => (object?)GetValue(FieldValueProperty);
            set => SetValue(FieldValueProperty, value);
        }
        public static void OnFieldValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RequiredFieldWarningTextBlock)d;
            control.ChangeVisibility();
        }

        public static readonly DependencyProperty EmptyFieldValueProperty =
            DependencyProperty.Register(
                nameof(EmptyFieldValue),
                typeof(object),
                typeof(RequiredFieldWarningTextBlock),
                new PropertyMetadata(false, OnEmptyFieldValueChanged));
        public object? EmptyFieldValue
        {
            get => (object?)GetValue(EmptyFieldValueProperty);
            set => SetValue(EmptyFieldValueProperty, value);
        }
        public static void OnEmptyFieldValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RequiredFieldWarningTextBlock)d;
            control.ChangeVisibility();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public RequiredFieldWarningTextBlock()
        {
            DefaultStyleKey = typeof(RequiredFieldWarningTextBlock);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBlockInstance = GetTemplateChild("PART_TextBlock") as TextBlock;
            ChangeVisibility();
        }

        public void ChangeVisibility()
        {
            if (ShowTrigger == false)
            {
                this.Visibility = Visibility.Collapsed;
                this.Height = 0;
                return;
            }
            if (FieldValue == null)
            {
                this.Visibility = Visibility.Visible;
                this.Height = TextBlockInstance?.ActualHeight ?? 0;
                return;
            }
            this.Visibility = FieldValue.Equals(EmptyFieldValue) ? Visibility.Visible : Visibility.Collapsed;
            this.Height = FieldValue.Equals(EmptyFieldValue) ? TextBlockInstance?.ActualHeight ?? 0 : 0;
        }

    }
}
