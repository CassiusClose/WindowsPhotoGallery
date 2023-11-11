using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
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
    public class PrecisionDateTime : IComparable
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

        /**
         * If the new PrecisionDateTime is more precise than the given one,
         * then the constructor will fill in the more detailed date time
         * information.
         * 
         * If fillAtBeginningOfRange is true, then it will fill in times from
         * the beginning of the precision range. So if the precision changes
         * from year to month, the new month will be January.
         * 
         * If fillAtBeginningOfRange is false, then it will fill in times from
         * the end of the precision range. So if the precision changes from
         * year to  month, the new month will be December.
         */
        public PrecisionDateTime(DateTime dt, TimeRange precision, bool fillDataAtBeginningOfRange=true)
        {
            _dt = FillDateTimeAtPrecision(dt, precision, fillDataAtBeginningOfRange);
            Precision = precision;
        }

        /**
         * If the new PrecisionDateTime is more precise than the given one,
         * then the constructor will fill in the more detailed date time
         * information.
         * 
         * If fillAtBeginningOfRange is true, then it will fill in times from
         * the beginning of the precision range. So if the precision changes
         * from year to month, the new month will be January.
         * 
         * If fillAtBeginningOfRange is false, then it will fill in times from
         * the end of the precision range. So if the precision changes from
         * year to  month, the new month will be December.
         */
        public PrecisionDateTime(PrecisionDateTime dt, TimeRange precision, bool fillAtBeginningOfRange=true)
        {
            _dt = FillDateTimeAtPrecision(dt._dt, dt.Precision, fillAtBeginningOfRange);
            Precision = precision;
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
            switch(Precision)
            {
                case TimeRange.Year:
                    return _dt.ToString("yyyy");
                   
                case TimeRange.Month:
                    return _dt.ToString("MMMM yyyy");

                case TimeRange.Day:
                    return _dt.ToString("MMMM dd, yyyy");

                case TimeRange.Hour:
                    return _dt.ToString("hhtt, MMMM dd, yyyy");

                case TimeRange.Minute:
                    return _dt.ToString("hh:mmtt, MMMM dd, yyyy");

                default:
                    return _dt.ToString("hh:mm:sstt, MMMM dd, yyyy");
            }
        }

        public string ToString(string format)
        {
            return _dt.ToString(format);
        }



        /**
         * Create a DateTime object from the given DateTime object. Assumes the given DateTime
         * object has the given precision. Creates timestamp data at higher precision. So if the
         * given DateTime object has Day precision, this function will generate data for hour,
         * minute, & second. fillDataAtBeginningOfRange determines whether the generated data is
         * at the beginning of the precision range (when precision is year, set month to January),
         * or if it's at the end of the precision range (when precisino is year, set month to
         * December).
         */
        protected static DateTime FillDateTimeAtPrecision(DateTime dt, TimeRange precision, bool fillDataAtBeginningOfRange)
        {
            if(fillDataAtBeginningOfRange)
            {
                switch(precision)
                {
                    case TimeRange.Year:
                        return new DateTime(dt.Year, 1, 1);

                    case TimeRange.Month:
                        return new DateTime(dt.Year, dt.Month, 1);

                    case TimeRange.Day:
                        return new DateTime(dt.Year, dt.Month, dt.Day);

                    case TimeRange.Hour:
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);

                    case TimeRange.Minute:
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

                    case TimeRange.Second:
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                }
            }  
            else
            {
                switch (precision)
                {
                    case TimeRange.Year:
                        return new DateTime(dt.Year, 12, 31, 23, 59, 59);

                    case TimeRange.Month:
                        switch(dt.Month)
                        {
                            case 1: case 3: case 5: case 7: case 8: case 10: case 12:
                                return new DateTime(dt.Year, dt.Month, 31, 23, 59, 59);

                            case 4: case 6: case 9: case 11:
                                return new DateTime(dt.Year, dt.Month, 30, 23, 59, 59);

                            case 2:
                                if(DateTime.IsLeapYear(dt.Year))
                                    return new DateTime(dt.Year, dt.Month, 29, 23, 59, 59);
                                else
                                    return new DateTime(dt.Year, dt.Month, 28, 23, 59, 59);
                        }
                        break;

                    case TimeRange.Day:
                        return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);

                    case TimeRange.Hour:
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 59, 59);

                    case TimeRange.Minute:
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 59);

                    case TimeRange.Second:
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                }
            }

            return new DateTime();
        }

        
        public DateTime ToDateTime(bool fillDataAtBeginningOfRange)
        {
            return FillDateTimeAtPrecision(this._dt, Precision, fillDataAtBeginningOfRange);
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


        public static bool operator <=(PrecisionDateTime? a, PrecisionDateTime? b)
        {
            return (a < b || a.Matches(b));
        }

        // TODO It's weird that >= uses Matches instead of ==. Should == not require precisions to be the same?
        public static bool operator >=(PrecisionDateTime? a, PrecisionDateTime? b)
        {
            return (a > b || a.Matches(b));
        }


        /// <summary>
        /// Returns whether the given PrecisionDateTime equals this one, up to its level of precision.
        /// The equality operator requires the two PrecisionDateTimes to have the same precision, but
        /// this allows the argument to have less precision. The equality test will only test up to the
        /// argument's level of precision.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        //TODO This is diff than == because == requries precisions to be equal. Is that desired func?
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

        public override int GetHashCode()
        {
            int code = (int)Precision;

            code += ((int)Year / 100)*10;
            if (Precision != TimeRange.Year)
                return code;

            code += Month * 100;
            if (Precision != TimeRange.Month)
                return code;

            code += Day * 1000;
            if (Precision != TimeRange.Day)
                return code;

            code += Hour * 10000;
            if (Precision != TimeRange.Hour)
                return code;

            code += Minute * 100000;
            if (Precision != TimeRange.Minute)
                return code;

            code += Second * 100000;
            return code;
        }

        public int CompareTo(object? obj)
        {
            if (obj is not PrecisionDateTime)
                throw new ArgumentException("CompareTo() argument is not of type PrecisionDateTime");

            PrecisionDateTime dt = (PrecisionDateTime)obj;

            if (this < dt)
                return -1;

            if (this > dt)
                return 1;

            return 0;
        }
    }
}
