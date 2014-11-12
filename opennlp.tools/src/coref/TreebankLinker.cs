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
    using PTBMentionFinder = opennlp.tools.coref.mention.PTBMentionFinder;

    /// <summary>
    /// This class perform coreference for treebank style parses.
    /// <para>
    /// It will only perform coreference over constituents defined in the trees and
    /// will not generate new constituents for pre-nominal entities or sub-entities in
    /// simple coordinated noun phrases.
    /// </para>
    /// <para>
    /// This linker requires that named-entity information also be provided.
    /// </para>
    /// </summary>
    public class TreebankLinker : DefaultLinker
    {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TreebankLinker(String project, LinkerMode mode) throws java.io.IOException
        public TreebankLinker(string project, LinkerMode mode) : base(project, mode)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TreebankLinker(String project, LinkerMode mode, boolean useDiscourseModel) throws java.io.IOException
        public TreebankLinker(string project, LinkerMode mode, bool useDiscourseModel)
            : base(project, mode, useDiscourseModel)
        {
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TreebankLinker(String project, LinkerMode mode, boolean useDiscourseModel, double fixedNonReferentialProbability) throws java.io.IOException
        public TreebankLinker(string project, LinkerMode mode, bool useDiscourseModel,
            double fixedNonReferentialProbability)
            : base(project, mode, useDiscourseModel, fixedNonReferentialProbability)
        {
        }

        protected internal override void initMentionFinder()
        {
            mentionFinder = PTBMentionFinder.getInstance(headFinder);
        }
    }
}