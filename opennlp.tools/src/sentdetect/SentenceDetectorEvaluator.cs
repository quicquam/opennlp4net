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

namespace opennlp.tools.sentdetect
{

	using Span = opennlp.tools.util.Span;
	using opennlp.tools.util.eval;
	using FMeasure = opennlp.tools.util.eval.FMeasure;

	/// <summary>
	/// The <seealso cref="SentenceDetectorEvaluator"/> measures the performance of
	/// the given <seealso cref="SentenceDetector"/> with the provided reference
	/// <seealso cref="SentenceSample"/>s.
	/// </summary>
	/// <seealso cref= Evaluator </seealso>
	/// <seealso cref= SentenceDetector </seealso>
	/// <seealso cref= SentenceSample </seealso>
	public class SentenceDetectorEvaluator : Evaluator<SentenceSample>
	{

	  private FMeasure fmeasure = new FMeasure();

	  /// <summary>
	  /// The <seealso cref="SentenceDetector"/> used to predict sentences.
	  /// </summary>
	  private SentenceDetector sentenceDetector;

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="sentenceDetector"> </param>
	  /// <param name="listeners"> evaluation sample listeners </param>
	  public SentenceDetectorEvaluator(SentenceDetector sentenceDetector, params SentenceDetectorEvaluationMonitor[] listeners)
          : base(listeners)
	  {
		this.sentenceDetector = sentenceDetector;
	  }

	  protected internal override SentenceSample processSample(SentenceSample sample)
	  {
		Span[] predictions = sentenceDetector.sentPosDetect(sample.Document);
		Span[] references = sample.Sentences;

		fmeasure.updateScores(references, predictions);

		return new SentenceSample(sample.Document, predictions);
	  }

	  public virtual FMeasure FMeasure
	  {
		  get
		  {
			return fmeasure;
		  }
	  }

	    public void evaluate(object testSampleStream)
	    {
	        throw new System.NotImplementedException();
	    }
	}

}