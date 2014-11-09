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
using j4n.Exceptions;
using j4n.IO.InputStream;
using j4n.Object;

namespace opennlp.tools.formats.ad
{


	using Sentence = opennlp.tools.formats.ad.ADSentenceStream.Sentence;
	using Leaf = opennlp.tools.formats.ad.ADSentenceStream.SentenceParser.Leaf;
	using Node = opennlp.tools.formats.ad.ADSentenceStream.SentenceParser.Node;
	using TreeElement = opennlp.tools.formats.ad.ADSentenceStream.SentenceParser.TreeElement;
	using POSSample = opennlp.tools.postag.POSSample;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </summary>
	public class ADPOSSampleStream : ObjectStream<POSSample>
	{

	  private readonly ObjectStream<ADSentenceStream.Sentence> adSentenceStream;
	  private bool expandME;
	  private bool isIncludeFeatures;

	  /// <summary>
	  /// Creates a new <seealso cref="POSSample"/> stream from a line stream, i.e.
	  /// <seealso cref="ObjectStream"/>< <seealso cref="String"/>>, that could be a
	  /// <seealso cref="PlainTextByLineStream"/> object.
	  /// </summary>
	  /// <param name="lineStream">
	  ///          a stream of lines as <seealso cref="String"/> </param>
	  /// <param name="expandME">
	  ///          if true will expand the multiword expressions, each word of the
	  ///          expression will have the POS Tag that was attributed to the
	  ///          expression plus the prefix B- or I- (CONLL convention) </param>
	  /// <param name="includeFeatures">
	  ///          if true will combine the POS Tag with the feature tags </param>
	  public ADPOSSampleStream(ObjectStream<string> lineStream, bool expandME, bool includeFeatures)
	  {
		this.adSentenceStream = new ADSentenceStream(lineStream);
		this.expandME = expandME;
		this.isIncludeFeatures = includeFeatures;
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="POSSample"/> stream from a <seealso cref="InputStream"/>
	  /// </summary>
	  /// <param name="in">
	  ///          the Corpus <seealso cref="InputStream"/> </param>
	  /// <param name="charsetName">
	  ///          the charset of the Arvores Deitadas Corpus </param>
	  /// <param name="expandME">
	  ///          if true will expand the multiword expressions, each word of the
	  ///          expression will have the POS Tag that was attributed to the
	  ///          expression plus the prefix B- or I- (CONLL convention) </param>
	  /// <param name="includeFeatures">
	  ///          if true will combine the POS Tag with the feature tags </param>
	  public ADPOSSampleStream(InputStream @in, string charsetName, bool expandME, bool includeFeatures)
	  {

		try
		{
		  this.adSentenceStream = new ADSentenceStream(new PlainTextByLineStream(@in, charsetName));
		  this.expandME = expandME;
		  this.isIncludeFeatures = includeFeatures;
		}
		catch (UnsupportedEncodingException e)
		{
		  // UTF-8 is available on all JVMs, will never happen
		  throw new IllegalStateException(e);
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.postag.POSSample read() throws java.io.IOException
	  public virtual POSSample read()
	  {
		Sentence paragraph;
		while ((paragraph = this.adSentenceStream.read()) != null)
		{
		  Node root = paragraph.Root;
		  IList<string> sentence = new List<string>();
		  IList<string> tags = new List<string>();
		  process(root, sentence, tags);

		  return new POSSample(sentence, tags);
		}
		return null;
	  }

	  private void process(Node node, IList<string> sentence, IList<string> tags)
	  {
		if (node != null)
		{
		  foreach (TreeElement element in node.Elements)
		  {
			if (element.Leaf)
			{
			  processLeaf((Leaf) element, sentence, tags);
			}
			else
			{
			  process((Node) element, sentence, tags);
			}
		  }
		}
	  }

	  private void processLeaf(Leaf leaf, IList<string> sentence, IList<string> tags)
	  {
		if (leaf != null)
		{
		  string lexeme = leaf.Lexeme;
		  string tag = leaf.FunctionalTag;

		  if (tag == null)
		  {
			tag = leaf.Lexeme;
		  }

		  if (isIncludeFeatures && leaf.MorphologicalTag != null)
		  {
			tag += " " + leaf.MorphologicalTag;
		  }
		  tag = tag.replaceAll("\\s+", "=");

		  if (tag == null)
		  {
			tag = lexeme;
		  }

		  if (expandME && lexeme.Contains("_"))
		  {
			StringTokenizer tokenizer = new StringTokenizer(lexeme, "_");

			if (tokenizer.countTokens() > 0)
			{
			  IList<string> toks = new List<string>(tokenizer.countTokens());
			  IList<string> tagsWithCont = new List<string>(tokenizer.countTokens());
			  toks.Add(tokenizer.nextToken());
			  tagsWithCont.Add("B-" + tag);
			  while (tokenizer.hasMoreTokens())
			  {
				toks.Add(tokenizer.nextToken());
				tagsWithCont.Add("I-" + tag);
			  }

			  sentence.AddRange(toks);
			  tags.AddRange(tagsWithCont);
			}
			else
			{
			  sentence.Add(lexeme);
			  tags.Add(tag);
			}

		  }
		  else
		  {
			sentence.Add(lexeme);
			tags.Add(tag);
		  }
		}

	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reset() throws java.io.IOException, UnsupportedOperationException
	  public virtual void reset()
	  {
		adSentenceStream.reset();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
	  public virtual void close()
	  {
		adSentenceStream.close();
	  }
	}

}