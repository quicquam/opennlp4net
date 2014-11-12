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

namespace opennlp.tools.coref.mention
{
    using Span = opennlp.tools.util.Span;

    /// <summary>
    /// Data structure representation of a mention.
    /// </summary>
    public class Mention : IComparable<Mention>
    {
        /// <summary>
        /// Represents the character offset for this extent. 
        /// </summary>
        private Span span;

        /// <summary>
        /// A string representing the type of this extent. This is helpful for determining
        /// which piece of code created a particular extent.
        /// </summary>
        protected internal string type;

        /// <summary>
        /// The entity id indicating which entity this extent belongs to.  This is only
        /// used when training a coreference classifier.
        /// </summary>
        private int id;

        /// <summary>
        /// Represents the character offsets of the the head of this extent.
        /// </summary>
        private Span headSpan;

        /// <summary>
        /// The parse node that this extent is based on.
        /// </summary>
        protected internal Parse parse;

        /// <summary>
        /// A string representing the name type for this extent. 
        /// </summary>
        protected internal string nameType;

        public Mention(Span span, Span headSpan, int entityId, Parse parse, string extentType)
        {
            this.span = span;
            this.headSpan = headSpan;
            this.id = entityId;
            this.type = extentType;
            this.parse = parse;
        }

        public Mention(Span span, Span headSpan, int entityId, Parse parse, string extentType, string nameType)
        {
            this.span = span;
            this.headSpan = headSpan;
            this.id = entityId;
            this.type = extentType;
            this.parse = parse;
            this.nameType = nameType;
        }

        public Mention(Mention mention)
            : this(mention.span, mention.headSpan, mention.id, mention.parse, mention.type, mention.nameType)
        {
        }

        /// <summary>
        /// Returns the character offsets for this extent.
        /// </summary>
        /// <returns> The span representing the character offsets of this extent. </returns>
        public virtual Span Span
        {
            get { return span; }
        }

        /// <summary>
        /// Returns the character offsets for the head of this extent.
        /// </summary>
        /// <returns> The span representing the character offsets for the head of this extent. </returns>
        public virtual Span HeadSpan
        {
            get { return headSpan; }
        }

        /// <summary>
        /// Returns the parse node that this extent is based on.
        /// </summary>
        /// <returns> The parse node that this extent is based on or null if the extent is newly created. </returns>
        public virtual Parse Parse
        {
            get { return parse; }
            set { this.parse = value; }
        }

        public virtual int CompareTo(Mention e)
        {
            return span.CompareTo(e.span);
        }


        /// <summary>
        /// Returns the named-entity category associated with this mention.
        /// </summary>
        /// <returns> the named-entity category associated with this mention. </returns>
        public virtual string NameType
        {
            get { return nameType; }
            set { this.nameType = value; }
        }


        /// <summary>
        /// Associates an id with this mention.
        /// </summary>
        /// <param name="i"> The id for this mention. </param>
        public virtual int Id
        {
            set { id = value; }
            get { return id; }
        }


        public override string ToString()
        {
            return "mention(span=" + span + ",hs=" + headSpan + ", type=" + type + ", id=" + id + " " + parse + " )";
        }
    }
}