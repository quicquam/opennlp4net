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

using System.IO;
using System.Text;
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.Reader;
using opennlp.tools.util;

namespace opennlp.tools.formats.convert
{
    public class FileToStringSampleStream : FilterObjectStream<Jfile, string>
	{

	  private readonly Charset encoding;

      public FileToStringSampleStream(ObjectStream<Jfile> samples, Charset encoding)
          : base(samples)
	  {

		this.encoding = encoding;
	  }

	  private static string readFile(Jfile textFile, Charset encoding)
	  {

		Reader @in = new BufferedReader(new InputStreamReader(new FileInputStream(textFile), encoding));

		StringBuilder text = new StringBuilder();

		try
		{
		  char[] buffer = new char[1024];
		  int length;
		  while ((length = @in.read(buffer, 0, buffer.Length)) > 0)
		  {
			text.Append(buffer, 0, length);
		  }
		}
		finally
		{
		  try
		  {
			@in.close();
		  }
		  catch (IOException)
		  {
			// sorry that this can fail!
		  }
		}

		return text.ToString();
	  }

	  public override string read()
	  {

		Jfile sampleFile = samples.read();

		if (sampleFile != null)
		{
		  return readFile(sampleFile, encoding);
		}
		else
		{
		  return null;
		}
	  }
	}

}