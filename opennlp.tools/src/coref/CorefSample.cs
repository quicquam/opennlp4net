using System.Collections.Generic;
using System.Text;
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
using System.Text.RegularExpressions;
using opennlp.tools.parser;

namespace opennlp.tools.coref
{

    // was opennlp.tools.coref.mention.DefaultParse
	using DefaultParse = opennlp.tools.coref.mention.StubDefaultParse;
	using Parse = opennlp.tools.parser.Parse;

	public class CorefSample
	{

	  private IList<Parse> parses;

	  public CorefSample(IList<Parse> parses)
	  {
		this.parses = parses;
	  }

	  public virtual IList<opennlp.tools.coref.mention.Parse> Parses
	  {
		  get
		  {
    
			IList<opennlp.tools.coref.mention.Parse> corefParses = new List<opennlp.tools.coref.mention.Parse>();
    
			int sentNumber = 0;
			foreach (Parse parse in parses)
			{
			  corefParses.Add(new DefaultParse(parse, sentNumber++));
			}
    
			return corefParses;
		  }
	  }

	  public override string ToString()
	  {

		StringBuilder sb = new StringBuilder();

		foreach (Parse parse in parses)
		{
		  parse.show(sb);
		  sb.Append('\n');
		}

		sb.Append('\n');

		return sb.ToString();
	  }

	  public static CorefSample parse(string corefSampleString)
	  {

		IList<Parse> parses = new List<Parse>();

        foreach (string line in Regex.Split(corefSampleString, "\\r?\\n", RegexOptions.None)) // was true
		{
		  parses.Add(Parse.parseParse(line, (HeadRules)null));
		}

		return new CorefSample(parses);
	  }
	}

}