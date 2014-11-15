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

namespace opennlp.tools.chunker
{
    using BaseToolFactory = opennlp.tools.util.BaseToolFactory;
    using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
    using opennlp.tools.util;
    using ExtensionLoader = opennlp.tools.util.ext.ExtensionLoader<ChunkerFactory>;

    public class ChunkerFactory : BaseToolFactory
    {
        /// <summary>
        /// Creates a <seealso cref="ChunkerFactory"/> that provides the default implementation
        /// of the resources.
        /// </summary>
        public ChunkerFactory()
        {
        }

        public static ChunkerFactory create(string subclassName)
        {
            if (subclassName == null)
            {
                // will create the default factory
                return new ChunkerFactory();
            }
            try
            {
                ChunkerFactory theFactory = ExtensionLoader.instantiateExtension(subclassName);
                return theFactory;
            }
            catch (Exception e)
            {
                string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
                Console.Error.WriteLine(msg);
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                throw new InvalidFormatException(msg, e);
            }
        }

        public override void validateArtifactMap()
        {
            // no additional artifacts
        }

        public virtual SequenceValidator<string> SequenceValidator
        {
            get { return new DefaultChunkerSequenceValidator(); }
        }

        public virtual ChunkerContextGenerator ContextGenerator
        {
            get { return new DefaultChunkerContextGenerator(); }
        }
    }
}