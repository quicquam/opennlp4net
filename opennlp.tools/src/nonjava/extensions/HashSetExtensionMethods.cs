using System;
using System.Collections.Generic;
using opennlp.tools.coref.sim;

namespace opennlp.tools.nonjava.extensions
{
    public static class HashSetExtensionMethods
    {
        public static bool containsAll(this HashSet<string> thisHashSet, HashSet<string> otherHashSet)
        {
            throw new NotImplementedException();
        }

        public static void addAll(this HashSet<Context> thisHashSet, IList<Context> otherHashSet)
        {
            foreach (var context in otherHashSet)
            {
                thisHashSet.Add(context);
            }
        }
    }
}
