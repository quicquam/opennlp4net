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
using System.IO;
using j4n.Exceptions;

namespace opennlp.tools.coref.mention
{
    //using JWNLException = net.didion.jwnl.JWNLException;

    /// <summary>
    /// Factory class used to get an instance of a dictionary object. </summary>
    /// <seealso cref= opennlp.tools.coref.mention.Dictionary
    ///  </seealso>
    public class DictionaryFactory
    {
        private static Dictionary dictionary;

        /// <summary>
        /// Returns the default implementation of the Dictionary interface. </summary>
        /// <returns> the default implementation of the Dictionary interface. </returns>
        public static Dictionary Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    try
                    {
                        // was JWNLDictionary (Java Dictionary), created alternative wordnet Dictionary
                        dictionary = new WNLDictionary(Environment.GetEnvironmentVariable("WNSEARCHDIR"));
                    }
                    catch (IOException e)
                    {
                        Console.Error.WriteLine(e);
                    }
                    catch (JWNLException e)
                    {
                        Console.Error.WriteLine(e);
                    }
                }
                return dictionary;
            }
        }
    }
}