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


namespace opennlp.tools.doccat
{

	using POSSample = opennlp.tools.postag.POSSample;
	using TokenSample = opennlp.tools.tokenize.TokenSample;
	using Mean = opennlp.tools.util.eval.Mean;

	/// <summary>
	/// The <seealso cref="DocumentCategorizerEvaluator"/> measures the performance of
	/// the given <seealso cref="DocumentCategorizer"/> with the provided reference
	/// <seealso cref="DocumentSample"/>s.
	/// </summary>
	/// <seealso cref= DocumentCategorizer </seealso>
	/// <seealso cref= DocumentSample </seealso>
	public class DocumentCategorizerEvaluator
	{

	  private DocumentCategorizer categorizer;

	  private Mean accuracy = new Mean();

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="categorizer"> </param>
	  public DocumentCategorizerEvaluator(DocumentCategorizer categorizer)
	  {
		this.categorizer = categorizer;
	  }

	  /// <summary>
	  /// Evaluates the given reference <seealso cref="DocumentSample"/> object.
	  /// 
	  /// This is done by categorizing the document from the provided
	  /// <seealso cref="DocumentSample"/>. The detected category is then used
	  /// to calculate and update the score.
	  /// </summary>
	  /// <param name="sample"> the reference <seealso cref="TokenSample"/>. </param>
	  public virtual void evaluteSample(DocumentSample sample)
	  {

		string[] document = sample.Text;

		double[] probs = categorizer.categorize(document);

		string cat = categorizer.getBestCategory(probs);

		if (sample.Category.Equals(cat))
		{
		  accuracy.add(1);
		}
		else
		{
		  accuracy.add(0);
		}
	  }

	  /// <summary>
	  /// Reads all <seealso cref="DocumentSample"/> objects from the stream
	  /// and evaluates each <seealso cref="DocumentSample"/> object with
	  /// <seealso cref="#evaluteSample(DocumentSample)"/> method.
	  /// </summary>
	  /// <param name="samples"> the stream of reference <seealso cref="POSSample"/> which
	  /// should be evaluated. </param>
	  public virtual void evaluate(IEnumerator<DocumentSample> samples)
	  {

		while (samples.MoveNext())
		{
		  evaluteSample(samples.Current);
		}
	  }

	  /// <summary>
	  /// Retrieves the accuracy of provided <seealso cref="DocumentCategorizer"/>.
	  /// 
	  /// accuracy = correctly categorized documents / total documents
	  /// </summary>
	  /// <returns> the accuracy </returns>
	  public virtual double Accuracy
	  {
		  get
		  {
			return accuracy.mean();
		  }
	  }

	  /// <summary>
	  /// Represents this objects as human readable <seealso cref="String"/>.
	  /// </summary>
	  public override string ToString()
	  {
		return "Accuracy: " + accuracy.mean() + "\n" + "Number of documents: " + accuracy.count();
	  }
	}

}