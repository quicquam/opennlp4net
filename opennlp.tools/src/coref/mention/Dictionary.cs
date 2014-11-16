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
    /// <summary>
    /// Interface to provide dictionary information to the coreference module assuming a
    /// hierarchically structured dictionary (such as WordNet) is available.
    /// </summary>
    public interface Dictionary
    {
        /// <summary>
        /// Returns the lemmas of the specified word with the specified part-of-speech.
        /// </summary>
        /// <param name="word"> The word whose lemmas are desired. </param>
        /// <param name="pos"> The part-of-speech of the specified word. </param>
        /// <returns> The lemmas of the specified word given the specified part-of-speech. </returns>
        string[] getLemmas(string word, string pos);

        /// <summary>
        /// Returns a key indicating the specified sense number of the specified
        /// lemma with the specified part-of-speech.
        /// </summary>
        /// <param name="lemma"> The lemmas for which the key is desired. </param>
        /// <param name="pos"> The pos for which the key is desired. </param>
        /// <param name="senseNumber"> The sense number for which the key is desired. </param>
        /// <returns> a key indicating the specified sense number of the specified
        /// lemma with the specified part-of-speech. </returns>
        string getSenseKey(string lemma, string pos, int senseNumber);

        /// <summary>
        /// Returns the number of senses in the dictionary for the specified lemma.
        /// </summary>
        /// <param name="lemma"> A lemmatized form of the word to look up. </param>
        /// <param name="pos"> The part-of-speech for the lemma. </param>
        /// <returns> the number of senses in the dictionary for the specified lemma. </returns>
        int getNumSenses(string lemma, string pos);

        /// <summary>
        /// Returns an array of keys for each parent of the specified sense number of the specified lemma with the specified part-of-speech.
        /// </summary>
        /// <param name="lemma"> A lemmatized form of the word to look up. </param>
        /// <param name="pos"> The part-of-speech for the lemma. </param>
        /// <param name="senseNumber"> The sense number for which the parent keys are desired. </param>
        /// <returns> an array of keys for each parent of the specified sense number of the specified lemma with the specified part-of-speech. </returns>
        string[] getParentSenseKeys(string lemma, string pos, int senseNumber);
    }
}