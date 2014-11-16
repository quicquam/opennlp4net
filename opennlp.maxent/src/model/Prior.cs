﻿/*
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
    /// This interface allows one to implement a prior distribution for use in
    /// maximum entropy model training.  
    /// </summary>
    public interface Prior
    {
        /// <summary>
        /// Populates the specified array with the the log of the distribution for the specified context.  
        /// The returned array will be overwritten and needs to be re-initialized with every call to this method. </summary>
        /// <param name="dist"> An array to be populated with the log of the prior distribution. </param>
        /// <param name="context"> The indices of the contextual predicates for an event. </param>
        void logPrior(double[] dist, int[] context);

        /// <summary>
        /// Populates the specified array with the the log of the distribution for the specified context.  
        /// The returned array will be overwritten and needs to be re-initialized with every call to this method. </summary>
        /// <param name="dist"> An array to be populated with the log of the prior distribution. </param>
        /// <param name="context"> The indices of the contextual predicates for an event. </param>
        /// <param name="values"> The values associated with the context.  </param>
        void logPrior(double[] dist, int[] context, float[] values);

        /// <summary>
        /// Method to specify the label for the outcomes and contexts.  This is used to map 
        /// integer outcomes and contexts to their string values.  This method is called prior
        /// to any call to #logPrior. </summary>
        /// <param name="outcomeLabels"> An array of each outcome label. </param>
        /// <param name="contextLabels"> An array of each context label. </param>
        void setLabels(string[] outcomeLabels, string[] contextLabels);
    }
}