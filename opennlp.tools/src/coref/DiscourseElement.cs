using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace opennlp.tools.coref
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;
    using opennlp.tools.util;

    /// <summary>
    /// Represents an item in which can be put into the discourse model.  Object which are
    /// to be placed in the discourse model should extend this class.
    /// </summary>
    /// <seealso cref= opennlp.tools.coref.DiscourseModel </seealso>
    public abstract class DiscourseElement
    {
        private IList<MentionContext> extents;
        private int id = -1;
        private MentionContext lastExtent;

        /// <summary>
        /// Creates a new discourse element which contains the specified mention.
        /// </summary>
        /// <param name="mention"> The mention which begins this discourse element. </param>
        public DiscourseElement(MentionContext mention)
        {
            extents = new List<MentionContext>(1);
            lastExtent = mention;
            extents.Add(mention);
        }

        /// <summary>
        /// Returns an iterator over the mentions which iterates through them based on which were most recently mentioned. </summary>
        /// <returns> the <seealso cref="Iterator"/>. </returns>
        public virtual IEnumerator<MentionContext> RecentMentions
        {
            get
            {
                extents.ToList().Reverse();
                return extents.GetEnumerator();
            }
        }

        /// <summary>
        /// Returns an iterator over the mentions which iterates through them based on
        /// their occurrence in the document.
        /// </summary>
        /// <returns> the <seealso cref="Iterator"/> </returns>
        public virtual IEnumerator<MentionContext> Mentions
        {
            get
            {
                //JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
                return (extents.GetEnumerator());
            }
        }

        /// <summary>
        /// Returns the number of mentions in this element.
        /// </summary>
        /// <returns> number of mentions </returns>
        public virtual int NumMentions
        {
            get { return (extents.Count); }
        }

        /// <summary>
        /// Adds the specified mention to this discourse element. </summary>
        /// <param name="mention"> The mention to be added. </param>
        public virtual void addMention(MentionContext mention)
        {
            extents.Add(mention);
            lastExtent = mention;
        }

        /// <summary>
        /// Returns the last mention for this element.  For appositives this will be the
        /// first part of the appositive. </summary>
        /// <returns> the last mention for this element. </returns>
        public virtual MentionContext LastExtent
        {
            get { return (lastExtent); }
        }

        /// <summary>
        /// Associates an id with this element. </summary>
        /// <param name="id"> The id. </param>
        public virtual int Id
        {
            set { this.id = value; }
            get { return (id); }
        }


        public override string ToString()
        {
            IEnumerator<MentionContext> ei = extents.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            MentionContext ex = ei.Current;
            StringBuilder de = new StringBuilder();
            de.Append("[ ").Append(ex.toText()); //.append("<").append(ex.getHeadText()).append(">");
            while (ei.MoveNext())
            {
                ex = ei.Current;
                de.Append(", ").Append(ex.toText()); //.append("<").append(ex.getHeadText()).append(">");
            }
            de.Append(" ]");
            return (de.ToString());
        }
    }
}