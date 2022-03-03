using System;
using System.Globalization;
using System.Linq;

namespace Bev.Numerics
{
    /// <summary>
    /// Implements binary operators for interval arithmetic.
    /// Provides static methods for trigonometric, logarithmic, and other common 
    /// mathematical functions for intervals.
    /// </summary>
    /// <remarks>
    /// If an interval occurs several times in a calculation, each occurrence is
    /// taken independently then this can lead to an unwanted 
    /// expansion of the resulting intervals (so-called dependency problem).
    /// In general, it can be shown that the exact range of values can be 
    /// achieved, if each variable appears only once in an equation and if f is
    /// continuous inside the box. 
    /// </remarks>
    public class Interval : IFormattable
    {
        #region Private Fields

        /// <summary>
        /// The lower endpoint.
        /// </summary>
        double a;

        /// <summary>
        /// The upper endpoint
        /// </summary>
        double b;

        /// <summary>
        /// A value indicating whether zero is inside the interval.
        /// Endpoints are not considered to be inside.
        /// </summary>
        bool zeroIsInside;

        /// <summary>
        /// A value indicating if the interval is non-empty.
        /// </summary>
        bool isInterval;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the lower endpoint if interval is non-empty.
        /// </summary>
        /// <value>The lower endpoint or null.</value>
        public double? A { get { if (isInterval) return a; else return null; } }

        /// <summary>
        /// Gets the upper endpoint if interval is non-empty.
        /// </summary>
        /// <value>The upper endpoint or null.</value>
        public double? B { get { if (isInterval) return b; else return null; } }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:IntervalArithmetic.Interval"/> is a proper or improper interval.
        /// </summary>
        /// <value><c>true</c> if is interval; otherwise, <c>false</c>.</value>
        public bool IsInterval { get { return isInterval; } }

        /// <summary>
        /// Gets a value indicating whether zero is an element of this interval.
        /// </summary>
        /// <value><c>true</c> if zero is an element; otherwise, <c>false</c>.</value>
        public bool ZeroIsInside { get { return zeroIsInside; } }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of a proper interval.
        /// </summary>
        /// <param name="a">The lower endpoint.</param>
        /// <param name="b">The upper endpoint.</param>
        public Interval(double a, double b)
        {
            isInterval = true;
            this.a = a;
            this.b = b;
            Consolidate();
        }

        /// <summary>
        /// Initializes a new instance of an improper interval (single point).
        /// </summary>
        /// <param name="a">The point.</param>
        public Interval(double a) : this(a, a) { }

        /// <summary>
        /// Initializes a new instance of a non-interval (or empty interval).
        /// </summary>
        public Interval()
        {
            isInterval = false;
            zeroIsInside = false;
        }

        #endregion

        #region Conversion Operator: Interval <- double

        /// <summary>
        /// Implicit cast from <c>double</c> to <c>Interval</c>.
        /// </summary>
        /// <param name="q">An improper interval.</param>
        public static implicit operator Interval(double q) { return new Interval(q); }

        #endregion

        #region Operator Overloading

        /// <summary>
        /// Adds a <see cref="Interval"/> to a <see cref="Interval"/>, yielding a new <see cref="T:IntervalArithmetic.Interval"/>.
        /// </summary>
        /// <param name="x">The first <see cref="Interval"/> to add.</param>
        /// <param name="y">The second <see cref="Interval"/> to add.</param>
        /// <returns>The <see cref="T:IntervalArithmetic.Interval"/> that is the sum of the values of <c>x</c> and <c>y</c>.</returns>
        public static Interval operator +(Interval x, Interval y)
        {
            if (!x.isInterval || !y.isInterval)
                return new Interval();
            return new Interval(x.a + y.a, x.b + y.b);
        }

        /// <summary>
        /// Subtracts a <see cref="Interval"/> from a <see cref="Interval"/>, yielding a
        /// new <see cref="T:IntervalArithmetic.Interval"/>.
        /// </summary>
        /// <param name="x">The <see cref="Interval"/> to subtract from (the minuend).</param>
        /// <param name="y">The <see cref="Interval"/> to subtract (the subtrahend).</param>
        /// <returns>The <see cref="T:IntervalArithmetic.Interval"/> that is the <c>x</c> minus <c>y</c>.</returns>
        public static Interval operator -(Interval x, Interval y)
        {
            if (!x.isInterval || !y.isInterval)
                return new Interval();
            return new Interval(x.a - y.b, x.b - y.a);
        }

        /// <summary>
        /// Computes the product of <c>x</c> and <c>y</c>, yielding a new <see cref="T:IntervalArithmetic.Interval"/>.
        /// </summary>
        /// <param name="x">The <see cref="Interval"/> to multiply.</param>
        /// <param name="y">The <see cref="Interval"/> to multiply.</param>
        /// <returns>The <see cref="T:IntervalArithmetic.Interval"/> that is the <c>x</c> * <c>y</c>.</returns>
        public static Interval operator *(Interval x, Interval y)
        {
            if (!x.isInterval || !y.isInterval)
                return new Interval();
            double[] combination = new double[4];
            combination[0] = x.a * y.a;
            combination[1] = x.a * y.b;
            combination[2] = x.b * y.a;
            combination[3] = x.b * y.b;
            return new Interval(combination.Min(), combination.Max());
        }

        /// <summary>
        /// Computes the division of <c>x</c> and <c>y</c>, yielding a new <see cref="T:IntervalArithmetic.Interval"/>.
        /// </summary>
        /// <param name="x">The <see cref="Interval"/> to divide (the divident).</param>
        /// <param name="y">The <see cref="Interval"/> to divide (the divisor).</param>
        /// <returns>The <see cref="T:IntervalArithmetic.Interval"/> that is the <c>x</c> / <c>y</c>.</returns>
        public static Interval operator /(Interval x, Interval y)
        {
            if (!x.isInterval || !y.isInterval)
                return new Interval();
            if (y.zeroIsInside)
                return new Interval();
            Interval divisor = new Interval(1 / y.b, 1 / y.a);
            // this line fixes a divide by zero to the "correct" sign.
            if (divisor.a < 0 && double.IsInfinity(divisor.b)) divisor.b = double.NegativeInfinity;
            //
            return x * divisor;
        }

        #endregion

        #region Single-valued Functions

        /// <summary>
        /// Returns a specified interval raised to the specified power.
        /// </summary>
        /// <returns>The interval x raised to the power p.</returns>
        /// <param name="x">An interval to be raised to a power.</param>
        /// <param name="p">An integer number that specifies a power.</param>
        public static Interval Pow(Interval x, int p)
        {
            if (!x.isInterval)
                return new Interval();
            // zero exponent
            if (p == 0)
                return new Interval(1);
            // positive exponent
            if (p > 0)
            {
                if (IsEven(p) && x.zeroIsInside)
                    return new Interval(0, Math.Max(Math.Pow(x.a, p), Math.Pow(x.b, p)));
                else
                    return new Interval(Math.Pow(x.a, p), Math.Pow(x.b, p));
            }
            // negative exponent
            if (x.zeroIsInside)
                return new Interval();
            Interval temp = Pow(x, -p);
            return new Interval(1 / temp.b, 1 / temp.a);
        }

        /// <summary>
        /// Returns the square root of a specified interval.
        /// </summary>
        /// <returns>The square root.</returns>
        /// <param name="x">The interval whose square root is to be found.</param>
        public static Interval Sqrt(Interval x)
        {
            if (!x.isInterval)
                return new Interval();
            if (x.a < 0)
                return new Interval();
            return new Interval(Math.Sqrt(x.a), Math.Sqrt(x.b));
        }

        /// <summary>
        /// Returns e raised to the specified interval.
        /// </summary>
        /// <returns>e raised to the specified interval.</returns>
        /// <param name="x">The interval specifying the power.</param>
        public static Interval Exp(Interval x)
        {
            if (!x.isInterval)
                return new Interval();
            if (x.a < 0)
                return new Interval();
            return new Interval(Math.Exp(x.a), Math.Exp(x.b));
        }

        /// <summary>
        /// Returns a specified number raised to the specified power (as interval).
        /// </summary>
        /// <returns>The interval.</returns>
        /// <param name="r">The number to be raised to a power.</param>
        /// <param name="x">The interval specifying the power.</param>
        public static Interval Pow(double r, Interval x)
        {
            if (!x.isInterval)
                return new Interval();
            if (x.a < 0)
                return new Interval();
            return new Interval(Math.Pow(r, x.a), Math.Pow(r, x.b));
        }

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified interval.
        /// </summary>
        /// <returns>The natural logarithm of a specified interval.</returns>
        /// <param name="x">The interval whose logarithm is to be found.</param>
        public static Interval Log(Interval x)
        {
            if (!x.isInterval)
                return new Interval();
            if (x.a < 0)
                return new Interval();
            return new Interval(Math.Log(x.a), Math.Log(x.b));
        }

        /// <summary>
        /// Returns the base 10 logarithm of a specified interval.
        /// </summary>
        /// <returns>The base 10 logarithm of a specified interval.</returns>
        /// <param name="x">The interval whose logarithm is to be found.</param>
        public static Interval Log10(Interval x)
        {
            if (!x.isInterval)
                return new Interval();
            if (x.a < 0)
                return new Interval();
            return new Interval(Math.Log10(x.a), Math.Log10(x.b));
        }

        /// <summary>
        /// Returns the logarithm of a specified interval in a specified base.
        /// </summary>
        /// <returns>The logarithm of a specified interval.</returns>
        /// <param name="x">An interval whose logarithm is to be found.</param>
        /// <param name="c">The base of the logarithm.</param>
        public static Interval Log(Interval x, double c)
        {
            if (!x.isInterval)
                return new Interval();
            if (x.a < 0)
                return new Interval();
            if (c <= 1)
                return new Interval();
            return new Interval(Math.Log(x.a, c), Math.Log(x.b, c));
        }

        #endregion

        #region ToString()

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        public override string ToString()
        {
            return this.ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The formated string representation of the value of this instance.</returns>
        /// <param name="format">The format.</param>
        /// <remarks>Implementation of <c>IFormattable</c>.</remarks>
        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The formated string representation of the value of this instance.</returns>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <remarks>Implementation of <c>IFormattable</c>.</remarks>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (String.IsNullOrEmpty(format))
                format = "G";
            if (formatProvider == null)
                formatProvider = CultureInfo.CurrentCulture;
            if (!isInterval)
                return "<no interval>";
            return "[" + a.ToString(format, formatProvider) + " , " + b.ToString(format, formatProvider) + "]";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs some preparatory work on instatiation.
        /// </summary>
        void Consolidate()
        {
            if (!isInterval) return;
            Order();
            CheckZero();
        }

        /// <summary>
        /// Sorts the upper and lower endpoints in correct order.
        /// </summary>
        void Order()
        {
            if (a > b)
            {
                double temp = b;
                b = a;
                a = temp;
            }
            return;
        }

        /// <summary>
        /// Checks if the zero is inside the interval.
        /// Endpoints are not considered to be inside.
        /// </summary>
        void CheckZero()
        {
            zeroIsInside = (a < 0 && b > 0);
        }

        /// <summary>
        /// Checks if an integer value is even.
        /// </summary>
        /// <remarks>A helper function for Pow().</remarks>
        /// <returns><c>true</c>, if value is even, <c>false</c> otherwise.</returns>
        /// <param name="value">The integer to be checked.</param>
        static bool IsEven(int value)
        {
            return value % 2 == 0;
        }

        #endregion
    }
}
