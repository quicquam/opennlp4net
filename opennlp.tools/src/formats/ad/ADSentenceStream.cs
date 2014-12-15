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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using j4n.IO.Reader;
using j4n.Lang;
using opennlp.tools.util;

namespace opennlp.tools.formats.ad
{
    /// <summary>
	/// Stream filter which merges text lines into sentences, following the Arvores
	/// Deitadas syntax.
	/// <para>
	/// Information about the format:<br>
	/// Susana Afonso.
	/// "Árvores deitadas: Descrição do formato e das opções de análise na Floresta Sintáctica"
	/// .<br>
	/// 12 de Fevereiro de 2006. 
	/// http://www.linguateca.pt/documentos/Afonso2006ArvoresDeitadas.pdf 
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class ADSentenceStream : FilterObjectStream<string, ADSentenceStream.Sentence>
	{

	  public class Sentence
	  {

		internal string text;
		internal SentenceParser.Node root;
		internal string metadata;

		public const string META_LABEL_FINAL = "final";

		public virtual string Text
		{
			get
			{
			  return text;
			}
			set
			{
			  this.text = value;
			}
		}


		public virtual SentenceParser.Node Root
		{
			get
			{
			  return root;
			}
			set
			{
			  this.root = value;
			}
		}


		public virtual string Metadata
		{
			set
			{
				this.metadata = value;
			}
			get
			{
				return metadata;
			}
		}


	  }

	  /// <summary>
	  /// Parses a sample of AD corpus. A sentence in AD corpus is represented by a
	  /// Tree. In this class we declare some types to represent that tree. Today we get only
	  /// the first alternative (A1).
	  /// </summary>
	  public class SentenceParser
	  {

		internal Pattern nodePattern = Pattern.compile("([=-]*)([^:=]+:[^\\(\\s]+)(\\(([^\\)]+)\\))?\\s*(?:(\\((<.+>)\\))*)\\s*$");
		internal Pattern leafPattern = Pattern.compile("^([=-]*)([^:=]+):([^\\(\\s]+)\\([\"'](.+)[\"']\\s*((?:<.+>)*)\\s*([^\\)]+)?\\)\\s+(.+)");
		internal Pattern bizarreLeafPattern = Pattern.compile("^([=-]*)([^:=]+=[^\\(\\s]+)\\(([\"'].+[\"'])?\\s*([^\\)]+)?\\)\\s+(.+)");
		internal Pattern punctuationPattern = Pattern.compile("^(=*)(\\W+)$");

		internal string text, meta;

		/// <summary>
		/// Parse the sentence 
		/// </summary>
		public virtual Sentence parse(string sentenceString, int para, bool isTitle, bool isBox)
		{
		  BufferedReader reader = new BufferedReader(new StringReader(sentenceString));
		  Sentence sentence = new Sentence();
		  Node root = new Node(this);
		  try
		  {
			// first line is <s ...>
			string line = reader.readLine();

			bool useSameTextAndMeta = false; // to handle cases where there are diff sug of parse (&&)

			  // should find the source source
			  while (!line.StartsWith("SOURCE", StringComparison.Ordinal))
			  {
				  if (line.Equals("&&"))
				  {
					  // same sentence again!
					  useSameTextAndMeta = true;
					  break;
				  }
				line = reader.readLine();
				if (line == null)
				{
				  return null;
				}
			  }
			if (!useSameTextAndMeta)
			{
				// got source, get the metadata
				string metaFromSource = line.Substring(7);
				line = reader.readLine();
				// we should have the plain sentence
				// we remove the first token
				int start = line.IndexOf(" ", StringComparison.Ordinal);
				text = line.Substring(start + 1).Trim();
				text = fixPunctuation(text);
				string titleTag = "";
				if (isTitle)
				{
					titleTag = " title";
				}
				string boxTag = "";
				if (isBox)
				{
					boxTag = " box";
				}
				if (start > 0)
				{
				  meta = line.Substring(0, start) + " p=" + para + titleTag + boxTag + metaFromSource;
				}
				else
				{
				  // rare case were there is no space between id and the sentence.
				  // will use previous meta for now
				}
			}
			sentence.Text = text;
			sentence.Metadata = meta;
			// now we look for the root node

			// skip lines starting with ###
			line = reader.readLine();
			while (line != null && line.StartsWith("###", StringComparison.Ordinal))
			{
				line = reader.readLine();
			}

			// got the root. Add it to the stack
			Stack<Node> nodeStack = new Stack<Node>();

			root.SyntacticTag = "ROOT";
			root.Level = 0;
			nodeStack.Push(root);


			/* now we have to take care of the lastLevel. Every time it raises, we will add the
			leaf to the node at the top. If it decreases, we remove the top. */

			while (line != null && line.Length != 0 && line.StartsWith("</s>", StringComparison.Ordinal) == false && !line.Equals("&&"))
			{
			  TreeElement element = this.getElement(line);

			  if (element != null)
			  {
				// The idea here is to keep a stack of nodes that are candidates for
				// parenting the following elements (nodes and leafs).

				// 1) When we get a new element, we check its level and remove from
				// the top of the stack nodes that are brothers or nephews.
				while (nodeStack.Count > 0 && element.Level > 0 && element.Level <= nodeStack.Peek().Level)
				{
				  Node nephew = nodeStack.Pop();
				}

				if (element.Leaf)
				{
				  // 2a) If the element is a leaf and there is no parent candidate,
				  // add it as a daughter of the root.  
				  if (nodeStack.Count == 0)
				  {
					root.addElement(element);
				  }
				  else
				  {
					// 2b) There are parent candidates. 
					// look for the node with the correct level
					Node peek = nodeStack.Peek();
					if (element.level == 0) // add to the root
					{
					  nodeStack.First().addElement(element);
					}
					else
					{
					  Node parent = null;
					  int index = nodeStack.Count - 1;
					  while (parent == null)
					  {
						if (peek.Level < element.Level)
						{
						  parent = peek;
						}
						else
						{
						  index--;
						  if (index > -1)
						  {
							peek = nodeStack.ElementAt(index);
						  }
						  else
						  {
							parent = nodeStack.First();
						  }
						}
					  }
					  parent.addElement(element);
					}
				  }
				}
				else
				{
				  // 3) Check if the element that is at the top of the stack is this
				  // node parent, if yes add it as a son 
				  if (nodeStack.Count > 0 && nodeStack.Peek().Level < element.Level)
				  {
					  nodeStack.Peek().addElement(element);
				  }
				  else
				  {
					Console.Error.WriteLine("should not happen!");
				  }
				  // 4) Add it to the stack so it is a parent candidate.
				  nodeStack.Push((Node) element);

				}
			  }
			  line = reader.readLine();
			}

		  }
		  catch (Exception e)
		  {
			Console.Error.WriteLine(sentenceString);
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			return sentence;
		  }
		  // second line should be SOURCE
		  sentence.Root = root;
		  return sentence;
		}

		internal virtual string fixPunctuation(string text)
		{
		  text = text.Replace("\\»\\s+\\.", "».");
		  text = text.Replace("\\»\\s+\\,", "»,");
		  return text;
		}

		/// <summary>
		/// Parse a tree element from a AD line
		/// </summary>
		/// <param name="line">
		///          the AD line </param>
		/// <returns> the tree element </returns>
		public virtual TreeElement getElement(string line)
		{
		  // Note: all levels are higher than 1, because 0 is reserved for the root.

		  // try node
		  Matcher nodeMatcher = nodePattern.matcher(line);
		  if (nodeMatcher.matches())
		  {
			int level = nodeMatcher.group(1).Length + 1;
			string syntacticTag = nodeMatcher.group(2);
			Node node = new Node(this);
			node.Level = level;
			node.SyntacticTag = syntacticTag;
			return node;
		  }

		  Matcher leafMatcher = leafPattern.matcher(line);
		  if (leafMatcher.matches())
		  {
			int level = leafMatcher.group(1).Length + 1;
			string syntacticTag = leafMatcher.group(2);
			string funcTag = leafMatcher.group(3);
			string lemma = leafMatcher.group(4);
			string secondaryTag = leafMatcher.group(5);
			string morphologicalTag = leafMatcher.group(6);
			string lexeme = leafMatcher.group(7);
			Leaf leaf = new Leaf(this);
			leaf.Level = level;
			leaf.SyntacticTag = syntacticTag;
			leaf.FunctionalTag = funcTag;
			leaf.SecondaryTag = secondaryTag;
			leaf.MorphologicalTag = morphologicalTag;
			leaf.Lexeme = lexeme;
			leaf.Lemma = lemma;

			return leaf;
		  }

		  Matcher punctuationMatcher = punctuationPattern.matcher(line);
		  if (punctuationMatcher.matches())
		  {
			int level = punctuationMatcher.group(1).Length + 1;
			string lexeme = punctuationMatcher.group(2);
			Leaf innerleaf = new Leaf(this);
            innerleaf.Level = level;
            innerleaf.Lexeme = lexeme;
            return innerleaf;
		  }

		  // process the bizarre cases
		  if (line.Equals("_") || line.StartsWith("<lixo", StringComparison.Ordinal) || line.StartsWith("pause", StringComparison.Ordinal))
		  {
			  return null;
		  }

		  if (line.StartsWith("=", StringComparison.Ordinal))
		  {
			  Matcher bizarreLeafMatcher = bizarreLeafPattern.matcher(line);
			if (bizarreLeafMatcher.matches())
			{
			  int level = bizarreLeafMatcher.group(1).Length + 1;
			  string syntacticTag = bizarreLeafMatcher.group(2);
			  string lemma = bizarreLeafMatcher.group(3);
			  string morphologicalTag = bizarreLeafMatcher.group(4);
			  string lexeme = bizarreLeafMatcher.group(5);
			  Leaf innerleaf = new Leaf(this);
              innerleaf.Level = level;
              innerleaf.SyntacticTag = syntacticTag;
              innerleaf.MorphologicalTag = morphologicalTag;
              innerleaf.Lexeme = lexeme;
			  if (lemma != null)
			  {
				if (lemma.Length > 2)
				{
				  lemma = lemma.Substring(1, lemma.Length - 1 - 1);
				}
                innerleaf.Lemma = lemma;
			  }

              return innerleaf;
			}
			else
			{
				int level = line.LastIndexOf("=", StringComparison.Ordinal) + 1;
				string lexeme = line.Substring(level + 1);
                var regex = new Regex("\\w.*?[\\.<>].*");
                if (regex.IsMatch(lexeme))
				{
				  return null;
				}

                Leaf innerleaf = new Leaf(this);
                innerleaf.Level = level + 1;
                innerleaf.SyntacticTag = "";
                innerleaf.MorphologicalTag = "";
                innerleaf.Lexeme = lexeme;

                return innerleaf;
			}
		  }

		  Console.Error.WriteLine("Couldn't parse leaf: " + line);
		  Leaf errorleaf = new Leaf(this);
          errorleaf.Level = 1;
          errorleaf.SyntacticTag = "";
          errorleaf.MorphologicalTag = "";
          errorleaf.Lexeme = line;

          return errorleaf;
		}

		/// <summary>
		/// Represents a tree element, Node or Leaf </summary>
		public abstract class TreeElement
		{
			private readonly ADSentenceStream.SentenceParser outerInstance;

			public TreeElement(ADSentenceStream.SentenceParser outerInstance)
			{
				this.outerInstance = outerInstance;
			}


		  internal string syntacticTag;
		  internal string morphologicalTag;
		  internal int level;

		  public virtual bool Leaf
		  {
			  get
			  {
				  return false;
			  }
		  }

		  public virtual string SyntacticTag
		  {
			  set
			  {
				this.syntacticTag = value;
			  }
			  get
			  {
				return syntacticTag;
			  }
		  }


		  public virtual int Level
		  {
			  set
			  {
				this.level = value;
			  }
			  get
			  {
				return level;
			  }
		  }


		  public virtual string MorphologicalTag
		  {
			  set
			  {
				this.morphologicalTag = value;
			  }
			  get
			  {
				return morphologicalTag;
			  }
		  }

		}

		/// <summary>
		/// Represents the AD node </summary>
		public class Node : TreeElement
		{
			private readonly ADSentenceStream.SentenceParser outerInstance;

			public Node(ADSentenceStream.SentenceParser outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		  internal IList<TreeElement> elems = new List<TreeElement>();

		  public virtual void addElement(TreeElement element)
		  {
			elems.Add(element);
		  }

		  public virtual TreeElement[] Elements
		  {
			  get
			  {
				return elems.ToArray();
			  }
		  }

		  public override string ToString()
		  {
			StringBuilder sb = new StringBuilder();
			// print itself and its children
			for (int i = 0; i < this.Level; i++)
			{
			  sb.Append("=");
			}
			sb.Append(this.SyntacticTag);
			if (this.MorphologicalTag != null)
			{
			  sb.Append(this.MorphologicalTag);
			}
			sb.Append("\n");
			foreach (TreeElement element in elems)
			{
			  sb.Append(element.ToString());
			}
			return sb.ToString();
		  }
		}

		/// <summary>
		/// Represents the AD leaf </summary>
		public class Leaf : TreeElement
		{
			private readonly ADSentenceStream.SentenceParser outerInstance;

			public Leaf(ADSentenceStream.SentenceParser outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}


		  internal string word;
		  internal string lemma;
		  internal string secondaryTag;
		  internal string functionalTag;

		  public bool isLeaf()
		  {
			  return true;
		  }

		  public virtual string FunctionalTag
		  {
			  set
			  {
				this.functionalTag = value;
			  }
			  get
			  {
				return this.functionalTag;
			  }
		  }


		  public virtual string SecondaryTag
		  {
			  set
			  {
				this.secondaryTag = value;
			  }
			  get
			  {
				return this.secondaryTag;
			  }
		  }


		  public virtual string Lexeme
		  {
			  set
			  {
				this.word = value;
			  }
			  get
			  {
				return word;
			  }
		  }


		  internal virtual string emptyOrString(string value, string prefix, string suffix)
		  {
			if (value == null)
			{
				return "";
			}
			return prefix + value + suffix;
		  }

		  public override string ToString()
		  {
			StringBuilder sb = new StringBuilder();
			// print itself and its children
			for (int i = 0; i < this.Level; i++)
			{
			  sb.Append("=");
			}
			if (this.SyntacticTag != null)
			{
			  sb.Append(this.SyntacticTag).Append(":").Append(FunctionalTag).Append("(").Append(emptyOrString(Lemma, "'", "' ")).Append(emptyOrString(SecondaryTag, "", " ")).Append(this.MorphologicalTag).Append(") ");
			}
			sb.Append(this.word).Append("\n");
			return sb.ToString();
		  }

		  public virtual string Lemma
		  {
			  set
			  {
				this.lemma = value;
			  }
			  get
			  {
				return lemma;
			  }
		  }

		}

	  }

	  /// <summary>
	  /// The start sentence pattern 
	  /// </summary>
	  private static readonly Pattern sentStart = Pattern.compile("<s[^>]*>");

	  /// <summary>
	  /// The end sentence pattern 
	  /// </summary>
	  private static readonly Pattern sentEnd = Pattern.compile("</s>");
	  private static readonly Pattern extEnd = Pattern.compile("</ext>");

	  /// <summary>
	  /// The start sentence pattern 
	  /// </summary>
	  private static readonly Pattern titleStart = Pattern.compile("<t[^>]*>");

	  /// <summary>
	  /// The end sentence pattern 
	  /// </summary>
	  private static readonly Pattern titleEnd = Pattern.compile("</t>");

	  /// <summary>
	  /// The start sentence pattern 
	  /// </summary>
	  private static readonly Pattern boxStart = Pattern.compile("<caixa[^>]*>");

	  /// <summary>
	  /// The end sentence pattern 
	  /// </summary>
	  private static readonly Pattern boxEnd = Pattern.compile("</caixa>");


	  /// <summary>
	  /// The start sentence pattern 
	  /// </summary>
	  private static readonly Pattern paraStart = Pattern.compile("<p[^>]*>");

	  /// <summary>
	  /// The start sentence pattern 
	  /// </summary>
	  private static readonly Pattern textStart = Pattern.compile("<ext[^>]*>");

	  private SentenceParser parser;

	  private int paraID = 0;
	  private bool isTitle = false;
	  private bool isBox = false;

	  public ADSentenceStream(ObjectStream<string> lineStream) : base(lineStream)
	  {
		parser = new SentenceParser();
	  }


	  public override Sentence read()
	  {

		StringBuilder sentence = new StringBuilder();
		bool sentenceStarted = false;

		while (true)
		{
		  string line = samples.read();

		  if (line != null)
		  {

			  if (sentenceStarted)
			  {
				  if (sentEnd.matcher(line).matches() || extEnd.matcher(line).matches())
				  {
					  sentenceStarted = false;
				  }
				  else if (line.StartsWith("A1", StringComparison.Ordinal))
				  {
					// skip
				  }
				  else
				  {
					  sentence.Append(line).Append('\n');
				  }
			  }
			  else
			  {
				  if (sentStart.matcher(line).matches())
				  {
					  sentenceStarted = true;
				  }
					else if (paraStart.matcher(line).matches())
					{
						paraID++;
					}
					else if (titleStart.matcher(line).matches())
					{
						isTitle = true;
					}
					else if (titleEnd.matcher(line).matches())
					{
						isTitle = false;
					}
					else if (textStart.matcher(line).matches())
					{
						paraID = 0;
					}
					else if (boxStart.matcher(line).matches())
					{
						isBox = true;
					}
					else if (boxEnd.matcher(line).matches())
					{
						isBox = false;
					}
			  }


			if (!sentenceStarted && sentence.Length > 0)
			{
			  return parser.parse(sentence.ToString(), paraID, isTitle, isBox);
			}

		  }
		  else
		  {
			// handle end of file
			if (sentenceStarted)
			{
			  if (sentence.Length > 0)
			  {
				return parser.parse(sentence.ToString(), paraID, isTitle, isBox);
			  }
			}
			else
			{
			  return null;
			}
		  }
		}
	  }
	}

}