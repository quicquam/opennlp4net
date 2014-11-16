using System;
using System.Text;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace opennlp.model
{
    /// <summary>
    /// A maxent predicate representation which we can use to sort based on the
    /// outcomes. This allows us to make the mapping of features to their parameters
    /// much more compact.
    /// </summary>
    public class ComparablePredicate : IComparable<ComparablePredicate>
    {
        public string name;
        public int[] outcomes;
        public double[] @params;

        public ComparablePredicate(string n, int[] ocs, double[] ps)
        {
            name = n;
            outcomes = ocs;
            @params = ps;
        }

        public virtual int CompareTo(ComparablePredicate cp)
        {
            int smallerLength = (outcomes.Length > cp.outcomes.Length ? cp.outcomes.Length : outcomes.Length);

            for (int i = 0; i < smallerLength; i++)
            {
                if (outcomes[i] < cp.outcomes[i])
                {
                    return -1;
                }
                else if (outcomes[i] > cp.outcomes[i])
                {
                    return 1;
                }
            }

            if (outcomes.Length < cp.outcomes.Length)
            {
                return -1;
            }
            else if (outcomes.Length > cp.outcomes.Length)
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            foreach (int outcome in outcomes)
            {
                s.Append(" ").Append(outcome);
            }
            return s.ToString();
        }
    }
}