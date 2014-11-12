using System;
using System.Collections.Generic;
using opennlp.model;

namespace opennlp.nonjava.extensions
{
    internal static class EnumeratorExtensions
    {
        public static bool hasNext(this IEnumerator<Sequence<Event>> sequence)
        {
            throw new NotImplementedException();
        }

        public static Sequence<Event> next(this IEnumerator<Sequence<Event>> sequence)
        {
            throw new NotImplementedException();
        }
    }
}