namespace MiniJson
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class AotSafe
    {
        public static void ForEach<T>(object enumerable, Action<T> action)
        {
            if (enumerable == null) return;
            Type type = enumerable.GetType().GetInterfaces().First((Type x) => !x.IsGenericType && x == typeof(IEnumerable));
            if (type == null) throw new ArgumentException("Object does not implement IEnumerable interface", "enumerable");
            MethodInfo method = type.GetMethod("GetEnumerator");
            if (method == null) throw new InvalidOperationException("Failed to get 'GetEnumberator()' method info from IEnumerable type");
            IEnumerator enumerator = null;
            try
            {
                enumerator = (IEnumerator)method.Invoke(enumerable, null);
                if (enumerator is IEnumerator)
                {
                    while (enumerator.MoveNext()) action((T)enumerator.Current);
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }
    }
}

