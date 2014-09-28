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
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.IO.Writer;


namespace opennlp.maxent.io
{


	/// <summary>
	/// A program to convert from java binary doubles to ascii.  With the new
	/// conversion utililities provided in Maxent 1.2 this probably won't be
	/// necessary, but it doesn't do any harm to keep it around for now.
	/// </summary>
	public class BinToAscii
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
	  public static void Main(string[] args)
	  {
		PrintWriter @out = new PrintWriter(new OutputStreamWriter(new GZIPOutputStream(new FileOutputStream(args[1]))));
		DataInputStream @in = new DataInputStream(new GZIPInputStream(new FileInputStream(args[0])));

		double d;
		try
		{
		  while (true)
		  {
			@out.println(@in.readDouble());
		  }
		}
		catch (Exception)
		{
		}
		@out.close();
		@in.close();
	  }

	}

}