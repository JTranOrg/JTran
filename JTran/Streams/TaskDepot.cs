/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: TaskDepot.cs					    		        
 *        Class(es): TaskDepot				         		            
 *          Purpose: Class for managing completion of tasks                   
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 5 Apr 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JTran.Streams
{
    /****************************************************************************/
    /****************************************************************************/
    internal class TaskDepot : IAsyncDisposable
    {
        private List<Task>?      _tasks;
        private List<TaskDepot>? _depots;
        private bool             _split  = false;
        private int              _roundRobinIndex = 0;
        private const int        Max = 100;

        /****************************************************************************/
        internal TaskDepot() 
        {
            _tasks = new List<Task>();
        }

        /****************************************************************************/
        internal protected TaskDepot(List<Task> tasks) 
        {
            _tasks = tasks;
        }

        /****************************************************************************/
        private bool IsFull => _split && _depots!.Count == Max && _depots[Max-1].IsFullOfTasks;
        
        /****************************************************************************/
        private bool IsFullOfTasks => !_split && _tasks!.Count == Max; 

        /****************************************************************************/
        internal void Add(Task task)
        {
            if(!_split)
            { 
                if(_tasks!.Count < Max)
                { 
                    _tasks.Add(task);
                    return;
                }

                var newDepot1 = new TaskDepot(_tasks);
                var newDepot2 = new TaskDepot();

                _tasks = null;

                _depots = [newDepot1, newDepot2];

                newDepot2.Add(task);

                _split = true;

                return;
            }
            
            if(!IsFull)
            { 
                var last = _depots!.Last();

                if(!last.IsFull)
                { 
                    if(last._tasks != null && last._tasks.Count < Max)
                    { 
                        _depots!.Last().Add(task);  
                        return;
                    }
                }

                if(_depots!.Count < Max)
                { 
                    var newDepot = new TaskDepot();

                    _depots!.Add(newDepot);
                    newDepot.Add(task);

                    return;
                }
            }

            if(_roundRobinIndex == Max)
                _roundRobinIndex = 0;

            _depots![_roundRobinIndex++].Add(task);
        }

        /****************************************************************************/
        private async Task DisposeTaskAsync()
        {
            if(_tasks != null)
                await Task.WhenAll(_tasks);

            if(_depots != null)
                await Task.WhenAll(_depots.Select( d=> d.DisposeTaskAsync()));          
        }

        /****************************************************************************/
        public async ValueTask DisposeAsync()
        {
            await DisposeTaskAsync();
        }
    }    
}
