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

namespace opennlp.tools.coref.sim
{
    /// <summary>
    /// Enumeration of number types.
    /// </summary>
    public class NumberEnum
    {
        private readonly string name;

        /// <summary>
        /// Singular number type. 
        /// </summary>
        public static readonly NumberEnum SINGULAR = new NumberEnum("singular");

        /// <summary>
        /// Plural number type. 
        /// </summary>
        public static readonly NumberEnum PLURAL = new NumberEnum("plural");

        /// <summary>
        /// Unknown number type. 
        /// </summary>
        public static readonly NumberEnum UNKNOWN = new NumberEnum("unknown");

        private NumberEnum(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}