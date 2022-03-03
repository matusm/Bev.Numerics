using System;
using System.Collections.Generic;
using System.Linq;

namespace Bev.Numerics
{
    /// <summary>
    /// Methods to calculate the standard deviation and the span of a double array and a list of doubles.
    /// </summary>
    public static class ArrayExtensions
    {

        /// <summary>
        /// Evaluates the standard deviation of a given array of doubles.
        /// </summary>
        /// <param name="values">An array of double values.</param>
        /// <returns>The standard deviation or <c>double.NaN</c>.</returns>
        public static double StandardDeviation(this double[] values)
        {
            int n = values.Length;
            if (n < 2) return double.NaN;
            double var = 0;
            double mean = values.Average();
            foreach (double x in values)
                var += (x - mean) * (x - mean);
            return Math.Sqrt(var / (n - 1));
        }

        /// <summary>
        /// Evaluates the standard deviation of a given list of doubles.
        /// </summary>
        /// <param name="values">A list of double values.</param>
        /// <returns>The standard deviation or <c>double.NaN</c>.</returns>
        public static double StandardDeviation(this List<double> values)
        {
            return StandardDeviation(values.ToArray());
        }

        /// <summary>
        /// Evaluates the standard deviation of a given array of decimals.
        /// </summary>
        /// <param name="values">An array of decimal values.</param>
        /// <returns>The standard deviation or <c>null</c>.</returns>
        public static decimal? StandardDeviation(this decimal[] values)
        {
            int n = values.Length;
            if (n < 2) return null;
            decimal var = 0;
            decimal mean = values.Average();
            foreach (decimal x in values)
                var += (x - mean) * (x - mean);
            return (decimal)Math.Sqrt((double)(var / (n - 1)));
        }

        /// <summary>
        /// Evaluates the standard deviation of a given list of decimals.
        /// </summary>
        /// <param name="values">A list of decimal values.</param>
        /// <returns>The standard deviation or <c>null</c>.</returns>
        public static decimal? StandardDeviation(this List<decimal> values)
        {
            return StandardDeviation(values.ToArray());
        }

        /// <summary>
        /// Evaluates the span of a given array of doubles.
        /// </summary>
        /// <param name="values">An array of double values.</param>
        /// <returns>The span.</returns>
        public static double Span(this double[] values)
        {
            return values.Max() - values.Min();
        }

        /// <summary>
        /// Evaluates the span of a given list of doubles.
        /// </summary>
        /// <param name="values">A list of double values.</param>
        /// <returns>The span.</returns>
        public static double Span(this List<double> values)
        {
            return values.Max() - values.Min();
        }

        /// <summary>
        /// Evaluates the span of a given array of decimals.
        /// </summary>
        /// <param name="values">An array of decimal values.</param>
        /// <returns>The span.</returns>
        public static decimal Span(this decimal[] values)
        {
            return values.Max() - values.Min();
        }

        /// <summary>
        /// Evaluates the span of a given list of decimals.
        /// </summary>
        /// <param name="values">A list of decimal values.</param>
        /// <returns>The span.</returns>
        public static decimal Span(this List<decimal> values)
        {
            return values.Max() - values.Min();
        }

      
        /*
        // Funktioniert leider nicht: - Operator nicht gewährleistet
        public static T Span<T> (this T[] values) where T: IEnumerable<float>
        {
            return values.Max() - values.Min();
        }
        */

    }
}
