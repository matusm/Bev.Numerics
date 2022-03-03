using System;

namespace Bev.Numerics
{
    // TODO
    //   FirstDate -> StartDate
    //   Update() -> Add(); Insert()
    //   Designation -> ID; Name; Title; Label; Description; Tag

    public class DataSeriesPod
    {

        #region Ctor

        public DataSeriesPod(string designation)
        {
            Designation = designation.Trim();
            if (string.IsNullOrEmpty(Designation))
            {
                Designation = "<unknown>";
            }
            Restart();
        }

        public DataSeriesPod() : this("") { }

        #endregion

        #region Properties

        public double AverageValue { get { return GetValueIfValid(sumValue / Count); } }
        public double Scatter { get { return GetValueIfValid((maximumValue - minimumValue) / 2.0); } }
        public double FirstValue { get; private set; }
        public double MostRecentValue { get; private set; }
        public double MaximumValue { get { return GetValueIfValid(maximumValue); } }
        public double MinimumValue { get { return GetValueIfValid(minimumValue); } }
        public double CentralValue { get { return GetValueIfValid((maximumValue + minimumValue) / 2.0); } }
        public long Count { get; private set; }
        public string Designation { get; private set; }
        public DateTime FirstDate { get; private set; }
        public DateTime MostRecentValueDate { get; private set; }
        public double Duration { get { return GetDurationInSeconds(); } }

        #endregion

        #region Methods

        public void Restart()
        {
            Count = 0;
            sumValue = 0;
            maximumValue = double.MinValue;
            minimumValue = double.MaxValue;
            FirstValue = double.NaN;
            MostRecentValue = double.NaN;
            FirstDate = DateTime.UtcNow;
            MostRecentValueDate = DateTime.UtcNow;
        }

        public void Update(double value)
        {
            if (double.IsNaN(value)) return;
            if (Count >= long.MaxValue - 1) return;
            Count++;
            if (Count == 1)
            {
                FirstValue = value;
                FirstDate = DateTime.UtcNow;
            }
            MostRecentValueDate = DateTime.UtcNow;
            MostRecentValue = value;
            sumValue += value;
            if (value > maximumValue) maximumValue = value;
            if (value < minimumValue) minimumValue = value;
        }

        #endregion

        #region deprecated API

        [Obsolete("Reset() is deprecated, use Restart() instead.", false)]
        public void Reset() { Restart(); }

        [Obsolete("NumberOfSamples is deprecated, use SampleSize instead.", false)]
        public long NumberOfSamples { get { return Count; } }

        [Obsolete("LastDate is deprecated, use MostRecentValueDate instead.", false)]
        public DateTime LastDate { get { return MostRecentValueDate; } }

        [Obsolete("LastValue is deprecated, use MostRecentValue instead.", false)]
        public double LastValue { get { return MostRecentValue; } }

        [Obsolete("MeanValue is deprecated, use AverageValue instead.", false)]
        public double MeanValue { get { return AverageValue; } }

        [Obsolete("MaxValue is deprecated, use MaximumValue instead.", false)]
        public double MaxValue { get { return MaximumValue; } }

        [Obsolete("MinValue is deprecated, use MinimumValue instead.", false)]
        public double MinValue { get { return MinimumValue; } }

        [Obsolete("SampleSize is deprecated, use Count instead.", false)]
        public long SampleSize { get { return Count; } }
        #endregion

        #region private stuff

        private double GetValueIfValid(double value)
        {
            if (Count <= 0)
                return double.NaN;
            return value;
        }

        private double GetDurationInSeconds()
        {
            TimeSpan ts = MostRecentValueDate.Subtract(FirstDate);
            return ts.TotalSeconds;
        }

        private double sumValue;
        private double maximumValue;
        private double minimumValue;

        #endregion

        public override string ToString()
        {
            if (Count > 0)
                return string.Format("{0} : {1} ± {2}", Designation, AverageValue, Scatter);
            else
                return string.Format("{0} : no data yet", Designation);
        }

    }
}
