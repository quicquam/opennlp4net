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
using j4n.Lang;
using j4n.Logging;
using j4n.Serialization;
using j4n.Utils;
using opennlp.tools.nonjava;


namespace opennlp.tools.tokenize
{


	using Event = opennlp.model.Event;
	using Factory = opennlp.tools.tokenize.lang.Factory;
	using opennlp.tools.util;
	using opennlp.tools.util;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// This class reads the <seealso cref="TokenSample"/>s from the given <seealso cref="Iterator"/>
	/// and converts the <seealso cref="TokenSample"/>s into <seealso cref="Event"/>s which
	/// can be used by the maxent library for training.
	/// </summary>
	public class TokSpanEventStream : AbstractEventStream<TokenSample>
	{

	  private static Logger logger = Logger.getLogger(typeof(TokSpanEventStream).Name);

	  private TokenContextGenerator cg;

	  private bool skipAlphaNumerics;

	  private readonly Pattern alphaNumeric;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="tokenSamples"> </param>
	  /// <param name="skipAlphaNumerics"> </param>
	  /// <param name="cg"> </param>
	  public TokSpanEventStream(ObjectStream<TokenSample> tokenSamples, bool skipAlphaNumerics, Pattern alphaNumeric, TokenContextGenerator cg) : base(tokenSamples)
	  {
		this.alphaNumeric = alphaNumeric;
		this.skipAlphaNumerics = skipAlphaNumerics;
		this.cg = cg;
	  }

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="tokenSamples"> </param>
	  /// <param name="skipAlphaNumerics"> </param>
	  /// <param name="cg"> </param>
	  public TokSpanEventStream(ObjectStream<TokenSample> tokenSamples, bool skipAlphaNumerics, TokenContextGenerator cg) : base(tokenSamples)
	  {
		Factory factory = new Factory();
		this.alphaNumeric = factory.getAlphanumeric(null);
		this.skipAlphaNumerics = skipAlphaNumerics;
		this.cg = cg;
	  }

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="tokenSamples"> </param>
	  /// <param name="skipAlphaNumerics"> </param>
	  public TokSpanEventStream(ObjectStream<TokenSample> tokenSamples, bool skipAlphaNumerics) : this(tokenSamples, skipAlphaNumerics, new DefaultTokenContextGenerator())
	  {
	  }

	  /// <summary>
	  /// Adds training events to the event stream for each of the specified tokens.
	  /// </summary>
	  /// <param name="tokens"> character offsets into the specified text. </param>
	  /// <param name="text"> The text of the tokens. </param>
	  protected internal override IEnumerator<Event> createEvents(TokenSample tokenSample)
	  {

		IList<Event> events = new List<Event>(50);

		Span[] tokens = tokenSample.TokenSpans;
		string text = tokenSample.Text;

		if (tokens.Length > 0)
		{

		  int start = tokens[0].Start;
		  int end = tokens[tokens.Length - 1].End;

		  string sent = text.Substring(start, end - start);

		  Span[] candTokens = WhitespaceTokenizer.INSTANCE.tokenizePos(sent);

		  int firstTrainingToken = -1;
		  int lastTrainingToken = -1;
		  foreach (Span candToken in candTokens)
		  {
			Span cSpan = candToken;
			string ctok = StringHelperClass.SubstringSpecial(sent, cSpan.Start, cSpan.End);
			//adjust cSpan to text offsets
			cSpan = new Span(cSpan.Start + start, cSpan.End + start);
			//should we skip this token
			if (ctok.Length > 1 && (!skipAlphaNumerics || !alphaNumeric.matcher(ctok).matches()))
			{

			  //find offsets of annotated tokens inside of candidate tokens
			  bool foundTrainingTokens = false;
			  for (int ti = lastTrainingToken + 1; ti < tokens.Length; ti++)
			  {
				if (cSpan.contains(tokens[ti]))
				{
				  if (!foundTrainingTokens)
				  {
					firstTrainingToken = ti;
					foundTrainingTokens = true;
				  }
				  lastTrainingToken = ti;
				}
				else if (cSpan.End < tokens[ti].End)
				{
				  break;
				}
				else if (tokens[ti].End < cSpan.Start)
				{
				  //keep looking
				}
				else
				{
				  if (logger.isLoggable(Level.WARNING))
				  {
					logger.warning("Bad training token: " + tokens[ti] + " cand: " + cSpan + " token=" + StringHelperClass.SubstringSpecial(text, tokens[ti].Start, tokens[ti].End));
				  }
				}
			  }

			  // create training data
			  if (foundTrainingTokens)
			  {

				for (int ti = firstTrainingToken; ti <= lastTrainingToken; ti++)
				{
				  Span tSpan = tokens[ti];
				  int cStart = cSpan.Start;
				  for (int i = tSpan.Start + 1; i < tSpan.End; i++)
				  {
					string[] context = cg.getContext(ctok, i - cStart);
					events.Add(new Event(TokenizerME.NO_SPLIT, context));
				  }

				  if (tSpan.End != cSpan.End)
				  {
					string[] context = cg.getContext(ctok, tSpan.End - cStart);
					events.Add(new Event(TokenizerME.SPLIT, context));
				  }
				}
			  }
			}
		  }
		}

		return events.GetEnumerator();
	  }

	    public override bool hasNext()
	    {
	        throw new System.NotImplementedException();
	    }

	    public override Event next()
	    {
	        throw new System.NotImplementedException();
	    }
	}

}