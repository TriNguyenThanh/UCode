using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UCode.Desktop.Helpers
{
    public class ClassIdToGradientConverter : IValueConverter
    {
        private static readonly LinearGradientBrush[] GradientBrushes = new[]
        {
            // Gradient 1: Blue to Purple
            new LinearGradientBrush(
                Color.FromRgb(99, 102, 241),   // Indigo
                Color.FromRgb(168, 85, 247),   // Purple
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            ),
            
            // Gradient 2: Green to Teal
            new LinearGradientBrush(
                Color.FromRgb(16, 185, 129),   // Green
                Color.FromRgb(6, 182, 212),    // Cyan
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            ),
            
            // Gradient 3: Orange to Pink
            new LinearGradientBrush(
                Color.FromRgb(251, 146, 60),   // Orange
                Color.FromRgb(236, 72, 153),   // Pink
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            ),
            
            // Gradient 4: Red to Orange
            new LinearGradientBrush(
                Color.FromRgb(239, 68, 68),    // Red
                Color.FromRgb(251, 146, 60),   // Orange
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            ),
            
            // Gradient 5: Purple to Pink
            new LinearGradientBrush(
                Color.FromRgb(139, 92, 246),   // Violet
                Color.FromRgb(219, 39, 119),   // Pink
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            ),
            
            // Gradient 6: Blue to Cyan
            new LinearGradientBrush(
                Color.FromRgb(59, 130, 246),   // Blue
                Color.FromRgb(14, 165, 233),   // Sky
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            ),
            
            // Gradient 7: Teal to Green
            new LinearGradientBrush(
                Color.FromRgb(20, 184, 166),   // Teal
                Color.FromRgb(34, 197, 94),    // Green
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            ),
            
            // Gradient 8: Navy to Blue (Default similar to original)
            new LinearGradientBrush(
                Color.FromRgb(25, 25, 112),    // Navy (#191970)
                Color.FromRgb(67, 97, 238),    // Royal Blue
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1)
            )
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return GradientBrushes[0];

            // Use hash code to deterministically select a gradient based on the ClassId
            int hashCode = value.GetHashCode();
            int index = Math.Abs(hashCode) % GradientBrushes.Length;
            
            return GradientBrushes[index];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
