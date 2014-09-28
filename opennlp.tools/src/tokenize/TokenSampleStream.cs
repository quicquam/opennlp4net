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

using j4n.Serialization;

namespace opennlp.tools.tokenize
{

	using opennlp.tools.util;

	/// <summary>
	/// This class is a stream filter which reads in string encoded samples and creates
	/// <seealso cref="TokenSample"/>s out of them. The input string sample is tokenized if a
	/// whitespace or the special separator chars occur.
	/// <para>
	/// Sample:<br>
	/// "token1 token2 token3<SPLIT>token4"<br>
	/// The tokens token1 and token2 are separated by a whitespace, token3 and token3
	/// are separated by the special character sequence, in this case the default
	/// split sequence.
	/// </para>
	/// <para>
	/// The sequence must be unique in the input string and is not escaped.
	/// </para>
	/// </summary>
	public class TokenSampleStream : FilterObjectStream<string, TokenSample>
	{

	  private readonly string separatorChars;


	  public TokenSampleStream(ObjectStream<string> sampleStrings, string separatorChars) : base(sampleStrings)
	  {


		if (sampleStrings == null)
		{
		  throw new System.ArgumentException("sampleStrings must not be null!");
		}
		if (separatorChars == null)
		{
		  throw new System.ArgumentException("separatorChars must not be null!");
		}

		this.separatorChars = separatorChars;
	  }

	  public TokenSampleStream(ObjectStream<string> sentences) : this(sentences, TokenSample.DEFAULT_SEPARATOR_CHARS)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TokenSample read() throws java.io.IOException
	  public override TokenSample read()
	  {
		string sampleString = samples.read();

		if (sampleString != null)
		{
		  return TokenSample.parse(sampleString, separatorChars);
		}
		else
		{
		  return null;
		}
	  }
	}

}