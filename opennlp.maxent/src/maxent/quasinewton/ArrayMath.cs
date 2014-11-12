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

namespace opennlp.maxent.quasinewton
{
    /// <summary>
    /// utility class for simple vector arithmetics.
    /// </summary>
    public class ArrayMath
    {
        public static double innerProduct(double[] vecA, double[] vecB)
        {
            if (vecA == null || vecB == null)
            {
                return double.NaN;
            }
            if (vecA.Length != vecB.Length)
            {
                return double.NaN;
            }

            double product = 0.0;
            for (int i = 0; i < vecA.Length; i++)
            {
                product += vecA[i]*vecB[i];
            }
            return product;
        }

        public static double[] updatePoint(double[] point, double[] vector, double scale)
        {
            if (point == null || vector == null)
            {
                return null;
            }
            if (point.Length != vector.Length)
            {
                return null;
            }

            double[] updated = point.Clone() as double[];
            for (int i = 0; i < updated.Length; i++)
            {
                updated[i] = updated[i] + (vector[i]*scale);
            }
            return updated;
        }
    }
}