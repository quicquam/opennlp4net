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


using j4n.IO.InputStream;

namespace opennlp.tools.util.model
{
    /// <summary>
    /// An <seealso cref="InputStream"/> which cannot be closed.
    /// </summary>
    public class UncloseableInputStream : FilterInputStream
    {
        public UncloseableInputStream(InputStream @in) : base(@in)
        {
        }

        /// <summary>
        /// This method does not has any effect the <seealso cref="InputStream"/>
        /// cannot be closed.
        /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
        public override void close()
        {
        }
    }
}