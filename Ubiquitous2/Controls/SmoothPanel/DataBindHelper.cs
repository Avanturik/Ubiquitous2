// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Devart.Controls
{
    /// <summary>
    /// Provides method for instant data binding.
    /// </summary>
    public static class DataBindHelper
    {
        /// <summary>
        /// The pending task status.
        /// </summary>
        public const int PendingStatus = 0;

        /// <summary>
        /// The retry task status.
        /// </summary>
        public const int RetryStatus = 3;

        /// <summary>
        /// The data bind engine.
        /// </summary>
        private static object _dataBindEngine;

        /// <summary>
        /// The head task field from DataBindEngine.
        /// </summary>
        private static FieldInfo _headField;

        /// <summary>
        /// The next task field from DataBindEngine.Task.
        /// </summary>
        private static FieldInfo _nextField;

        /// <summary>
        /// The status field from DataBindEngine.Task.
        /// </summary>
        private static FieldInfo _statusField;

        /// <summary>
        /// The run method from DataBindEngine.Task.
        /// </summary>
        private static MethodInfo _runMethod;

        /// <summary>
        /// Performs the action with instant data binding.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void PerformWithInstantBinding(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var task = GetHeadTask();

            if (task == null)
            {
                action();
                return;
            }

            // Get last added task before action.
            var nextTask = GetNextTask(task);
            while (nextTask != null)
            {
                task = nextTask;
                nextTask = GetNextTask(task);
            }

            // Perform the action.
            action();

            var pendingTasks = new List<object>();
            task = GetNextTask(task);

            // Get all newer pending tasks.
            while (task != null)
            {
                if (GetTaskStatus(task) == PendingStatus)
                {
                    pendingTasks.Add(task);
                }
                task = GetNextTask(task);
            }

            // Run pending tasks.
            foreach (var pendingTask in pendingTasks)
            {
                RunTask(pendingTask, false);
                if (GetTaskStatus(pendingTask) == RetryStatus)
                {
                    SetTaskStatus(pendingTask, PendingStatus);
                }
            }
        }

        /// <summary>
        /// Gets the field information.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="isPublic">If set to <c>true</c> field is public.</param>
        /// <returns>A <see cref="FieldInfo"/>.</returns>
        private static FieldInfo GetFieldInfo(object target, string fieldName, bool isPublic)
        {
            if (target == null)
            {
                return null;
            }
            var flags = BindingFlags.Instance | BindingFlags.IgnoreCase;
            if (isPublic)
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.NonPublic;
            }
            return target.GetType().GetField(fieldName, flags);
        }

        /// <summary>
        /// Gets the head task.
        /// </summary>
        /// <returns>A <see cref="DataBindEngine.Task"/> instance.</returns>
        private static object GetHeadTask()
        {
            if (_headField == null)
            {
                var engineType = typeof(System.Windows.Data.BindingExpression).Assembly.GetType("MS.Internal.Data.DataBindEngine");
                if (engineType != null)
                {
                    var engineField = engineType.GetProperty("CurrentDataBindEngine", BindingFlags.Static | BindingFlags.NonPublic);
                    if (engineField != null)
                    {
                        _dataBindEngine = engineField.GetValue(null, null);

                        // Binding tasks are stored as linked list (class DataBindEngine.Task). Get the head field.
                        if (_dataBindEngine != null)
                        {
                            _headField = GetFieldInfo(_dataBindEngine, "_head", false);
                        }
                    }
                }
            }

            if (_headField != null)
            {
                // Return head task.
                return _headField.GetValue(_dataBindEngine);
            }
            return null;
        }

        /// <summary>
        /// Gets the next task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A <see cref="DataBindEngine.Task"/> instance.</returns>
        private static object GetNextTask(object task)
        {
            if (_nextField == null)
            {
                _nextField = GetFieldInfo(task, "Next", true);
            }
            if (_nextField != null)
            {
                // Return next task in chain.
                return _nextField.GetValue(task);
            }
            return null;
        }

        /// <summary>
        /// Gets the task status.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A <see cref="DataBindEngine.Task.Status"/> casted to <see cref="Int32"/>.</returns>
        private static int GetTaskStatus(object task)
        {
            if (_statusField == null)
            {
                _statusField = GetFieldInfo(task, "status", true);
            }
            if (_statusField != null)
            {
                var value = _statusField.GetValue(task);

                // Cast from DataBindEngine.Task.Status enum to int.
                if (value is Enum)
                {
                    return (int)value;
                }
            }
            return -1;
        }

        /// <summary>
        /// Sets the task status.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="status">The status.</param>
        private static void SetTaskStatus(object task, int status)
        {
            if (_statusField == null)
            {
                _statusField = GetFieldInfo(task, "status", true);
            }
            if (_statusField != null)
            {
                // Cast from int to DataBindEngine.Task.Status
                _statusField.SetValue(task, Enum.ToObject(_statusField.FieldType, status));
            }
        }

        /// <summary>
        /// Runs the task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="lastChance">Value indicating first or last attempt.</param>
        private static void RunTask(object task, bool lastChance)
        {
            if (_runMethod == null && task != null)
            {
                _runMethod = task.GetType().GetMethod(
                    "Run",
                    BindingFlags.Instance | BindingFlags.Public);
            }
            if (_runMethod != null)
            {
                _runMethod.Invoke(task, new object[] { lastChance });
            }
        }
    }
}