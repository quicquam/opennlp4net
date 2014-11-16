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

namespace opennlp.tools.coref.resolver
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;

    /// <summary>
    /// Interface for coreference resolvers. 
    /// </summary>
    public interface Resolver
    {
        /// <summary>
        /// Returns true if this resolver is able to resolve the referring expression of the same type
        /// as the specified mention.
        /// </summary>
        /// <param name="mention"> The mention being considered for resolution.
        /// </param>
        /// <returns> true if the resolver handles this type of referring
        /// expression, false otherwise. </returns>
        bool canResolve(MentionContext mention);

        /// <summary>
        /// Resolve this referring expression to a discourse entity in the discourse model.
        /// </summary>
        /// <param name="ec"> the referring expression. </param>
        /// <param name="dm"> the discourse model.
        /// </param>
        /// <returns> the discourse entity which the resolver believes this
        /// referring expression refers to or null if no discourse entity is
        /// coreferent with the referring expression. </returns>
        DiscourseEntity resolve(MentionContext ec, DiscourseModel dm);

        /// <summary>
        /// Uses the specified mention and discourse model to train this resolver.
        /// All mentions sent to this method need to have their id fields set to indicate coreference
        /// relationships.
        /// </summary>
        /// <param name="mention"> The mention which is being used for training. </param>
        /// <param name="model"> the discourse model.
        /// </param>
        /// <returns> the discourse entity which is referred to by the referring
        /// expression or null if no discourse entity is referenced. </returns>
        DiscourseEntity retain(MentionContext mention, DiscourseModel model);

        /// <summary>
        /// Retrains model on examples for which retain was called.
        /// </summary>
        /// <exception cref="IOException"> </exception>
        void train();
    }
}