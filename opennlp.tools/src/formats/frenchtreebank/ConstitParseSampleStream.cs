using System.Collections.Generic;
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
using j4n.IO.InputStream;
using j4n.Serialization;
using saxlib.Entities;
using saxlib.Exceptions;
using saxlib.Factories;

namespace opennlp.tools.formats.frenchtreebank
{



	using Parse = opennlp.tools.parser.Parse;
	using opennlp.tools.util;
	using opennlp.tools.util;

	public class ConstitParseSampleStream : FilterObjectStream<byte[], Parse>
	{

	  private SAXParser saxParser;

	  private IList<Parse> parses = new List<Parse>();

	  protected internal ConstitParseSampleStream(ObjectStream<sbyte[]> samples) : base(samples)
	  {

		SAXParserFactory factory = SAXParserFactory.newInstance();
		try
		{
		  saxParser = factory.newSAXParser();
		}
		catch (ParserConfigurationException e)
		{
		  throw new IllegalStateException(e);
		}
		catch (SAXException e)
		{
		  throw new IllegalStateException(e);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.parser.Parse read() throws java.io.IOException
	  public override Parse read()
	  {


		if (parses.Count == 0)
		{
		  sbyte[] xmlbytes = samples.read();

		  if (xmlbytes != null)
		  {

			IList<Parse> producedParses = new List<Parse>();
			try
			{
			  saxParser.parse(new ByteArrayInputStream(xmlbytes), new ConstitDocumentHandler(producedParses));
			}
			catch (SAXException e)
			{
			  //TODO update after Java6 upgrade
			  throw (IOException) (new IOException(e.Message)).initCause(e);
			}

			parses.AddRange(producedParses);
		  }
		}

		if (parses.Count > 0)
		{
		  return parses.Remove(0);
		}
		else
		{
		  return null;
		}
	  }
	}

}