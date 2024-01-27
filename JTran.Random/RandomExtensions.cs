﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace JTran.Random
{
    public class RandomExtensions
    {
        private readonly System.Random _random = new System.Random();
        private Dictionary<string, Dictionary<int, bool>> _uniqueTracker = new Dictionary<string, Dictionary<int, bool>>();

        /// <summary>
        /// Randomly picks a single item from the list
        /// </summary>
        public object PickRandom(object parm)
        {
            var list = EnsureList(parm);
            var index = _random.Next(0, list.Count());

            return list.Skip(index).Take(1).Single();
        }

        /// <summary>
        /// Randomly picks a single item from the list but never returns the same item
        /// </summary>
        public object PickRandomUnique(object parm, string name)
        {
            var list = EnsureList(parm);

            if(!_uniqueTracker.ContainsKey(name))
                _uniqueTracker[name] = new Dictionary<int, bool>();

            var index = -1;

            do
            { 
                // If we've used them all we'll just have to start over otherwise this would result in an infinite loop
                if(_uniqueTracker[name].Count >= list.Count())
                   _uniqueTracker[name].Clear();
                   
                index = _random.Next(0, list.Count());
            }
            while(_uniqueTracker[name].ContainsKey(index));

            _uniqueTracker[name].Add(index, true);

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

        private IEnumerable<object> EnsureList(object obj)
        {
            if(obj is null)
                return null;

            if(obj is IEnumerable<object> list)
                return list;

            return new List<object> { obj };
        }
    }
}
