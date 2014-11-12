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

namespace opennlp.tools.coref
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;

    /// <summary>
    /// Represents the elements which are part of a discourse.
    /// </summary>
    public class DiscourseModel
    {
        private IList<DiscourseEntity> entities;

        internal int nextEntityId = 1;

        /// <summary>
        /// Creates a new discourse model.
        /// </summary>
        public DiscourseModel()
        {
            entities = new List<DiscourseEntity>();
        }

        /// <summary>
        /// Indicates that the specified entity has been mentioned.
        /// </summary>
        /// <param name="e"> The entity which has been mentioned. </param>
        public virtual void mentionEntity(DiscourseEntity e)
        {
            if (entities.Remove(e))
            {
                entities.Insert(0, e);
            }
            else
            {
                Console.Error.WriteLine("DiscourseModel.mentionEntity: failed to remove " + e);
            }
        }

        /// <summary>
        /// Returns the number of entities in this discourse model.
        /// </summary>
        /// <returns> the number of entities in this discourse model. </returns>
        public virtual int NumEntities
        {
            get { return entities.Count; }
        }

        /// <summary>
        /// Returns the entity at the specified index.
        /// </summary>
        /// <param name="i"> The index of the entity to be returned. </param>
        /// <returns> the entity at the specified index. </returns>
        public virtual DiscourseEntity getEntity(int i)
        {
            return entities[i];
        }

        /// <summary>
        /// Adds the specified entity to this discourse model.
        /// </summary>
        /// <param name="e"> the entity to be added to the model. </param>
        public virtual void addEntity(DiscourseEntity e)
        {
            e.Id = nextEntityId;
            nextEntityId++;
            entities.Insert(0, e);
        }

        /// <summary>
        /// Merges the specified entities into a single entity with the specified confidence.
        /// </summary>
        /// <param name="e1"> The first entity. </param>
        /// <param name="e2"> The second entity. </param>
        /// <param name="confidence"> The confidence. </param>
        public virtual void mergeEntities(DiscourseEntity e1, DiscourseEntity e2, float confidence)
        {
            for (IEnumerator<MentionContext> ei = e2.Mentions; ei.MoveNext();)
            {
                e1.addMention(ei.Current);
            }
            //System.err.println("DiscourseModel.mergeEntities: removing "+e2);
            entities.Remove(e2);
        }

        /// <summary>
        /// Returns the entities in the discourse model.
        /// </summary>
        /// <returns> the entities in the discourse model. </returns>
        public virtual DiscourseEntity[] Entities
        {
            get
            {
                DiscourseEntity[] des = new DiscourseEntity[entities.Count];
                return des;
            }
        }

        /// <summary>
        /// Removes all elements from this discourse model.
        /// </summary>
        public virtual void clear()
        {
            entities.Clear();
        }
    }
}