using System.Threading.Tasks;

namespace Herald.MessageQueue.Extensions
{
    public static class TaskExtension
    {
        public static Task<T> DefaultIfCanceled<T>(this Task<T> @this, T defaultValue = default(T))
        {
            return
              @this.ContinueWith
                (
                  t =>
                  {
                      if (t.IsCanceled || t.Exception is System.AggregateException)
                          return defaultValue;

                      return t.Result;
                  }
                );
        }
    }
}
