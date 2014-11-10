using System.Collections;
using System.Collections.Generic;

namespace opennlp.tools.nonjava.extensions
{
    public static class IListExtensionMethods
    {
        public static void AddRange<T>(this IList<T> self, IList<T> other)
        {
            foreach (var item in other)
            {
                self.Add(item);
            }
        }
    }
}
