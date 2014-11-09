using System;
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
using j4n.IO.Reader;

namespace opennlp.tools.lang.english
{


	using NameFinderEventStream = opennlp.tools.namefind.NameFinderEventStream;
	using NameFinderME = opennlp.tools.namefind.NameFinderME;
	using TokenNameFinderModel = opennlp.tools.namefind.TokenNameFinderModel;
	using Parse = opennlp.tools.parser.Parse;
	using SimpleTokenizer = opennlp.tools.tokenize.SimpleTokenizer;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// Class is used to create a name finder for English.
	/// </summary>
	/// @deprecated will be removed soon! 
	[Obsolete("will be removed soon!")]
	public class TreebankNameFinder
	{

	  public static string[] NAME_TYPES = new string[] {"person", "organization", "location", "date", "time", "percentage", "money"};

	  private NameFinderME nameFinder;

	  /// <summary>
	  /// Creates an English name finder using the specified model. </summary>
	  /// <param name="mod"> The model used for finding names. </param>
	  public TreebankNameFinder(TokenNameFinderModel mod)
	  {
		nameFinder = new NameFinderME(mod);
	  }

	  private static void clearPrevTokenMaps(TreebankNameFinder[] finders)
	  {
		for (int mi = 0; mi < finders.Length; mi++)
		{
		  finders[mi].nameFinder.clearAdaptiveData();
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void processParse(TreebankNameFinder[] finders, String[] tags, java.io.BufferedReader input) throws java.io.IOException
	  private static void processParse(TreebankNameFinder[] finders, string[] tags, BufferedReader input)
	  {
		Span[][] nameSpans = new Span[finders.Length][];

		for (string line = input.readLine(); null != line; line = input.readLine())
		{
		  if (line.Equals(""))
		  {
			Console.WriteLine();
			clearPrevTokenMaps(finders);
			continue;
		  }
		  Parse p = Parse.parseParse(line);
		  Parse[] tagNodes = p.TagNodes;
		  string[] tokens = new string[tagNodes.Length];
		  for (int ti = 0;ti < tagNodes.Length;ti++)
		  {
			tokens[ti] = tagNodes[ti].CoveredText;
		  }
		  //System.err.println(java.util.Arrays.asList(tokens));
		  for (int fi = 0, fl = finders.Length; fi < fl; fi++)
		  {
			nameSpans[fi] = finders[fi].nameFinder.find(tokens);
			//System.err.println("english.NameFinder.processParse: "+tags[fi] + " " + java.util.Arrays.asList(nameSpans[fi]));
		  }

		  for (int fi = 0, fl = finders.Length; fi < fl; fi++)
		  {
			Parse.addNames(tags[fi],nameSpans[fi],tagNodes);
		  }
		  p.show();
		}
	  }

	  /// <summary>
	  /// Adds sgml style name tags to the specified input buffer and outputs this information to stdout. </summary>
	  /// <param name="finders"> The name finders to be used. </param>
	  /// <param name="tags"> The tag names for the corresponding name finder. </param>
	  /// <param name="input"> The input reader. </param>
	  /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void processText(TreebankNameFinder[] finders, String[] tags, java.io.BufferedReader input) throws java.io.IOException
	  private static void processText(TreebankNameFinder[] finders, string[] tags, BufferedReader input)
	  {
		Span[][] nameSpans = new Span[finders.Length][];
		string[][] nameOutcomes = new string[finders.Length][];
		opennlp.tools.tokenize.Tokenizer tokenizer = new SimpleTokenizer();
		StringBuilder output = new StringBuilder();
		for (string line = input.readLine(); null != line; line = input.readLine())
		{
		  if (line.Equals(""))
		  {
			clearPrevTokenMaps(finders);
			Console.WriteLine();
			continue;
		  }
		  output.Length = 0;
		  Span[] spans = tokenizer.tokenizePos(line);
		  string[] tokens = Span.spansToStrings(spans,line);
		  for (int fi = 0, fl = finders.Length; fi < fl; fi++)
		  {
			nameSpans[fi] = finders[fi].nameFinder.find(tokens);
			//System.err.println("EnglighNameFinder.processText: "+tags[fi] + " " + java.util.Arrays.asList(finderTags[fi]));
			nameOutcomes[fi] = NameFinderEventStream.generateOutcomes(nameSpans[fi], null, tokens.Length);
		  }

		  for (int ti = 0, tl = tokens.Length; ti < tl; ti++)
		  {
			for (int fi = 0, fl = finders.Length; fi < fl; fi++)
			{
			  //check for end tags
			  if (ti != 0)
			  {
				if ((nameOutcomes[fi][ti].Equals(NameFinderME.START) || nameOutcomes[fi][ti].Equals(NameFinderME.OTHER)) && (nameOutcomes[fi][ti - 1].Equals(NameFinderME.START) || nameOutcomes[fi][ti - 1].Equals(NameFinderME.CONTINUE)))
				{
				  output.Append("</").Append(tags[fi]).Append(">");
				}
			  }
			}
			if (ti > 0 && spans[ti - 1].End < spans[ti].Start)
			{
			  output.Append(StringHelperClass.SubstringSpecial(line, spans[ti - 1].End, spans[ti].Start));
			}
			//check for start tags
			for (int fi = 0, fl = finders.Length; fi < fl; fi++)
			{
			  if (nameOutcomes[fi][ti].Equals(NameFinderME.START))
			  {
				output.Append("<").Append(tags[fi]).Append(">");
			  }
			}
			output.Append(tokens[ti]);
		  }
		  //final end tags
		  if (tokens.Length != 0)
		  {
			for (int fi = 0, fl = finders.Length; fi < fl; fi++)
			{
			  if (nameOutcomes[fi][tokens.Length - 1].Equals(NameFinderME.START) || nameOutcomes[fi][tokens.Length - 1].Equals(NameFinderME.CONTINUE))
			  {
				output.Append("</").Append(tags[fi]).Append(">");
			  }
			}
		  }
		  if (tokens.Length != 0)
		  {
			if (spans[tokens.Length - 1].End < line.length())
			{
			  output.Append(line.Substring(spans[tokens.Length - 1].End));
			}
		  }
		  Console.WriteLine(output);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
	  public static void Main(string[] args)
	  {
		if (args.Length == 0)
		{
		  Console.Error.WriteLine("Usage NameFinder -[parse] model1 model2 ... modelN < sentences");
		  Console.Error.WriteLine(" -parse: Use this option to find names on parsed input.  Un-tokenized sentence text is the default.");
		  Environment.Exit(1);
		}
		int ai = 0;
		bool parsedInput = false;
		while (args[ai].StartsWith("-", StringComparison.Ordinal) && ai < args.Length)
		{
		  if (args[ai].Equals("-parse"))
		  {
			parsedInput = true;
		  }
		  else
		  {
			Console.Error.WriteLine("Ignoring unknown option " + args[ai]);
		  }
		  ai++;
		}
		TreebankNameFinder[] finders = new TreebankNameFinder[args.Length - ai];
		string[] names = new string[args.Length - ai];
		for (int fi = 0; ai < args.Length; ai++,fi++)
		{
		  string modelName = args[ai];
		  finders[fi] = new TreebankNameFinder(new TokenNameFinderModel(new FileInputStream(modelName)));
		  int nameStart = modelName.LastIndexOf(System.getProperty("file.separator")) + 1;
		  int nameEnd = modelName.IndexOf('.', nameStart);
		  if (nameEnd == -1)
		  {
			nameEnd = modelName.Length;
		  }
		  names[fi] = modelName.Substring(nameStart, nameEnd - nameStart);
		}
		//long t1 = System.currentTimeMillis();
		BufferedReader @in = new BufferedReader(new InputStreamReader(Console.OpenStandardInput));
		if (parsedInput)
		{
		  processParse(finders,names,@in);
		}
		else
		{
		  processText(finders,names,@in);
		}
		//long t2 = System.currentTimeMillis();
		//System.err.println("Time "+(t2-t1));
	  }
	}
}