/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: DateTimeFunctions.cs					    		        
 *        Class(es): DateTimeFunctions
 *          Purpose: Date time related built in functions                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 19 Jul 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2022 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using JTran.Extensions;

namespace JTran.Expressions
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal class DateTimeFunctions
    {
        private static readonly DateTime _daydexBase = new DateTime(1900, 1, 1);

        /*****************************************************************************/
        public string CurrentDatetime()
        {
            return DateTime.Now.ToString("o");
        }

        /*****************************************************************************/
        public string CurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        /*****************************************************************************/
        public string CurrentDatetimeUTC()
        {
            return DateTime.UtcNow.ToString("o");
        }

        /*****************************************************************************/
        public string CurrentDateUTC()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd");
        }

        /*****************************************************************************/
        public string Date(object data)
        {
            if(data == null)
                return "";

            var sdate = data.ToString();

            if(!sdate.TryParseDateTime(out DateTime dtValue))
                return sdate;

            return dtValue.Date.ToString("yyyy-MM-dd");
        }

        /*****************************************************************************/
        public string DateTimeUtc(object data)
        {
            if(data == null)
                return "";

            var sdate = "";

            if(data is DateTime dtValue1)
            {
                sdate = dtValue1.ToString("o");

                if(dtValue1.Kind == DateTimeKind.Utc)
                    return sdate;
            }
            else
                sdate = data.ToString();

            if(DateTimeOffset.TryParse(sdate, out DateTimeOffset dtoValue))
            {
                return dtoValue.UtcDateTime.ToString("o");
            }

            if(DateTime.TryParse(sdate, out DateTime dtValue))
                return dtValue.ToString("o");

            return sdate;
        }

        /*****************************************************************************/
        public string AddYears(object data, int amount)
        {
            return Add(data, (dt)=> dt.AddYears(amount) );
        }

        /*****************************************************************************/
        public string AddMonths(object data, int amount)
        {
            return Add(data, (dt)=> dt.AddMonths(amount) );
        }

        /*****************************************************************************/
        public string AddDays(object data, int amount)
        {
            return Add(data, (dt)=> dt.AddDays(amount) );
        }

        /*****************************************************************************/
        public string AddHours(object data, int amount)
        {
            return Add(data, (dt)=> dt.AddHours(amount) );
        }

        /*****************************************************************************/
        public string AddMinutes(object data, int amount)
        {
            return Add(data, (dt)=> dt.AddMinutes(amount) );
        }

        /*****************************************************************************/
        public string AddSeconds(object data, int amount)
        {
            return Add(data, (dt)=> dt.AddSeconds(amount) );
        }

        /*****************************************************************************/
        public string FormatDateTime(object data, string format)
        {
            if(data == null)
                return "";

            if(!DateTime.TryParse(data.ToString(), out DateTime dtValue))
                return data.ToString();
         
            return dtValue.ToString(format);
        }

        /*****************************************************************************/
        /// <summary>
        /// Number of days since Jan 1, 1900
        /// </summary>
        /// <param name="data">A string representation of a datetime</param>
        public int Daydex(object data)
        {
            return Component(data, (dt)=> (dt.Date - _daydexBase).Days, true);
        }

        /*****************************************************************************/
        public int Year(object data)
        {
            return Component(data, (dt)=> dt.Year );
        }

        /*****************************************************************************/
        public int Month(object data)
        {
            return Component(data, (dt)=> dt.Month );
        }

        /*****************************************************************************/
        public int DayOfWeek(object data)
        {
            return Component(data, (dt)=> (int)dt.DayOfWeek );
        }

        /*****************************************************************************/
        public int DayOfWeekOccurrence(object data)
        {
            return Component(data, (dt)=> dt.DayOfWeekOccurrence() );
        }

        /*****************************************************************************/
        public int Day(object data)
        {
            return Component(data, (dt)=> dt.Day );
        }

        /*****************************************************************************/
        public int Hour(object data)
        {
            return Component(data, (dt)=> dt.Hour );
        }

        /*****************************************************************************/
        public int Minute(object data)
        {
            return Component(data, (dt)=> dt.Minute );
        }

        /*****************************************************************************/
        public int Second(object data)
        {
            return Component(data, (dt)=> dt.Second );
        }

        #region Private Methods

        /*****************************************************************************/
        private static string Add(object data, Func<DateTime, DateTime> fn)
        {
            if(data == null)
                return "";

            var sdate = data.ToString();

            if(!sdate.TryParseDateTime(out DateTime dtValue))
                return sdate;
         
            return fn(dtValue).ToString("o");
        }

        /*****************************************************************************/
        private static int Component(object data, Func<DateTime, int> fn, bool zeroIfMin = false)
        {
            if(data == null)
                return 0;

            if(!data.ToString().TryParseDateTime(out DateTime dtValue))
                return 0;

            if(zeroIfMin && dtValue == DateTime.MinValue)
                return 0;
     
            return fn(dtValue);
        }

        #endregion
    }
}
