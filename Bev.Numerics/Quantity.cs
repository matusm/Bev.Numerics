using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Bev.Numerics
{
    /// <summary>
    /// Class to implement a physical quantity according to GUM.
    /// Provides static methods for common 
    /// mathematical functions for quantities.
    /// Implements binary operators for uncertainty arithmetic.
    /// </summary>
    /// <remarks>
    /// If an quantity occurs several times in a calculation using parameters,
    /// each occurrence is taken independently then this can lead to an unwanted 
    /// expansion of the resulting uncertainty (correlation problem).
    /// In general, it can be shown that the exact range of values can be 
    /// achieved, if each variable appears only once in an equation.
    /// x*y+x is not equivalent to x*(y+1).
    /// Two Quantity objects are considered equal (==) if their En-value is less than 1.
    /// </remarks>
    public class Quantity : IComparable<Quantity>, IFormattable
    {
        #region Private Fields
        /// <summary>
        /// The value of the quantity.
        /// </summary>
        double x;

        /// <summary>
        /// The standard uncertainty associated to the value.
        /// </summary>
        double u;

        /// <summary>
        /// The number of observation of a measurement series.
        /// <c>null</c> for type B uncertainties.
        /// </summary>
        int? n;

        /// <summary>
        /// Expansion factor for IsEquivalent().
        /// </summary>
        static double k = 2;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value of the quantity
        /// </summary>
        /// <value>The value.</value>
        public double X { get { return x; } private set { x = value; } }

        /// <summary>
        /// Gets or sets the standard uncertainty associated to the value.
        /// </summary>
        /// <remarks>
        /// On setting a negative u it is transformed to 0. 
        /// </remarks>
        /// <value>The standard uncertainty.</value>
        public double U { get { return u; } private set { u = value; FixU(); } }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Bev.Quantity.Quantity"/> class.
        /// </summary>
        /// <param name="x">The value.</param>
        /// <param name="u">The standard uncertainty.</param>
        public Quantity(double x, double u)
        {
            X = x;
            U = u;
            n = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bev.Quantity.Quantity"/> class with uncertainty = 0.
        /// </summary>
        /// <param name="x">The value.</param>
        public Quantity(double x) : this(x, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bev.Quantity.Quantity"/> class with value and uncertainty = 0.
        /// </summary>
        public Quantity() : this(0, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bev.Quantity.Quantity"/> class from an array of double.
        /// </summary>
        /// <param name="xi">Series of measurement values.</param>
        public Quantity(double[] xi)
        {
            n = xi.Length;
            X = 0;
            U = 0;
            if (n > 0)
            {
                X = xi.Average();
                if (n > 1)
                {
                    U = xi.StandardDeviation() / Math.Sqrt((double)n);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bev.Quantity.Quantity"/> class from a list of double.
        /// </summary>
        /// <param name="xi">Series of measurement values.</param>
        public Quantity(List<double> xi) : this(xi.ToArray()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bev.Quantity.Quantity"/> class.
        /// </summary>
        /// <param name="a">The lower endpoint.</param>
        /// <param name="b">The upper endpoint.</param>
        /// <param name="pdf">Selector for choosen propability density function.</param>
        public Quantity(double a, double b, string pdf)
        {
            switch (pdf.ToUpper().Trim())
            {
                // rectangular distribution
                case "R":
                    X = (a + b) / 2.0;
                    U = Math.Abs(a - b) / Math.Sqrt(12.0);
                    break;
                // normal distribution
                case "N":
                    X = a;
                    U = b;
                    break;
                // U-shaped distribution
                case "U":
                    X = (a + b) / 2.0;
                    U = Math.Abs(a - b) / Math.Sqrt(8.0);
                    break;
                default:
                    X = 0;
                    U = 0;
                    break;
            }
            n = null;
        }

        #endregion

        #region Operator Overloading: (Quantity, Quantity) -> Quantity

        /// <summary>
        /// Adds a <see cref="Bev.Quantity.Quantity"/> to a <see cref="Bev.Quantity.Quantity"/>, yielding a new <see cref="T:Bev.Quantity.Quantity"/>.
        /// </summary>
        /// <param name="q1">The first <see cref="Bev.Quantity.Quantity"/> to add.</param>
        /// <param name="q2">The second <see cref="Bev.Quantity.Quantity"/> to add.</param>
        /// <returns>The <see cref="T:Bev.Quantity.Quantity"/> that is the sum of the values of <c>q1</c> and <c>q2</c>.</returns>
        public static Quantity operator +(Quantity q1, Quantity q2)
        {
            double uTemp = Math.Sqrt(q1.u * q1.u + q2.u * q2.u);
            return new Quantity(q1.x + q2.x, uTemp);
        }


        /// <summary>
        /// Subtracts a <see cref="Bev.Quantity.Quantity"/> from a <see cref="Bev.Quantity.Quantity"/>, yielding a new <see cref="T:Bev.Quantity.Quantity"/>.
        /// </summary>
        /// <param name="q1">The <see cref="Bev.Quantity.Quantity"/> to subtract from (the minuend).</param>
        /// <param name="q2">The <see cref="Bev.Quantity.Quantity"/> to subtract (the subtrahend).</param>
        /// <returns>The <see cref="T:Bev.Quantity.Quantity"/> that is the <c>q1</c> minus <c>q2</c>.</returns>
        public static Quantity operator -(Quantity q1, Quantity q2)
        {
            double uTemp = Math.Sqrt(q1.u * q1.u + q2.u * q2.u);
            return new Quantity(q1.x - q2.x, uTemp);
        }

        /// <summary>
        /// Computes the product of <c>q1</c> and <c>q2</c>, yielding a new <see cref="T:Bev.Quantity.Quantity"/>.
        /// </summary>
        /// <param name="q1">The <see cref="Bev.Quantity.Quantity"/> to multiply.</param>
        /// <param name="q2">The <see cref="Bev.Quantity.Quantity"/> to multiply.</param>
        /// <returns>The <see cref="T:Bev.Quantity.Quantity"/> that is the <c>q1</c> * <c>q2</c>.</returns>
        public static Quantity operator *(Quantity q1, Quantity q2)
        {
            double uc1, uc2;
            uc1 = q1.u * q2.x;
            uc2 = q2.u * q1.x;
            double uTemp = Math.Sqrt(uc1 * uc1 + uc2 * uc2);
            return new Quantity(q1.x * q2.x, uTemp);
        }

        /// <summary>
        /// Computes the division of <c>q1</c> and <c>q2</c>, yielding a new <see cref="T:Bev.Quantity.Quantity"/>.
        /// </summary>
        /// <param name="q1">The <see cref="Bev.Quantity.Quantity"/> to divide (the divident).</param>
        /// <param name="q2">The <see cref="Bev.Quantity.Quantity"/> to divide (the divisor).</param>
        /// <returns>The <see cref="T:Bev.Quantity.Quantity"/> that is the <c>q1</c> / <c>q2</c>.</returns>
        public static Quantity operator /(Quantity q1, Quantity q2)
        {
            double uc1, uc2;
            uc1 = (q1.u / q2.x);
            uc2 = (q2.u * q1.x / (q2.x * q2.x));
            double uTemp = Math.Sqrt(uc1 * uc1 + uc2 * uc2);
            return new Quantity(q1.x / q2.x, uTemp);
        }

        public static bool operator ==(Quantity q1, Quantity q2)
        { return q1.Equals(q2); }
        public static bool operator !=(Quantity q1, Quantity q2)
        { return !q1.Equals(q2); }
        public static bool operator >=(Quantity q1, Quantity q2)
        {
            if (q1 == q2)
                return true;
            if (q1.X >= q2.X)
                return true;
            return false;
        }
        public static bool operator <=(Quantity q1, Quantity q2)
        {
            if (q1 == q2)
                return true;
            if (q1.X <= q2.X)
                return true;
            return false;
        }
        public static bool operator >(Quantity q1, Quantity q2)
        {
            if (q1 == q2)
                return false;
            if (q1.X > q2.X)
                return true;
            return false;
        }
        public static bool operator <(Quantity q1, Quantity q2)
        {
            if (q1 == q2)
                return false;
            if (q1.X < q2.X)
                return true;
            return false;
        }

        #endregion

        #region Conversion Operators: Quantity <-> double

        /// <summary>
        /// Explicit cast from <c>Quantity</c> to <c>double</c>.
        /// </summary>
        /// <param name="q">A <c>double</c> representing the value without uncertainty.</param>
        /// <remarks>Information is lost! -> explicit.</remarks>
        public static explicit operator double(Quantity q) { return q.x; } // information is lost! -> explicit

        /// <summary>
        /// Implicit cast from <c>double</c> to <c>Quantity</c>.
        /// </summary>
        /// <param name="q">A <c>Quantity</c> with zero uncertainty.</param>
        public static implicit operator Quantity(double q) { return new Quantity(q); }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates degree of equivalence between two given quantities.
        /// </summary>
        /// <param name="q1">The first quantity.</param>
        /// <param name="q2">The second quantity.</param>
        /// <returns>The En value.</returns>
        public static double En(Quantity q1, Quantity q2)
        {
            Quantity delta = q1 - q2;
            return delta.X / delta.U;
        }

        /// <summary>
        /// Calculates degree of equivalence between this and a given quantities.
        /// </summary>
        /// <param name="q2">The second quantity.</param>
        /// <returns>The En value.</returns>
        public double En(Quantity q2)
        {
            Quantity delta = this - q2;
            return delta.X / delta.U;
        }

        /// <summary>
        /// Checks if two quantities are "equal" considering their uncertainties.
        /// </summary>
        /// <param name="q1">The first quantity.</param>
        /// <param name="q2">The second quantity.</param>
        /// <returns><c>true</c>, if the quantities are equivalent, <c>false</c> otherwise.</returns>
        public static bool IsEquivalent(Quantity q1, Quantity q2)
        {
            Quantity delta = q1 - q2;
            return (Math.Abs(delta.x) <= k * delta.u);
        }

        /// <summary>
        /// Checks if a quantity is "equal" with this object considering the uncertainties.
        /// </summary>
        /// <param name="q2">The second quantity.</param>
        /// <returns><c>true</c>, if the quantities are equivalent, <c>false</c> otherwise.</returns>
        public bool IsEquivalent(Quantity q2)
        {
            Quantity delta = this - q2;
            return (Math.Abs(delta.x) <= k * delta.u);
        }

        /// <summary>
        /// Checks if q is equal to zero considering it's uncertainty.
        /// </summary>
        /// <param name="q"></param>
        /// <returns><c>true</c>, if the quantity is equivalent to zero, <c>false</c> otherwise.</returns>
        public static bool IsZero(Quantity q)
        {
            return IsEquivalent(q, new Quantity());
        }

        /// <summary>
        /// Checks if this quantity is equal to zero considering it's uncertainty.
        /// </summary>
        /// <returns><c>true</c>, if this quantity is equivalent to zero, <c>false</c> otherwise.</returns>
        public bool IsZero()
        {
            return this.IsEquivalent(new Quantity());
        }

        #endregion

        #region Overrides and implementations

        /// <summary>
        /// Implementation of IComparable
        /// </summary>
		/// <remarks>
		/// Sorting according to the value. More strict than using the overload operators</remarks>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Quantity other)
        {
            if (this.x < other.x) return -1;
            if (this.x == other.x) return 0;
            return 1;
        }

        /// <summary>
        /// Implementation of <c>IFormattable</c> with default (= no) formatting.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Implementation of <c>IFormattable</c>.
        /// </summary>
        /// <param name="format">Format.</param>
        /// <returns></returns>
        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Implementation of <c>IFormattable</c>.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="format">Format.</param>
        /// <param name="formatProvider">Format provider.</param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (String.IsNullOrEmpty(format)) format = "G";
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            char[] sep = { '.' };
            string[] token = format.Trim().Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string unit = "";
            string prec = "G";

            if (token.Length == 2)
            {
                unit = FormatUnit(token[0]);
                prec = FormatPrec(token[1]);
            }

            if (token.Length == 1)
            {
                unit = FormatUnit(token[0]);
                prec = "G";
            }

            string output = "(" + X.ToString(prec, formatProvider) + unit + " ± " + U.ToString(prec, formatProvider) + unit + ")";
            return output;

        }

        /// <summary>
        /// override object.Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            return IsEquivalent((Quantity)obj);
        }

        /// <summary>
        /// override object.GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            // IsEquivalent is _not_ an equivalence relation
            // GetHashCode can not be formulated in a sense-full way
            return 1;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Uncertainty must always be positive. Set to zero if negative.
        /// </summary>
        void FixU() { if (u < 0) u = 0; }

        /// <summary>
        /// Precedes an unit symbol with a space, with exception of certain angle units.
        /// </summary>
        /// <returns>The formated unit symbol.</returns>
        /// <param name="symbol">The unit symbol.</param>
        /// <remarks>Used by <c>ToString()</c> only.</remarks>
        string FormatUnit(string symbol)
        {
            switch (symbol)
            {
                case "G":
                    return "";
                case "°":
                case "'":
                case "''":
                    return symbol;
                default:
                    return " " + symbol;
            }
        }

        /// <summary>
        /// Formats the precision string.
        /// </summary>
        /// <returns>The string to be analyzed.</returns>
        /// <param name="pr">The <c>double</c> format specification.</param>
        /// <remarks>Used by <c>ToString()</c> only.</remarks>
        string FormatPrec(string pr)
        {
            pr = pr.Trim();
            if (pr == null || pr.Length == 0) return "G";
            int n;
            if (!Int32.TryParse(pr, out n)) return "G";
            return "F" + pr;
        }

        #endregion
    }
}
