using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CloudNine.Core.Birthdays
{
    /// <summary>
    /// Compares the BirthdayDate based upon a current date.
    /// </summary>
    public class BirthdayDateComparer : IComparer<DateTime>
    {
        /// <summary>
        /// The current day to base the comparisons on.
        /// </summary>
        public DateTime CurrentDay { get; set; }

        /// <summary>
        /// Creates a new birthday date comparer using a start date.
        /// </summary>
        /// <param name="startDate">Using the start date, or the inital value in which birthdays are compared to.</param>
        public BirthdayDateComparer(DateTime startDate)
        {
            CurrentDay = startDate;
        }

        /// <summary>
        /// Compares two dates around a current date. Dates before the current date are greater than dates after the start date.
        /// </summary>
        /// <param name="x">First date</param>
        /// <param name="y">Second date</param>
        /// <returns>Value representing the relative positions of the dates.</returns>
        public int Compare([AllowNull] DateTime x, [AllowNull] DateTime y)
        {
            var xCompare = GetComparsionDateTime(x);
            var yCompare = GetComparsionDateTime(y);

            if (xCompare is null && yCompare is null) return 0;
            if (xCompare is null) return -1;
            if (yCompare is null) return 1;

            var xDate = xCompare.Value.Date;
            var yDate = yCompare.Value.Date;

            var yearRes = xDate.Year.CompareTo(yDate.Year);

            if (yearRes == 0)
            {
                var monthRes = xDate.Month.CompareTo(yDate.Month);
                if (monthRes == 0)
                {
                    return xDate.Day.CompareTo(yDate.Day);
                }
                else return monthRes;
            }
            else return yearRes;
        }

        /// <summary>
        /// Retuns a modified DateTime where the year is representative of the position compared to the StartDate.
        /// </summary>
        /// <param name="d">Date to convert.</param>
        /// <returns>A Converted DateTime.</returns>
        private DateTime? GetComparsionDateTime(DateTime d)
        {
            if (d == DateTime.MinValue) return null;

            if (d.Month > CurrentDay.Month)
                return new DateTime(1, d.Month, d.Day);
            if (d.Month < CurrentDay.Month)
                return new DateTime(2, d.Month, d.Day);
            if (d.Month == CurrentDay.Month)
            {
                if (d.Day >= CurrentDay.Day)
                    return new DateTime(1, d.Month, d.Day);
                if (d.Day < CurrentDay.Day)
                    return new DateTime(2, d.Month, d.Day);
            }

            return null;
        }
    }
}
