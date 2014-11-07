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


namespace opennlp.tools.chunker
{

	using opennlp.tools.util.eval;
	using FMeasure = opennlp.tools.util.eval.FMeasure;

	/// <summary>
	/// The <seealso cref="ChunkerEvaluator"/> measures the performance
	/// of the given <seealso cref="Chunker"/> with the provided
	/// reference <seealso cref="ChunkSample"/>s.
	/// </summary>
	/// <seealso cref= Evaluator </seealso>
	/// <seealso cref= Chunker </seealso>
	/// <seealso cref= ChunkSample </seealso>
	public class ChunkerEvaluator : Evaluator<ChunkSample>
	{

	  private FMeasure fmeasure = new FMeasure();

	  /// <summary>
	  /// The <seealso cref="Chunker"/> used to create the predicted
	  /// <seealso cref="ChunkSample"/> objects.
	  /// </summary>
	  private Chunker chunker;

	  /// <summary>
	  /// Initializes the current instance with the given
	  /// <seealso cref="Chunker"/>.
	  /// </summary>
	  /// <param name="chunker"> the <seealso cref="Chunker"/> to evaluate. </param>
	  /// <param name="listeners"> evaluation listeners </param>
	  public ChunkerEvaluator(Chunker chunker, params ChunkerEvaluationMonitor[] listeners) : base(listeners)
	  {
		this.chunker = chunker;
	  }

	  /// <summary>
	  /// Evaluates the given reference <seealso cref="ChunkSample"/> object.
	  /// 
	  /// This is done by finding the phrases with the
	  /// <seealso cref="Chunker"/> in the sentence from the reference
	  /// <seealso cref="ChunkSample"/>. The found phrases are then used to
	  /// calculate and update the scores.
	  /// </summary>
	  /// <param name="reference"> the reference <seealso cref="ChunkSample"/>.
	  /// </param>
	  /// <returns> the predicted sample </returns>
	  protected internal override ChunkSample processSample(ChunkSample reference)
	  {
		string[] preds = chunker.chunk(reference.Sentence, reference.Tags);
		ChunkSample result = new ChunkSample(reference.Sentence, reference.Tags, preds);

		fmeasure.updateScores(reference.PhrasesAsSpanList, result.PhrasesAsSpanList);

		return result;
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