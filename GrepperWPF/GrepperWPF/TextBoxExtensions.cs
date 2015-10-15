using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GrepperWPF
{
   public static class TextBoxHelper
   {
      public static int GetSelectionStart(DependencyObject obj)
      {
         return (int)obj.GetValue(SelectionStartProperty);
      }

      public static void SetSelectionStart(DependencyObject obj, int value)
      {
         obj.SetValue(SelectionStartProperty, value);
      }

      // Using a DependencyProperty as the backing store for SelectedText.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SelectionStartProperty =
          DependencyProperty.RegisterAttached(
              "SelectionStart",
              typeof(int),
              typeof(TextBoxHelper),
              new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectionStartChanged));

      private static void SelectionStartChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         TextBox tb = obj as TextBox;
         if (tb != null)
         {
            tb.SelectionStart = (int)e.NewValue;
            tb.Focus();
            
            var rect = tb.GetRectFromCharacterIndex(tb.SelectionStart);
            var line = tb.GetLineIndexFromCharacterIndex(tb.SelectionStart);
            tb.ScrollToLine(line);

            var actualX = tb.HorizontalOffset + rect.Left;
            if (actualX <= tb.ViewportWidth)
            {
               actualX = 0;
            }

            tb.ScrollToHorizontalOffset(actualX);
         }
      }

      public static int GetSelectionLength(DependencyObject obj)
      {
         return (int)obj.GetValue(SelectionLengthProperty);
      }

      public static void SetSelectionLength(DependencyObject obj, int value)
      {
         obj.SetValue(SelectionLengthProperty, value);
      }

      // Using a DependencyProperty as the backing store for SelectedText.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SelectionLengthProperty =
          DependencyProperty.RegisterAttached(
              "SelectionLength",
              typeof(int),
              typeof(TextBoxHelper),
              new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectionLengthChanged));

      private static void SelectionLengthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         TextBox tb = obj as TextBox;
         if (tb != null)
         {
            tb.SelectionLength = (int)e.NewValue;
            tb.Focus();
         }
      }
   }
}
