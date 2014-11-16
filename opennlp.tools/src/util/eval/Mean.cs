using System;

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

namespace opennlp.tools.util.eval
{
    /// <summary>
    /// Calculates the arithmetic mean of values
    /// added with the <seealso cref="#add(double)"/> method.
    /// </summary>
    public class Mean
    {
        /// <summary>
        /// The sum of all added values.
        /// </summary>
        private double sum;

        /// <summary>
        /// The number of times a value was added.
        /// </summary>
        private long count_Renamed;

        /// <summary>
        /// Adds a value to the arithmetic mean.
        /// </summary>
        /// <param name="value"> the value which should be added
        /// to the arithmetic mean. </param>
        public virtual void add(double value)
        {
            add(value, 1);
        }

        /// <summary>
        /// Adds a value count times to the arithmetic mean.
        /// </summary>
        /// <param name="value"> the value which should be added
        /// to the arithmetic mean.
        /// </param>
        /// <param name="count"> number of times the value should be added to
        /// arithmetic mean. </param>
        public virtual void add(double value, long count)
        {
            sum += value*count;
            this.count_Renamed += count;
        }

        /// <summary>
        /// Retrieves the mean of all values added with
        /// <seealso cref="#add(double)"/> or 0 if there are zero added
        /// values.
        /// </summary>
        public virtual double mean()
        {
            return count_Renamed > 0 ? sum/count_Renamed : 0;
        }

        /// <summary>
        /// Retrieves the number of times a value
        /// was added to the mean.
        /// </summary>
        public virtual long count()
        {
            return count_Renamed;
        }

        public override string ToString()
        {
            return Convert.ToString(mean());
        }
    }
}