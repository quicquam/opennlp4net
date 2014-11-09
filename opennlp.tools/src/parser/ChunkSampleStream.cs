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

namespace opennlp.tools.parser
{


	using ChunkSample = opennlp.tools.chunker.ChunkSample;
	using Parser = opennlp.tools.parser.chunking.Parser;
	using opennlp.tools.util;
	using opennlp.tools.util;

	public class ChunkSampleStream : FilterObjectStream<Parse, ChunkSample>
	{

	  public ChunkSampleStream(ObjectStream<Parse> @in) : base(@in)
	  {
	  }

	  private static void getInitialChunks(Parse p, IList<Parse> ichunks)
	  {
		if (p.PosTag)
		{
		  ichunks.Add(p);
		}
		else
		{
		  Parse[] kids = p.Children;
		  bool allKidsAreTags = true;
		  for (int ci = 0, cl = kids.Length; ci < cl; ci++)
		  {
			if (!kids[ci].PosTag)
			{
			  allKidsAreTags = false;
			  break;
			}
		  }
		  if (allKidsAreTags)
		  {
			ichunks.Add(p);
		  }
		  else
		  {
			for (int ci = 0, cl = kids.Length; ci < cl; ci++)
			{
			  getInitialChunks(kids[ci], ichunks);
			}
		  }
		}
	  }

	  public static Parse[] getInitialChunks(Parse p)
	  {
		IList<Parse> chunks = new List<Parse>();
		getInitialChunks(p, chunks);
		return chunks.ToArray();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.chunker.ChunkSample read() throws java.io.IOException
	  public override ChunkSample read()
	  {

		Parse parse = samples.read();

		if (parse != null)
		{
		  Parse[] chunks = getInitialChunks(parse);
		  IList<string> toks = new List<string>();
		  IList<string> tags = new List<string>();
		  IList<string> preds = new List<string>();
		  for (int ci = 0, cl = chunks.Length; ci < cl; ci++)
		  {
			Parse c = chunks[ci];
			if (c.PosTag)
			{
			  toks.Add(c.CoveredText);
			  tags.Add(c.Type);
			  preds.Add(Parser.OTHER);
			}
			else
			{
			  bool start = true;
			  string ctype = c.Type;
			  Parse[] kids = c.Children;
			  for (int ti = 0,tl = kids.Length;ti < tl;ti++)
			  {
				Parse tok = kids[ti];
				toks.Add(tok.CoveredText);
				tags.Add(tok.Type);
				if (start)
				{
				  preds.Add(Parser.START + ctype);
				  start = false;
				}
				else
				{
				  preds.Add(Parser.CONT + ctype);
				}
			  }
			}
		  }

		  return new ChunkSample(toks.ToArray(), tags.ToArray(), preds.ToArray());
		}
		else
		{
		  return null;
		}
	  }
	}

}