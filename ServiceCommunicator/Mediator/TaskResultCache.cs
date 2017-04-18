using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Mediator
{
    public static class TaskResultCache
    {
        private static readonly Dictionary<Type, PropertyInfo> _resultCache = new Dictionary<Type, PropertyInfo>();

        public static object GetTaskResult(Task task)
        {
            var taskType = task.GetType();
            if (_resultCache.ContainsKey(taskType) == false)
            {
                _resultCache.Add(taskType, taskType.GetProperty("Result"));
            }

            var propertyInfo = _resultCache[taskType];
            if (propertyInfo == null)
            {
                return null;
            }

            return propertyInfo.GetValue(task);
        }

        public static Type GetTaskResultType(Type taskType)
        {
            if (_resultCache.ContainsKey(taskType) == false)
            {
                _resultCache.Add(taskType, taskType.GetProperty("Result"));
            }

            var propertyInfo = _resultCache[taskType];
            if (propertyInfo == null)
            {
                return null;
            }

            return propertyInfo.PropertyType;
        }
    }
}
