using System;
using System.Collections.Generic;
using opennlp.model;

namespace opennlp.nonjava.extensions
{
    static class EnumeratorExtensions
    {
// ReSharper disable InconsistentNaming
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
