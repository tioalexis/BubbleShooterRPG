using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Cyl.Common.Utils
{
    public static class AwaitableExtensions
    {
        public static async Task AsTask(this Awaitable a)
        {
            await a;
        }

        public static async Task<T> AsTask<T>(this Awaitable<T> a)
        {
            return await a;
        }
        
        public static void FireAndForget(this Awaitable a, Action onSuccess = null, Action<Exception> onError = null)
        {
            a.AsTask().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    if (onError != null)
                    {
                        onError(task.Exception);
                    }
                    else
                    {
                        Debug.LogException(task.Exception);
                    }
                }
                else
                {
                    onSuccess?.Invoke();
                }
            });
        }
        
        public static void FireAndForget<T>(this Awaitable<T> a, Action<T> onSuccess = null, Action<Exception> onError = null)
        {
            a.AsTask().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    if (onError != null)
                    {
                        onError(task.Exception);
                    }
                    else
                    {
                        Debug.LogException(task.Exception);
                    }
                }
                else
                {
                    onSuccess?.Invoke(task.Result);
                }
            });
        }
    }
}