/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: DateTimeFunctions.cs					    		        
 *        Class(es): DateTimeFunctions
 *          Purpose: Date time related built in functions                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 19 Jul 2020                                             
 *                                                                          
 *   Copyright (c) 2020 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;

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
            return DateTime.Now.ToString("s");
        }

        /*****************************************************************************/
        public string CurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        /*****************************************************************************/
        public string CurrentDatetimeUTC()
        {
            return DateTime.UtcNow.ToString("s");
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

            if(!DateTime.TryParse(sdate, out DateTime dtValue))
                return sdate;

            return dtValue.Date.ToString("yyyy-MM-dd");
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

            if(!DateTime.TryParse(sdate, out DateTime dtValue))
                return sdate;
         
            return fn(dtValue).ToString("s");
        }

        /*****************************************************************************/
        private static int Component(object data, Func<DateTime, int> fn, bool zeroIfMin = false)
        {
            if(data == null)
                return 0;

            if(!DateTime.TryParse(data.ToString(), out DateTime dtValue))
                return 0;

            if(zeroIfMin && dtValue == DateTime.MinValue)
                return 0;
     
            return fn(dtValue);
        }

        #endregion
    }
}
