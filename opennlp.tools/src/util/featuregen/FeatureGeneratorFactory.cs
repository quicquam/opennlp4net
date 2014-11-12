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

namespace opennlp.tools.util.featuregen
{
    /// <summary>
    /// The <seealso cref="FeatureGeneratorFactory"/> interface is factory for <seealso cref="AdaptiveFeatureGenerator"/>s.
    /// <para>
    /// <b>Note:</b><br>
    /// All implementing classes must be thread safe.
    /// 
    /// </para>
    /// </summary>
    /// <seealso cref= AdaptiveFeatureGenerator </seealso>
    /// <seealso cref= FeatureGeneratorResourceProvider
    /// 
    /// </seealso>
    /// @deprecated do not use this interface, will be removed! 
    [Obsolete("do not use this interface, will be removed!")]
    public interface FeatureGeneratorFactory
        /// <summary>
        /// Constructs a new <seealso cref="AdaptiveFeatureGenerator"/>.
        /// <para>
        /// <b>Note:</b><br>
        /// It is assumed that all resource objects are thread safe and can be shared
        /// between multiple instances of feature generators. If that is not the
        /// case the implementor should make a copy of the resource object.
        /// All resource objects that are included in OpenNLP can be assumed to be thread safe.
        /// 
        /// </para>
        /// </summary>
        /// <param name="resourceProvider"> provides access to resources which are needed for feature generation.
        /// </param>
        /// <returns> the newly created feature generator </returns>
    {
        [Obsolete]
        AdaptiveFeatureGenerator createFeatureGenerator(FeatureGeneratorResourceProvider resourceProvider);
    }
}