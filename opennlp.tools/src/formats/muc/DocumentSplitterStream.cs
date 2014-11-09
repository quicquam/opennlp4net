using System;
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

namespace opennlp.tools.formats.muc
{


	using opennlp.tools.util;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using opennlp.tools.util;

	internal class DocumentSplitterStream : FilterObjectStream<string, string>
	{

	  private const string DOC_START_ELEMENT = "<DOC>";
	  private const string DOC_END_ELEMENT = "</DOC>";

	  private IList<string> docs = new List<string>();

	  internal DocumentSplitterStream(ObjectStream<string> samples) : base(samples)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String read() throws java.io.IOException
	  public override string read()
	  {

		if (docs.Count == 0)
		{
		  string newDocs = samples.read();

		  if (newDocs != null)
		  {
			int docStartOffset = 0;

			while (true)
			{
			  int startDocElement = newDocs.IndexOf(DOC_START_ELEMENT, docStartOffset, StringComparison.Ordinal);
			  int endDocElement = newDocs.IndexOf(DOC_END_ELEMENT, docStartOffset, StringComparison.Ordinal);

			  if (startDocElement != -1 && endDocElement != -1)
			  {

				if (startDocElement < endDocElement)
				{
				  docs.Add(newDocs.Substring(startDocElement, endDocElement + DOC_END_ELEMENT.Length - startDocElement));
				  docStartOffset = endDocElement + DOC_END_ELEMENT.Length;
				}
				else
				{
				  throw new InvalidFormatException("<DOC> element is not closed!");
				}
			  }
			  else if (startDocElement != endDocElement)
			  {
				throw new InvalidFormatException("Missing <DOC> or </DOC> element!");
			  }
			  else
			  {
				break;
			  }
			}
		  }
		}

		if (docs.Count > 0)
		{
		  return docs.Remove(0);
		}
		else
		{
		  return null;
		}
	  }
	}

}