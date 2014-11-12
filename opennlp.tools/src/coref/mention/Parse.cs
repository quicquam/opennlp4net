using System;
using System.Collections.Generic;

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

namespace opennlp.tools.coref.mention
{
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Interface for syntactic and named-entity information to be used in coreference
    /// annotation.
    /// </summary>
    public interface Parse : IComparable<Parse>
    {
        /// <summary>
        /// Returns the index of the sentence which contains this parse.
        /// </summary>
        /// <returns> The index of the sentence which contains this parse. </returns>
        int SentenceNumber { get; }

        /// <summary>
        /// Returns a list of the all noun phrases
        /// contained by this parse.  The noun phrases in this list should
        /// also implement the <seealso cref="Parse"/> interface.
        /// </summary>
        /// <returns> a list of all the noun phrases contained by this parse. </returns>
        IList<Parse> NounPhrases { get; }

        /// <summary>
        /// Returns a list of all the named entities
        /// contained by this parse.  The named entities in this list should
        /// also implement the <seealso cref="Parse"/> interface.
        /// </summary>
        /// <returns> a list of all the named entities contained by this parse.  </returns>
        IList<Parse> NamedEntities { get; }

        /// <summary>
        /// Returns a list of the children to this object.  The
        /// children should also implement the <seealso cref="Parse"/> interface
        /// . </summary>
        /// <returns> a list of the children to this object.
        ///  </returns>
        IList<Parse> Children { get; }

        /// <summary>
        /// Returns a list of the children to this object which are constituents or tokens.  The
        /// children should also implement the <seealso cref="Parse"/> interface.  This allows
        /// implementations which contain addition nodes for things such as semantic categories to
        /// hide those nodes from the components which only care about syntactic nodes.
        /// </summary>
        /// <returns> a list of the children to this object which are constituents or tokens. </returns>
        IList<Parse> SyntacticChildren { get; }

        /// <summary>
        /// Returns a list of the tokens contained by this object.  The tokens in this list should also
        /// implement the <seealso cref="Parse"/> interface.
        /// </summary>
        /// <returns> the tokens </returns>
        IList<Parse> Tokens { get; }

        /// <summary>
        /// Returns the syntactic type of this node. Typically this is the part-of-speech or
        /// constituent labeling.
        /// </summary>
        /// <returns> the syntactic type. </returns>
        string SyntacticType { get; }

        /// <summary>
        /// Returns the named-entity type of this node.
        /// </summary>
        /// <returns> the named-entity type.  </returns>
        string EntityType { get; }

        /// <summary>
        /// Determines whether this has an ancestor of type NAC.
        /// </summary>
        /// <returns> true is this has an ancestor of type NAC, false otherwise. </returns>
        bool ParentNAC { get; }

        /// <summary>
        /// Returns the parent parse of this parse node.
        /// </summary>
        /// <returns> the parent parse of this parse node. </returns>
        Parse Parent { get; }

        /// <summary>
        /// Specifies whether this parse is a named-entity.
        /// </summary>
        /// <returns> True if this parse is a named-entity; false otherwise. </returns>
        bool NamedEntity { get; }

        /// <summary>
        /// Specifies whether this parse is a noun phrase.
        /// </summary>
        /// <returns> True if this parse is a noun phrase; false otherwise. </returns>
        bool NounPhrase { get; }

        /// <summary>
        /// Specifies whether this parse is a sentence.
        /// </summary>
        /// <returns> True if this parse is a sentence; false otherwise. </returns>
        bool Sentence { get; }

        /// <summary>
        /// Specifies whether this parse is a coordinated noun phrase.
        /// </summary>
        /// <returns> True if this parse is a coordinated noun phrase; false otherwise. </returns>
        bool CoordinatedNounPhrase { get; }

        /// <summary>
        /// Specifies whether this parse is a token.
        /// </summary>
        /// <returns> True if this parse is a token; false otherwise. </returns>
        bool Token { get; }

        string ToString();

        /// <summary>
        /// Returns an entity id associated with this parse and coreferent parses.  This is only used for training on
        /// already annotated coreference annotation.
        /// </summary>
        /// <returns> an entity id associated with this parse and coreferent parses. </returns>
        int EntityId { get; }

        /// <summary>
        /// Returns the character offsets of this parse node.
        /// </summary>
        /// <returns> The span representing the character offsets of this parse node. </returns>
        Span Span { get; }

        /// <summary>
        /// Returns the first token which is not a child of this parse.  If the first token of a sentence is
        /// a child of this parse then null is returned.
        /// </summary>
        /// <returns> the first token which is not a child of this parse or null if no such token exists. </returns>
        Parse PreviousToken { get; }

        /// <summary>
        /// Returns the next token which is not a child of this parse.  If the last token of a sentence is
        /// a child of this parse then null is returned.
        /// </summary>
        /// <returns> the next token which is not a child of this parse or null if no such token exists. </returns>
        Parse NextToken { get; }
    }
}