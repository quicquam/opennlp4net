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


namespace opennlp.tools.tokenize
{

	using Span = opennlp.tools.util.Span;
	using opennlp.tools.util.eval;
	using FMeasure = opennlp.tools.util.eval.FMeasure;

	/// <summary>
	/// The <seealso cref="TokenizerEvaluator"/> measures the performance of
	/// the given <seealso cref="Tokenizer"/> with the provided reference
	/// <seealso cref="TokenSample"/>s.
	/// </summary>
	/// <seealso cref= Evaluator </seealso>
	/// <seealso cref= Tokenizer </seealso>
	/// <seealso cref= TokenSample </seealso>
	public class TokenizerEvaluator : Evaluator<TokenSample>
	{

	  private FMeasure fmeasure = new FMeasure();

	  /// <summary>
	  /// The <seealso cref="Tokenizer"/> used to create the
	  /// predicted tokens.
	  /// </summary>
	  private Tokenizer tokenizer;

	  /// <summary>
	  /// Initializes the current instance with the
	  /// given <seealso cref="Tokenizer"/>.
	  /// </summary>
	  /// <param name="tokenizer"> the <seealso cref="Tokenizer"/> to evaluate. </param>
	  /// <param name="listeners"> evaluation sample listeners </param>
	  public TokenizerEvaluator(Tokenizer tokenizer, params TokenizerEvaluationMonitor[] listeners) : base(listeners)
	  {
		this.tokenizer = tokenizer;
	  }

	  protected internal override TokenSample processSample(TokenSample reference)
	  {
		Span[] predictions = tokenizer.tokenizePos(reference.Text);
		fmeasure.updateScores(reference.TokenSpans, predictions);

		return new TokenSample(reference.Text, predictions);
	  }

	  public virtual FMeasure FMeasure
	  {
		  get
		  {
			return fmeasure;
		  }
	  }
	}

}