using System.Collections;

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace opennlp.tools.util
{
    /// <summary>
    /// Class which creates mapping between keys and a list of values.
    /// </summary>
    public class HashList : Hashtable
    {
        private const long serialVersionUID = 1;

        public HashList()
        {
        }

        public virtual object get(object key, int index)
        {
            if (this[key] != null)
            {
                return ((IList) this[key])[index];
            }
            else
            {
                return null;
            }
        }

        public virtual object putAll(object key, ICollection values)
        {
            IList o = (IList) this[key];

            if (o == null)
            {
                o = new ArrayList();
                base[key] = o;
            }

            foreach (var value in values)
            {
                o.Add(value);
            }

            if (o.Count == values.Count)
            {
                return null;
            }
            else
            {
                return o;
            }
        }

        public IList Put(object key, object value)
        {
            IList o = (IList) this[key];

            if (o == null)
            {
                o = new ArrayList();
                base[key] = o;
            }

            o.Add(value);

            if (o.Count == 1)
            {
                return null;
            }
            else
            {
                return o;
            }
        }

        public virtual bool remove(object key, object value)
        {
            IList l = (IList) this[key];
            if (l == null)
            {
                return false;
            }
            else
            {
                l.Remove(value);
                if (l.Count == 0)
                {
                    this.Remove(key);
                }
                return true;
            }
        }
    }
}