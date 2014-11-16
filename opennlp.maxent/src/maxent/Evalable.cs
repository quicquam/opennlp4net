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

using j4n.IO.Reader;

namespace opennlp.maxent
{
    using EventCollector = opennlp.model.EventCollector;
    using MaxentModel = opennlp.model.MaxentModel;

    /// <summary>
    /// Interface for components which use maximum entropy models and can evaluate
    /// the performace of the models using the TrainEval class.
    /// </summary>
    public interface Evalable
    {
        /// <summary>
        /// The outcome that should be considered a negative result. This is used for
        /// computing recall. In the case of binary decisions, this would be the false
        /// one.
        /// </summary>
        /// <returns> the events that this EventCollector has gathered </returns>
        string NegativeOutcome { get; }

        /// <summary>
        /// Returns the EventCollector that is used to collect all relevant information
        /// from the data file. This is used for to test the predictions of the model.
        /// Note that if some of your features are the oucomes of previous events, this
        /// method will give you results assuming 100% performance on the previous
        /// events. If you don't like this, use the localEval method.
        /// </summary>
        /// <param name="r">
        ///          A reader containing the data for the event collector </param>
        /// <returns> an EventCollector </returns>
        EventCollector getEventCollector(Reader r);

        /// <summary>
        /// If the -l option is selected for evaluation, this method will be called
        /// rather than TrainEval's evaluation method. This is good if your features
        /// includes the outcomes of previous events.
        /// </summary>
        /// <param name="model">
        ///          the maxent model to evaluate </param>
        /// <param name="r">
        ///          Reader containing the data to process </param>
        /// <param name="e">
        ///          The original Evalable. Probably not relevant. </param>
        /// <param name="verbose">
        ///          a request to print more specific processing information </param>
        void localEval(MaxentModel model, Reader r, Evalable e, bool verbose);
    }
}