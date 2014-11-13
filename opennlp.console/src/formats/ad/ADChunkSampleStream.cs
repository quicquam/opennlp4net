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
using j4n.Exceptions;
using j4n.IO.InputStream;
using j4n.Serialization;

namespace opennlp.tools.formats.ad
{


	using ChunkSample = opennlp.tools.chunker.ChunkSample;
	using Sentence = opennlp.tools.formats.ad.ADSentenceStream.Sentence;
	using Leaf = opennlp.tools.formats.ad.ADSentenceStream.SentenceParser.Leaf;
	using Node = opennlp.tools.formats.ad.ADSentenceStream.SentenceParser.Node;
	using TreeElement = opennlp.tools.formats.ad.ADSentenceStream.SentenceParser.TreeElement;
	using NameSample = opennlp.tools.namefind.NameSample;
	using opennlp.tools.util;
	using PlainTextByLineStream = opennlp.tools.util.PlainTextByLineStream;

	/// <summary>
	/// Parser for Floresta Sita(c)tica Arvores Deitadas corpus, output to for the
	/// Portuguese Chunker training.
	/// <para>
	/// The heuristic to extract chunks where based o paper 'A Machine Learning
	/// Approach to Portuguese Clause Identification', (Eraldo Fernandes, Cicero
	/// Santos and Ruy Milidiú).<br>
	/// </para>
	/// <para>
	/// Data can be found on this web site:<br>
	/// http://www.linguateca.pt/floresta/corpus.html
	/// </para>
	/// <para>
	/// Information about the format:<br>
	/// Susana Afonso.
	/// "Árvores deitadas: Descrição do formato e das opções de análise na Floresta Sintáctica"
	/// .<br>
	/// 12 de Fevereiro de 2006.
	/// http://www.linguateca.pt/documentos/Afonso2006ArvoresDeitadas.pdf
	/// </para>
	/// <para>
	/// Detailed info about the NER tagset:
	/// http://beta.visl.sdu.dk/visl/pt/info/portsymbol.html#semtags_names
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class ADChunkSampleStream : ObjectStream<ChunkSample>
	{

		protected internal readonly ObjectStream<ADSentenceStream.Sentence> adSentenceStream;

		private int start = -1;
		private int end = -1;

		private int index = 0;

		public const string OTHER = "O";

		/// <summary>
		/// Creates a new <seealso cref="NameSample"/> stream from a line stream, i.e.
		/// <seealso cref="ObjectStream"/>< <seealso cref="String"/>>, that could be a
		/// <seealso cref="PlainTextByLineStream"/> object.
		/// </summary>
		/// <param name="lineStream">
		///          a stream of lines as <seealso cref="String"/> </param>
		public ADChunkSampleStream(ObjectStream<string> lineStream)
		{
		  this.adSentenceStream = new ADSentenceStream(lineStream);
		}

		/// <summary>
		/// Creates a new <seealso cref="NameSample"/> stream from a <seealso cref="InputStream"/>
		/// </summary>
		/// <param name="in">
		///          the Corpus <seealso cref="InputStream"/> </param>
		/// <param name="charsetName">
		///          the charset of the Arvores Deitadas Corpus </param>
		public ADChunkSampleStream(InputStream @in, string charsetName)
		{

			try
			{
				this.adSentenceStream = new ADSentenceStream(new PlainTextByLineStream(@in, charsetName));
			}
			catch (UnsupportedEncodingException e)
			{
				// UTF-8 is available on all JVMs, will never happen
				throw new IllegalStateException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public opennlp.tools.chunker.ChunkSample read() throws java.io.IOException
		public virtual ChunkSample read()
		{

			Sentence paragraph;
			while ((paragraph = this.adSentenceStream.read()) != null)
			{

				if (end > -1 && index >= end)
				{
					// leave
					return null;
				}

				if (start > -1 && index < start)
				{
					index++;
					// skip this one
				}
				else
				{
					Node root = paragraph.Root;
					IList<string> sentence = new List<string>();
					IList<string> tags = new List<string>();
					IList<string> target = new List<string>();

					processRoot(root, sentence, tags, target);

					if (sentence.Count > 0)
					{
						index++;
						return new ChunkSample(sentence, tags, target);
					}

				}

			}
			return null;
		}

		protected internal virtual void processRoot(Node root, IList<string> sentence, IList<string> tags, IList<string> target)
		{
			if (root != null)
			{
				TreeElement[] elements = root.Elements;
				for (int i = 0; i < elements.Length; i++)
				{
					if (elements[i].Leaf)
					{
						processLeaf((Leaf) elements[i], false, OTHER, sentence, tags, target);
					}
					else
					{
						processNode((Node) elements[i], sentence, tags, target, null);
					}
				}
			}
		}

		private void processNode(Node node, IList<string> sentence, IList<string> tags, IList<string> target, string inheritedTag)
		{
		string phraseTag = getChunkTag(node);

		bool inherited = false;
		if (phraseTag.Equals(OTHER) && inheritedTag != null)
		{
		  phraseTag = inheritedTag;
		  inherited = true;
		}

		TreeElement[] elements = node.Elements;
		for (int i = 0; i < elements.Length; i++)
		{
			if (elements[i].Leaf)
			{
				bool isIntermediate = false;
				string tag = phraseTag;
				Leaf leaf = (Leaf) elements[i];

				string localChunk = getChunkTag(leaf);
				if (localChunk != null && !tag.Equals(localChunk))
				{
				  tag = localChunk;
				}

				if (isIntermediate(tags, target, tag) && (inherited || i > 0))
				{
					  isIntermediate = true;
				}
				if (!IncludePunctuations && leaf.FunctionalTag == null && (!(i + 1 < elements.Length && elements[i + 1].Leaf) || !(i > 0 && elements[i - 1].Leaf)))
				{
				  isIntermediate = false;
				  tag = OTHER;
				}
				processLeaf(leaf, isIntermediate, tag, sentence, tags, target);
			}
			else
			{
				int before = target.Count;
				processNode((Node) elements[i], sentence, tags, target, phraseTag);

				// if the child node was of a different type we should break the chunk sequence
				for (int j = target.Count - 1; j >= before; j--)
				{
				  if (!target[j].EndsWith("-" + phraseTag, StringComparison.Ordinal))
				  {
					phraseTag = OTHER;
					break;
				  }
				}
			}
		}
		}


	  protected internal virtual void processLeaf(Leaf leaf, bool isIntermediate, string phraseTag, IList<string> sentence, IList<string> tags, IList<string> target)
	  {
			string chunkTag;

			if (leaf.FunctionalTag != null && phraseTag.Equals(OTHER))
			{
			  phraseTag = getPhraseTagFromPosTag(leaf.FunctionalTag);
			}

			if (!phraseTag.Equals(OTHER))
			{
				if (isIntermediate)
				{
					chunkTag = "I-" + phraseTag;
				}
				else
				{
					chunkTag = "B-" + phraseTag;
				}
			}
			else
			{
				chunkTag = phraseTag;
			}

			sentence.Add(leaf.Lexeme);
			if (leaf.SyntacticTag == null)
			{
				tags.Add(leaf.Lexeme);
			}
			else
			{
				tags.Add(ADChunkSampleStream.convertFuncTag(leaf.FunctionalTag, false));
			}
			target.Add(chunkTag);
	  }

	  protected internal virtual string getPhraseTagFromPosTag(string functionalTag)
	  {
		if (functionalTag.Equals("v-fin"))
		{
		  return "VP";
		}
		else if (functionalTag.Equals("n"))
		{
		  return "NP";
		}
		return OTHER;
	  }

	  public static string convertFuncTag(string t, bool useCGTags)
	  {
		if (useCGTags)
		{
		  if ("art".Equals(t) || "pron-det".Equals(t) || "pron-indef".Equals(t))
		  {
			t = "det";
		  }
		}
		return t;
	  }

	  protected internal virtual string getChunkTag(Leaf leaf)
	  {
		string tag = leaf.SyntacticTag;
		if ("P".Equals(tag))
		{
		  return "VP";
		}
		return null;
	  }

	  protected internal virtual string getChunkTag(Node node)
	  {
		string tag = node.SyntacticTag;

		string phraseTag = tag.Substring(tag.LastIndexOf(":", StringComparison.Ordinal) + 1);

		while (phraseTag.EndsWith("-", StringComparison.Ordinal))
		{
		  phraseTag = phraseTag.Substring(0, phraseTag.Length - 1);
		}

		// maybe we should use only np, vp and pp, but will keep ap and advp.
		if (phraseTag.Equals("np") || phraseTag.Equals("vp") || phraseTag.Equals("pp") || phraseTag.Equals("ap") || phraseTag.Equals("advp") || phraseTag.Equals("adjp"))
		{
		  phraseTag = phraseTag.ToUpper();
		}
		else
		{
		  phraseTag = OTHER;
		}
		return phraseTag;
	  }

		public virtual int Start
		{
			set
			{
				this.start = value;
			}
		}

		public virtual int End
		{
			set
			{
				this.end = value;
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

	  protected internal virtual bool IncludePunctuations
	  {
		  get
		  {
			return false;
		  }
	  }

	  protected internal virtual bool isIntermediate(IList<string> tags, IList<string> target, string phraseTag)
	  {
		return target.Count > 0 && target[target.Count - 1].EndsWith("-" + phraseTag, StringComparison.Ordinal);
	  }

	}

}