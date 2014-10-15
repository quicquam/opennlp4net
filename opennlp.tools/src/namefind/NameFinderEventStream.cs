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
using j4n.IO.Reader;
using j4n.Serialization;

namespace opennlp.tools.namefind
{


	using Event = opennlp.model.Event;
	using EventStream = opennlp.model.EventStream;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;
	using Span = opennlp.tools.util.Span;
	using AdditionalContextFeatureGenerator = opennlp.tools.util.featuregen.AdditionalContextFeatureGenerator;
	using WindowFeatureGenerator = opennlp.tools.util.featuregen.WindowFeatureGenerator;

	/// <summary>
	/// Class for creating an event stream out of data files for training an name
	/// finder.
	/// </summary>
	public class NameFinderEventStream : opennlp.tools.util.AbstractEventStream<NameSample>
	{

	  private NameContextGenerator contextGenerator;

	  private AdditionalContextFeatureGenerator additionalContextFeatureGenerator = new AdditionalContextFeatureGenerator();

	  private string type;

	  /// <summary>
	  /// Creates a new name finder event stream using the specified data stream and context generator. </summary>
	  /// <param name="dataStream"> The data stream of events. </param>
	  /// <param name="type"> null or overrides the type parameter in the provided samples </param>
	  /// <param name="contextGenerator"> The context generator used to generate features for the event stream. </param>
	  public NameFinderEventStream(ObjectStream<NameSample> dataStream, string type, NameContextGenerator contextGenerator) : base(dataStream)
	  {

		this.contextGenerator = contextGenerator;
		this.contextGenerator.addFeatureGenerator(new WindowFeatureGenerator(additionalContextFeatureGenerator, 8, 8));

		if (type != null)
		{
		  this.type = type;
		}
		else
		{
		  this.type = "default";
		}
	  }

	  public NameFinderEventStream(ObjectStream<NameSample> dataStream) : this(dataStream, null, new DefaultNameContextGenerator())
	  {
	  }

	  /// <summary>
	  /// Generates the name tag outcomes (start, continue, other) for each token in a sentence
	  /// with the specified length using the specified name spans. </summary>
	  /// <param name="names"> Token spans for each of the names. </param>
	  /// <param name="type"> null or overrides the type parameter in the provided samples </param>
	  /// <param name="length"> The length of the sentence. </param>
	  /// <returns> An array of start, continue, other outcomes based on the specified names and sentence length. </returns>
	  public static string[] generateOutcomes(Span[] names, string type, int length)
	  {
		string[] outcomes = new string[length];
		for (int i = 0; i < outcomes.Length; i++)
		{
		  outcomes[i] = NameFinderME.OTHER;
		}
		foreach (Span name in names)
		{
		  if (name.Type == null)
		  {
			outcomes[name.Start] = type + "-" + NameFinderME.START;
		  }
		  else
		  {
			outcomes[name.Start] = name.Type + "-" + NameFinderME.START;
		  }
		  // now iterate from begin + 1 till end
		  for (int i = name.Start + 1; i < name.End; i++)
		  {
			if (name.Type == null)
			{
			  outcomes[i] = type + "-" + NameFinderME.CONTINUE;
			}
			else
			{
			  outcomes[i] = name.Type + "-" + NameFinderME.CONTINUE;
			}
		  }
		}
		return outcomes;
	  }

	  public static List<Event> generateEvents(string[] sentence, string[] outcomes, NameContextGenerator cg)
	  {
		List<Event> events = new List<Event>(outcomes.Length);
		for (int i = 0; i < outcomes.Length; i++)
		{
		  events.Add(new Event(outcomes[i], cg.getContext(i, sentence, outcomes,null)));
		}

		cg.updateAdaptiveData(sentence, outcomes);

		return events;
	  }

	  protected internal override IEnumerator<Event> createEvents(NameSample sample)
	  {

		if (sample.ClearAdaptiveDataSet)
		{
		  contextGenerator.clearAdaptiveData();
		}

		string[] outcomes = generateOutcomes(sample.Names, type, sample.Sentence.Length);
		additionalContextFeatureGenerator.CurrentContext = sample.AdditionalContext;
		string[] tokens = new string[sample.Sentence.Length];

		for (int i = 0; i < sample.Sentence.Length; i++)
		{
		  tokens[i] = sample.Sentence[i];
		}

		return generateEvents(tokens, outcomes, contextGenerator).GetEnumerator();
	  }


	  /// <summary>
	  /// Generated previous decision features for each token based on contents of the specified map. </summary>
	  /// <param name="tokens"> The token for which the context is generated. </param>
	  /// <param name="prevMap"> A mapping of tokens to their previous decisions. </param>
	  /// <returns> An additional context array with features for each token. </returns>
	  public static string[][] additionalContext(string[] tokens, IDictionary<string, string> prevMap)
	  {
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: string[][] ac = new string[tokens.Length][1];
		string[][] ac = RectangularArrays.ReturnRectangularStringArray(tokens.Length, 1);
		for (int ti = 0;ti < tokens.Length;ti++)
		{
		  string pt = prevMap[tokens[ti]];
		  ac[ti][0] = "pd=" + pt;
		}
		return ac;

	  }

	  // Will be removed soon!
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated public static final void main(String[] args) throws java.io.IOException
	  [Obsolete]
	  public static void main(string[] args)
	  {
		if (args.Length != 0)
		{
		  Console.Error.WriteLine("Usage: NameFinderEventStream < training files");
		  Environment.Exit(1);
		}
        EventStream es = new NameFinderEventStream(new NameSampleDataStream(new PlainTextByLineStream(new InputStreamReader(Console.OpenStandardInput()))));
		while (es.hasNext())
		{
		  Console.WriteLine(es.next());
		}
	  }
	}

}