using System;
/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
using j4n.IO.File;
using j4n.IO.InputStream;

namespace opennlp.model
{
    public class BinaryFileDataReader : DataReader
    {
        private DataInputStream input;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public BinaryFileDataReader(java.io.File f) throws java.io.IOException
        public BinaryFileDataReader(Jfile f)
        {
            if (f.Name.EndsWith(".gz", StringComparison.Ordinal))
            {
                input =
                    new DataInputStream(
                        new BufferedInputStream(new GZIPInputStream(new BufferedInputStream(new FileInputStream(f)))));
            }
            else
            {
                input = new DataInputStream(new BufferedInputStream(new FileInputStream(f)));
            }
        }

        public BinaryFileDataReader(InputStream @in)
        {
            input = new DataInputStream(@in);
        }

        public BinaryFileDataReader(DataInputStream @in)
        {
            input = @in;
        }

        public BinaryFileDataReader(ByteArrayInputStream @in)
        {
            throw new NotImplementedException();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double readDouble() throws java.io.IOException
        public virtual double readDouble()
        {
            return input.readDouble();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readInt() throws java.io.IOException
        public virtual int readInt()
        {
            return input.readInt();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readUTF() throws java.io.IOException
        public virtual string readUTF()
        {
            return input.readUTF();
        }
    }
}