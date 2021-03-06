﻿/*
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


namespace opennlp.tools.parser
{
    /// <summary>
    ///  Interface for full-syntactic parsers.
    /// </summary>
    public interface Parser
    {
        /// <summary>
        /// Returns the specified number of parses or fewer for the specified tokens. <br>
        /// <b>Note:</b> The nodes within
        /// the returned parses are shared with other parses and therefore their parent node references will not be consistent
        /// with their child node reference.  <seealso cref="Parse#setParent(Parse)"/> can be used to make the parents consistent
        /// with a particular parse, but subsequent calls to <code>setParents</code> can invalidate the results of earlier
        /// calls.<br> </summary>
        /// <param name="tokens"> A parse containing the tokens with a single parent node. </param>
        /// <param name="numParses"> The number of parses desired. </param>
        /// <returns> the specified number of parses for the specified tokens. </returns>
        Parse[] parse(Parse tokens, int numParses);

        /// <summary>
        /// Returns a parse for the specified parse of tokens. </summary>
        /// <param name="tokens"> The root node of a flat parse containing only tokens. </param>
        /// <returns> A full parse of the specified tokens or the flat chunks of the tokens if a fullparse could not be found. </returns>
        Parse parse(Parse tokens);
    }
}