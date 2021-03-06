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
using j4n.IO.File;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.Serialization;

namespace opennlp.tools.formats.convert
{


	using opennlp.tools.util;
	using opennlp.tools.util;

	public class FileToByteArraySampleStream : FilterObjectStream<Jfile, byte[]>
	{

	  public FileToByteArraySampleStream(ObjectStream<Jfile> samples) : base(samples)
	  {
	  }

	  private static sbyte[] readFile(Jfile file)
	  {

		InputStream @in = new BufferedInputStream(new FileInputStream(file));

	      ByteArrayOutputStream bytes = new ByteArrayOutputStream(new FileStream(file.Name, FileMode.Open));

		try
		{
		  sbyte[] buffer = new sbyte[1024];
		  int length;
		  while ((length = @in.read(buffer, 0, buffer.Length)) > 0)
		  {
			bytes.write(buffer, 0, length);
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

        return bytes.toSbyteArray();
	  }

	  public override byte[] read()
	  {

		Jfile sampleFile = samples.read();

		if (sampleFile != null)
		{
		    return null;  // MJJ 14/11/2014 need to cast sbyte[] to byte[] readFile(sampleFile);
		}
		else
		{
		  return null;
		}
	  }
	}

}