using System;
using System.Collections.Generic;
using System.Linq;

namespace JTran.Random
{
    public class RandomExtensions
    {
        private readonly System.Random _random = new System.Random();

        /// <summary>
        /// Randomly picks a single item from the list
        /// </summary>
        public object PickRandom(IEnumerable<object> list)
        {
            var index = _random.Next(0, list.Count());

            return list.Skip(index).Take(1).Single();
        }

        /// <summary>
        /// Randomly picks a floating point number between two values
        /// </summary>
        public double RandomNumber(double from, double to)
        {
            return from + ((to - from) * _random.NextDouble());
        }

        /// <summary>
        /// Randomly picks an integral point number between two values
        /// </summary>
        public int RandomInteger(int from, int to)
        {
            return _random.Next(from, to);
        }
    }
}
