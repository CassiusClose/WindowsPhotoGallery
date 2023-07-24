using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    //TODO Switch to PrecisionDateTime in media & event models
    /// <summary>
    /// A DateTime wrapper that allows the user to define the precision that the timestamp is defined for.
    /// So if you only knew the date that a media was created on, and not the time, you would define its
    /// timestamp as a precision of Day.
    /// </summary>
    public class PrecisionDateTime
    {
        public PrecisionDateTime()
        {
            _dt = DateTime.MinValue;
            Precision = TimeRange.Second;
        }

        public PrecisionDateTime(int year)
        {
            _dt = new DateTime(year, 1, 1);
            Precision = TimeRange.Year;
        }

        public PrecisionDateTime(int year, int month)
        {
            _dt = new DateTime(year, month, 1);
            Precision = TimeRange.Month;
        }

        public PrecisionDateTime(int year, int month, int day)
        {
            _dt = new DateTime(year, month, day);
            Precision = TimeRange.Day;
        }

        public PrecisionDateTime(int year, int month, int day, int hour)
        {
            _dt = new DateTime(year, month, day, hour, 0, 0);
            Precision = TimeRange.Hour;
        }

        public PrecisionDateTime(int year, int month, int day, int hour, int minute)
        {
            _dt = new DateTime(year, month, day, hour, minute, 0);
            Precision = TimeRange.Minute;
        }

        public PrecisionDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            _dt = new DateTime(year, month, day, hour, minute, second);
            Precision = TimeRange.Second;
        }

        public PrecisionDateTime(DateTime dt, TimeRange precision)
        {
            switch (precision)
            {
                case TimeRange.Year:
                    _dt = new DateTime(dt.Year, 1, 1);
                    break;

                case TimeRange.Month:
                    _dt = new DateTime(dt.Year, dt.Month, 1);
                    break;

                case TimeRange.Day:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day);
                    break;

                case TimeRange.Hour:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                    break;

                case TimeRange.Minute:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
                    break;

                case TimeRange.Second:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    break;
            }

            Precision = precision;
        }

        public PrecisionDateTime(PrecisionDateTime dt, TimeRange precision)
        {
            Precision = precision;
            if (dt.Precision < precision)
                Trace.WriteLine("Warning: Trying to create a PrecisionDateTime from an existing PrecisionDateTime with less precision");

            switch (precision)
            {
                case TimeRange.Year:
                    _dt = new DateTime(dt.Year, 1, 1);
                    break;

                case TimeRange.Month:
                    _dt = new DateTime(dt.Year, dt.Month, 1);
                    break;

                case TimeRange.Day:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day);
                    break;

                case TimeRange.Hour:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
                    break;

                case TimeRange.Minute:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
                    break;

                case TimeRange.Second:
                    _dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    break;
            }
        }




        private DateTime _dt;

        /// <summary>
        /// Up to what range of time is this timestamp defined.
        /// </summary>
        public TimeRange Precision
        {
            get; internal set;
        }


        public int Year { get { return _dt.Year; } }
        public int Month { get { return _dt.Month; } }
        public int Day { get { return _dt.Day; } }
        public int Hour { get { return _dt.Hour; } }
        public int Minute { get { return _dt.Minute; } }
        public int Second { get { return _dt.Second; } }



        public override string ToString()
        {
            return _dt.ToString();
        }

        public string ToString(string format)
        {
            return _dt.ToString(format);
        }


        /// <summary>
        /// Returns whether the first PrecisionDateTime is less than the second one.
        /// If their precisions are not the same, the one with the same time fields
        /// but less precision is treated as earlier.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(PrecisionDateTime a, PrecisionDateTime b)
        {
            if (a.Year < b.Year)
                return true;

            if (a.Year > b.Year)
                return false;

            if (a.Precision == TimeRange.Year)
            {
                if (b.Precision == TimeRange.Year)
                    return false;
                else
                    return true;
            }
            else if (b.Precision <= TimeRange.Year)
                return false;


            if (a.Month < b.Month)
                return true;

            if (a.Month > b.Month)
                return false;

            if (a.Precision == TimeRange.Month)
            {
                if (b.Precision == TimeRange.Month)
                    return false;
                else
                    return true;
            }
            else if (b.Precision <= TimeRange.Month)
                return false;


            if (a.Day < b.Day)
                return true;

            if (a.Day > b.Day)
                return false;

            if (a.Precision == TimeRange.Day)
            {
                if (b.Precision == TimeRange.Day)
                    return false;
                else
                    return true;
            }
            else if (b.Precision <= TimeRange.Day)
                return false;


            if (a.Hour < b.Hour)
                return true;

            if (a.Hour > b.Hour)
                return false;

            if (a.Precision == TimeRange.Hour)
            {
                if (b.Precision == TimeRange.Hour)
                    return false;
                else
                    return true;
            }
            else if (b.Precision <= TimeRange.Hour)
                return false;


            if (a.Minute < b.Minute)
                return true;

            if (a.Minute > b.Minute)
                return false;

            if (a.Precision == TimeRange.Minute)
            {
                if (b.Precision == TimeRange.Minute)
                    return false;
                else
                    return true;
            }
            else if (b.Precision <= TimeRange.Minute)
                return false;
            

            if (a.Second < b.Second)
                return true;

            if (a.Second > b.Second)
                return false;

            if (b.Precision <= TimeRange.Second)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether the first PrecisionDateTime is greater than the second one.
        /// If their precisions are not the same, the one with the same time fields
        /// but less precision is treated as earlier.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(PrecisionDateTime a, PrecisionDateTime b)
        {
            return b < a;
        }

        /// <summary>
        /// Returns whether the two PrecisionDateTimes have the same time fields,
        /// up to the their levels of precision. Their precision levels must be
        /// equal as well.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(PrecisionDateTime? a, PrecisionDateTime? b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) ||  ReferenceEquals(b, null)) 
                return false;

            if (a.Precision != b.Precision)
                return false;

            if (a.Year != b.Year)
                return false;
            if (a.Precision == TimeRange.Year)
                return true;

            if (a.Month != b.Month)
                return false;
            if (a.Precision == TimeRange.Month)
                return true;

            if (a.Day != b.Day)
                return false;
            if (a.Precision == TimeRange.Day)
                return true;

            if (a.Hour != b.Hour)
                return false;
            if (a.Precision == TimeRange.Hour)
                return true;

            if (a.Minute != b.Minute)
                return false;
            if (a.Precision == TimeRange.Minute)
                return true;

            if (a.Second != b.Second)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the two PrecisionDateTimes don't have the same time fields,
        /// up to the their levels of precision, or if their precision levels are not
        /// equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(PrecisionDateTime? a, PrecisionDateTime? b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (ReferenceEquals(obj, null))
                return false;

            if (obj is PrecisionDateTime)
                return this == (PrecisionDateTime)obj;

            return false;
        }


        /// <summary>
        /// Returns whether the given PrecisionDateTime equals this one, up to its level of precision.
        /// The equality operator requires the two PrecisionDateTimes to have the same precision, but
        /// this allows the argument to have less precision. The equality test will only test up to the
        /// argument's level of precision.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Matches(PrecisionDateTime b)
        {
            if (Precision < b.Precision)
                return false;

            if (Year != b.Year)
                return false;
            if (b.Precision == TimeRange.Year)
                return true;

            if (Month != b.Month)
                return false;
            if (b.Precision == TimeRange.Month)
                return true;

            if (Day!= b.Day)
                return false;
            if (b.Precision == TimeRange.Day)
                return true;

            if (Hour != b.Hour)
                return false;
            if (b.Precision == TimeRange.Hour)
                return true;

            if (Minute != b.Minute)
                return false;
            if (b.Precision == TimeRange.Minute)
                return true;

            if (Second != b.Second)
                return false;

            return true;
        }
    }
}
