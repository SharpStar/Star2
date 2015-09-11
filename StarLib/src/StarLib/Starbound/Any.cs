using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Networking;
using System.Reflection;

namespace StarLib.Starbound
{
    public abstract class Any
    {
        private object _value;

        public byte Index { get; protected set; }

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                Type[] gArgs = this.GetType().GetGenericArguments();
                var selType = (from a in Enumerable.Range(0, gArgs.Length)
                               where gArgs[a].IsInstanceOfType(value)
                               select new
                               {
                                   Index = a
                               }).SingleOrDefault();
                
                if (selType == null)
                    throw new InvalidOperationException();

                Index = (byte)(selType.Index + 1);
                _value = value;
            }
        }

        public T GetValue<T>()
        {
            return (T)Value;
        }

        public bool TryGetValue<T>(out T value)
        {
            value = default(T);

            if (!(Value is T))
                return false;

            value = (T)Value;

            return true;
        }
    }

    public class Any<T1, T2> : Any
    {
    }

    public class Any<T1, T2, T3> : Any
    {
    }

    public class Any<T1, T2, T3, T4> : Any
    {
    }

    public class Any<T1, T2, T3, T4, T5> : Any
    {
    }

    public class Any<T1, T2, T3, T4, T5, T6> : Any
    {
    }
}
