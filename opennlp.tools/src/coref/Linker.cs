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

using System.IO;

namespace opennlp.tools.coref
{
    using HeadFinder = opennlp.tools.coref.mention.HeadFinder;
    using Mention = opennlp.tools.coref.mention.Mention;
    using MentionContext = opennlp.tools.coref.mention.MentionContext;
    using MentionFinder = opennlp.tools.coref.mention.MentionFinder;

    /// <summary>
    /// A linker provides an interface for finding mentions, <seealso cref="#getMentionFinder getMentionFinder"/>,
    /// and creating entities out of those mentions, <seealso cref="#getEntities getEntities"/>.  This interface also allows
    /// for the training of a resolver with the method <seealso cref="#setEntities setEntitites"/> which is used to give the
    /// resolver mentions whose entityId fields indicate which mentions refer to the same entity and the
    /// <seealso cref="#train train"/> method which compiles all the information provided via calls to
    /// <seealso cref="#setEntities setEntities"/> into a model.
    /// </summary>
    public interface Linker
    {
        /// <summary>
        /// String constant used to label a mention which is a description.
        /// </summary>
        /// <summary>
        /// String constant used to label an mention in an appositive relationship.
        /// </summary>
        /// <summary>
        /// String constant used to label a mention which consists of two or more noun phrases.
        /// </summary>
        /// <summary>
        /// String constant used to label a mention which consists of a single noun phrase.
        /// </summary>
        /// <summary>
        /// String constant used to label a mention which is a proper noun modifying another noun.
        /// </summary>
        /// <summary>
        /// String constant used to label a mention which is a pronoun.
        /// </summary>
        /// <summary>
        /// Indicated that the specified mentions can be used to train this linker.
        /// This requires that the coreference relationship between the mentions have been labeled
        /// in the mention's id field.
        /// </summary>
        /// <param name="mentions"> The mentions to be used to train the linker. </param>
        Mention[] Entities { set; }

        /// <summary>
        /// Returns a list of entities which group the mentions into entity classes. </summary>
        /// <param name="mentions"> A array of mentions.
        /// </param>
        /// <returns> An array of discourse entities. </returns>
        DiscourseEntity[] getEntities(Mention[] mentions);

        /// <summary>
        /// Creates mention contexts for the specified mention exents.  These are used to compute coreference features over. </summary>
        /// <param name="mentions"> The mention of a document.
        /// </param>
        /// <returns> mention contexts for the specified mention exents. </returns>
        MentionContext[] constructMentionContexts(Mention[] mentions);

        /// <summary>
        /// Trains the linker based on the data specified via calls to <seealso cref="#setEntities setEntities"/>.
        /// </summary>
        /// <exception cref="IOException"> </exception>
        void train();

        /// <summary>
        /// Returns the mention finder for this linker.  This can be used to get the mentions of a Parse.
        /// </summary>
        /// <returns> The object which finds mentions for this linker. </returns>
        MentionFinder MentionFinder { get; }

        /// <summary>
        /// Returns the head finder associated with this linker.
        /// </summary>
        /// <returns> The head finder associated with this linker. </returns>
        HeadFinder HeadFinder { get; }
    }

    public static class Linker_Fields
    {
        public const string DESCRIPTOR = "desc";
        public const string ISA = "isa";
        public const string COMBINED_NPS = "cmbnd";
        public const string NP = "np";
        public const string PROPER_NOUN_MODIFIER = "pnmod";
        public const string PRONOUN_MODIFIER = "np";
    }
}