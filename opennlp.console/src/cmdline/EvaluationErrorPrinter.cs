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
using j4n.Interfaces;
using j4n.IO.OutputStream;

namespace opennlp.tools.cmdline
{


	using Span = opennlp.tools.util.Span;
	using opennlp.tools.util.eval;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public abstract class EvaluationErrorPrinter<T> : EvaluationMonitor<T>
	{

	  private PrintStream printStream;

	  protected internal EvaluationErrorPrinter(OutputStream outputStream)
	  {
		this.printStream = new PrintStream(outputStream);
	  }

	    protected EvaluationErrorPrinter(TextWriter outputStream)
	    {
	        throw new System.NotImplementedException();
	    }

	    // for the sentence detector
	  protected internal virtual void printError(Span[] references, Span[] predictions, T referenceSample, T predictedSample, string sentence)
	  {
		IList<Span> falseNegatives = new List<Span>();
		IList<Span> falsePositives = new List<Span>();

		findErrors(references, predictions, falseNegatives, falsePositives);

		if (falsePositives.Count + falseNegatives.Count > 0)
		{

		  printSamples(referenceSample, predictedSample);

		  printErrors(falsePositives, falseNegatives, sentence);

		}
	  }

	  // for namefinder, chunker...
	  protected internal virtual void printError(Span[] references, Span[] predictions, T referenceSample, T predictedSample, string[] sentenceTokens)
	  {
		IList<Span> falseNegatives = new List<Span>();
		IList<Span> falsePositives = new List<Span>();

		findErrors(references, predictions, falseNegatives, falsePositives);

		if (falsePositives.Count + falseNegatives.Count > 0)
		{

		  printSamples(referenceSample, predictedSample);

		  printErrors(falsePositives, falseNegatives, sentenceTokens);

		}
	  }

	  // for pos tagger
	  protected internal virtual void printError(string[] references, string[] predictions, T referenceSample, T predictedSample, string[] sentenceTokens)
	  {
		IList<string> filteredDoc = new List<string>();
		IList<string> filteredRefs = new List<string>();
		IList<string> filteredPreds = new List<string>();

		for (int i = 0; i < references.Length; i++)
		{
		  if (!references[i].Equals(predictions[i]))
		  {
			filteredDoc.Add(sentenceTokens[i]);
			filteredRefs.Add(references[i]);
			filteredPreds.Add(predictions[i]);
		  }
		}

		if (filteredDoc.Count > 0)
		{

		  printSamples(referenceSample, predictedSample);

		  printErrors(filteredDoc, filteredRefs, filteredPreds);

		}
	  }

	  /// <summary>
	  /// Auxiliary method to print tag errors
	  /// </summary>
	  /// <param name="filteredDoc">
	  ///          the document tokens which were tagged wrong </param>
	  /// <param name="filteredRefs">
	  ///          the reference tags </param>
	  /// <param name="filteredPreds">
	  ///          the predicted tags </param>
	  private void printErrors(IList<string> filteredDoc, IList<string> filteredRefs, IList<string> filteredPreds)
	  {
		printStream.println("Errors: {");
		printStream.println("Tok: Ref | Pred");
		printStream.println("---------------");
		for (int i = 0; i < filteredDoc.Count; i++)
		{
		  printStream.println(filteredDoc[i] + ": " + filteredRefs[i] + " | " + filteredPreds[i]);
		}
		printStream.println("}\n");
	  }

	  /// <summary>
	  /// Auxiliary method to print span errors
	  /// </summary>
	  /// <param name="falsePositives">
	  ///          false positives span </param>
	  /// <param name="falseNegatives">
	  ///          false negative span </param>
	  /// <param name="doc">
	  ///          the document text </param>
	  private void printErrors(IList<Span> falsePositives, IList<Span> falseNegatives, string doc)
	  {
		printStream.println("False positives: {");
		foreach (Span span in falsePositives)
		{
		  printStream.println(span.getCoveredText(doc));
		}
		printStream.println("} False negatives: {");
		foreach (Span span in falseNegatives)
		{
		  printStream.println(span.getCoveredText(doc));
		}
		printStream.println("}\n");
	  }

	  /// <summary>
	  /// Auxiliary method to print span errors
	  /// </summary>
	  /// <param name="falsePositives">
	  ///          false positives span </param>
	  /// <param name="falseNegatives">
	  ///          false negative span </param>
	  /// <param name="toks">
	  ///          the document tokens </param>
	  private void printErrors(IList<Span> falsePositives, IList<Span> falseNegatives, string[] toks)
	  {
		printStream.println("False positives: {");
		printStream.println(print(falsePositives, toks));
		printStream.println("} False negatives: {");
		printStream.println(print(falseNegatives, toks));
		printStream.println("}\n");
	  }

	  /// <summary>
	  /// Auxiliary method to print spans
	  /// </summary>
	  /// <param name="spans">
	  ///          the span list </param>
	  /// <param name="toks">
	  ///          the tokens array </param>
	  /// <returns> the spans as string </returns>
	  private string print(IList<Span> spans, string[] toks)
	  {
		return Arrays.ToString(Span.spansToStrings(spans.ToArray(), toks));
	  }

	  /// <summary>
	  /// Auxiliary method to print expected and predicted samples.
	  /// </summary>
	  /// <param name="referenceSample">
	  ///          the reference sample </param>
	  /// <param name="predictedSample">
	  ///          the predicted sample </param>
	  private void printSamples<S>(S referenceSample, S predictedSample)
	  {
		string details = "Expected: {\n" + referenceSample + "}\nPredicted: {\n" + predictedSample + "}";
		printStream.println(details);
	  }

	  /// <summary>
	  /// Outputs falseNegatives and falsePositives spans from the references and
	  /// predictions list.
	  /// </summary>
	  /// <param name="references"> </param>
	  /// <param name="predictions"> </param>
	  /// <param name="falseNegatives">
	  ///          [out] the false negatives list </param>
	  /// <param name="falsePositives">
	  ///          [out] the false positives list </param>
	  private void findErrors(Span[] references, Span[] predictions, IList<Span> falseNegatives, IList<Span> falsePositives)
	  {

		falseNegatives.AddRange(Arrays.asList(references));
		falsePositives.AddRange(Arrays.asList(predictions));

		for (int referenceIndex = 0; referenceIndex < references.Length; referenceIndex++)
		{

		  Span referenceName = references[referenceIndex];

		  for (int predictedIndex = 0; predictedIndex < predictions.Length; predictedIndex++)
		  {
			if (referenceName.Equals(predictions[predictedIndex]))
			{
			  // got it, remove from fn and fp
			  falseNegatives.Remove(referenceName);
			  falsePositives.Remove(predictions[predictedIndex]);
			}
		  }
		}
	  }

	  public virtual void correctlyClassified(T reference, T prediction)
	  {
		// do nothing
	  }

	  public abstract void missclassified(T reference, T prediction);

	}

}