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
using System.Linq;
using System.Text.RegularExpressions;
using j4n.Exceptions;
using j4n.IO.InputStream;
using j4n.Lang;
using j4n.Serialization;
using opennlp.tools.namefind;
using opennlp.tools.nonjava.extensions;
using opennlp.tools.util;

namespace opennlp.console.formats.ad
{
    /// <summary>
	/// Parser for Floresta Sita(c)tica Arvores Deitadas corpus, output to for the
	/// Portuguese NER training.
	/// <para>
	/// The data contains four named entity types: Person, Organization, Group,
	/// Place, Event, ArtProd, Abstract, Thing, Time and Numeric.<br>
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
	public class ADNameSampleStream : ObjectStream<NameSample>
	{

	  /// <summary>
	  /// Pattern of a NER tag in Arvores Deitadas 
	  /// </summary>
	  private static readonly Pattern tagPattern = Pattern.compile("<(NER:)?(.*?)>");

	  private static readonly Pattern whitespacePattern = Pattern.compile("\\s+");
	  private static readonly Pattern underlinePattern = Pattern.compile("[_]+");
	  private static readonly Pattern hyphenPattern = Pattern.compile("((\\p{L}+)-$)|(^-(\\p{L}+)(.*))|((\\p{L}+)-(\\p{L}+)(.*))");
	  private static readonly Pattern alphanumericPattern = Pattern.compile("^[\\p{L}\\p{Nd}]+$");

	  /// <summary>
	  /// Map to the Arvores Deitadas types to our types. It is read-only.
	  /// </summary>
	  private static readonly IDictionary<string, string> HAREM;

	  static ADNameSampleStream()
	  {
		IDictionary<string, string> harem = new Dictionary<string, string>();

		const string person = "person";
		harem["hum"] = person;
		harem["official"] = person;
		harem["member"] = person;

		const string organization = "organization";
		harem["admin"] = organization;
		harem["org"] = organization;
		harem["inst"] = organization;
		harem["media"] = organization;
		harem["party"] = organization;
		harem["suborg"] = organization;

		const string group = "group";
		harem["groupind"] = group;
		harem["groupofficial"] = group;

		const string place = "place";
		harem["top"] = place;
		harem["civ"] = place;
		harem["address"] = place;
		harem["site"] = place;
		harem["virtual"] = place;
		harem["astro"] = place;

		const string @event = "event";
		harem["occ"] = @event;
		harem["event"] = @event;
		harem["history"] = @event;

		const string artprod = "artprod";
		harem["tit"] = artprod;
		harem["pub"] = artprod;
		harem["product"] = artprod;
		harem["V"] = artprod;
		harem["artwork"] = artprod;

		const string _abstract = "abstract";
		harem["brand"] = _abstract;
		harem["genre"] = _abstract;
		harem["school"] = _abstract;
		harem["idea"] = _abstract;
		harem["plan"] = _abstract;
		harem["author"] = _abstract;
		harem["absname"] = _abstract;
		harem["disease"] = _abstract;

		const string thing = "thing";
		harem["object"] = thing;
		harem["common"] = thing;
		harem["mat"] = thing;
		harem["class"] = thing;
		harem["plant"] = thing;
		harem["currency"] = thing;

		const string time = "time";
		harem["date"] = time;
		harem["hour"] = time;
		harem["period"] = time;
		harem["cyclic"] = time;

		const string numeric = "numeric";
		harem["quantity"] = numeric;
		harem["prednum"] = numeric;
		harem["currency"] = numeric;

		HAREM = harem;
	  }

	  private readonly ObjectStream<ADSentenceStream.Sentence> adSentenceStream;

	  /// <summary>
	  /// To keep the last left contraction part
	  /// </summary>
	  private string leftContractionPart = null;

	  private readonly bool splitHyphenatedTokens;

	  /// <summary>
	  /// Creates a new <seealso cref="NameSample"/> stream from a line stream, i.e.
	  /// <seealso cref="ObjectStream"/>< <seealso cref="String"/>>, that could be a
	  /// <seealso cref="PlainTextByLineStream"/> object.
	  /// </summary>
	  /// <param name="lineStream">
	  ///          a stream of lines as <seealso cref="String"/> </param>
	  /// <param name="splitHyphenatedTokens">
	  ///          if true hyphenated tokens will be separated: "carros-monstro" >
	  ///          "carros" "-" "monstro" </param>
	  public ADNameSampleStream(ObjectStream<string> lineStream, bool splitHyphenatedTokens)
	  {
		this.adSentenceStream = new ADSentenceStream(lineStream);
		this.splitHyphenatedTokens = splitHyphenatedTokens;
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="NameSample"/> stream from a <seealso cref="InputStream"/>
	  /// </summary>
	  /// <param name="in">
	  ///          the Corpus <seealso cref="InputStream"/> </param>
	  /// <param name="charsetName">
	  ///          the charset of the Arvores Deitadas Corpus </param>
	  /// <param name="splitHyphenatedTokens">
	  ///          if true hyphenated tokens will be separated: "carros-monstro" >
	  ///          "carros" "-" "monstro" </param>
	  public ADNameSampleStream(InputStream @in, string charsetName, bool splitHyphenatedTokens)
	  {

		try
		{
		  this.adSentenceStream = new ADSentenceStream(new PlainTextByLineStream(@in, charsetName));
		  this.splitHyphenatedTokens = splitHyphenatedTokens;
		}
		catch (UnsupportedEncodingException e)
		{
		  // UTF-8 is available on all JVMs, will never happen
		  throw new IllegalStateException(e);
		}
	  }

	  internal int textID = -1;

	  public virtual NameSample read()
	  {

		ADSentenceStream.Sentence paragraph;
		// we should look for text here.
		while ((paragraph = this.adSentenceStream.read()) != null)
		{

		  int currentTextID = getTextID(paragraph);
		  bool clearData = false;
		  if (currentTextID != textID)
		  {
			clearData = true;
			textID = currentTextID;
		  }

		  ADSentenceStream.SentenceParser.Node root = paragraph.Root;
		  IList<string> sentence = new List<string>();
		  IList<Span> names = new List<Span>();
		  process(root, sentence, names);

		  return new NameSample(sentence.ToArray(), names.ToArray(), clearData);
		}
		return null;
	  }

	  /// <summary>
	  /// Recursive method to process a node in Arvores Deitadas format.
	  /// </summary>
	  /// <param name="node">
	  ///          the node to be processed </param>
	  /// <param name="sentence">
	  ///          the sentence tokens we got so far </param>
	  /// <param name="names">
	  ///          the names we got so far </param>
	  private void process(ADSentenceStream.SentenceParser.Node node, IList<string> sentence, IList<Span> names)
	  {
		if (node != null)
		{
		  foreach (ADSentenceStream.SentenceParser.TreeElement element in node.Elements)
		  {
			if (element.Leaf)
			{
			  processLeaf((ADSentenceStream.SentenceParser.Leaf) element, sentence, names);
			}
			else
			{
			  process((ADSentenceStream.SentenceParser.Node) element, sentence, names);
			}
		  }
		}
	  }

	  /// <summary>
	  /// Process a Leaf of Arvores Detaitadas format
	  /// </summary>
	  /// <param name="leaf">
	  ///          the leaf to be processed </param>
	  /// <param name="sentence">
	  ///          the sentence tokens we got so far </param>
	  /// <param name="names">
	  ///          the names we got so far </param>
	  private void processLeaf(ADSentenceStream.SentenceParser.Leaf leaf, IList<string> sentence, IList<Span> names)
	  {

		bool alreadyAdded = false;

		if (leftContractionPart != null)
		{
		  // will handle the contraction
		  string right = leaf.Lexeme;

		  string c = PortugueseContractionUtility.toContraction(leftContractionPart, right);
		  if (c != null)
		  {
			string[] parts = whitespacePattern.Split(c);
			sentence.AddRange(parts);
			alreadyAdded = true;
		  }
		  else
		  {
			// contraction was missing! why?
			sentence.Add(leftContractionPart);
			// keep alreadyAdded false.
		  }
		  leftContractionPart = null;
		}

		  string namedEntityTag = null;
		  int startOfNamedEntity = -1;

		  string leafTag = leaf.SecondaryTag;
		  bool expandLastNER = false; // used when we find a <NER2> tag

		  if (leafTag != null)
		  {
			if (leafTag.Contains("<sam->") && !alreadyAdded)
			{
			  string[] lexemes = underlinePattern.Split(leaf.Lexeme);
			  if (lexemes.Length > 1)
			  {
				 sentence.AddRange(lexemes.ToList().GetRange(0, lexemes.Length - 1));
			  }
			  leftContractionPart = lexemes[lexemes.Length - 1];
			  return;
			}
			if (leafTag.Contains("<NER2>"))
			{
			  // this one an be part of the last name
			  expandLastNER = true;
			}
			namedEntityTag = getNER(leafTag);
		  }

		  if (namedEntityTag != null)
		  {
			startOfNamedEntity = sentence.Count;
		  }

		  if (!alreadyAdded)
		  {
			sentence.AddRange(processLexeme(leaf.Lexeme));
		  }

		  if (namedEntityTag != null)
		  {
			names.Add(new Span(startOfNamedEntity, sentence.Count, namedEntityTag));
		  }

		  if (expandLastNER)
		  {
			// if the current leaf has the tag <NER2>, it can be the continuation of
			// a NER.
			// we check if it is true, and expand the last NER
			int lastIndex = names.Count - 1;
			Span last = null;
			bool error = false;
			if (names.Count > 0)
			{
			  last = names[lastIndex];
			  if (last.End == sentence.Count - 1)
			  {
				names[lastIndex] = new Span(last.Start, sentence.Count, last.Type);
			  }
			  else
			  {
				error = true;
			  }
			}
			else
			{
			  error = true;
			}
			if (error)
			{
	//           Maybe it is not the same NER, skip it.
	//           Console.Error.println("Missing NER start for sentence [" + sentence
	//           + "] node [" + leaf + "]");
			}
		  }

	  }

	  private IList<string> processLexeme(string lexemeStr)
	  {
		IList<string> @out = new List<string>();
		string[] parts = underlinePattern.Split(lexemeStr);
		foreach (string tok in parts)
		{
		  if (tok.Length > 1 && !alphanumericPattern.matcher(tok).matches())
		  {
			@out.AddRange(processTok(tok));
		  }
		  else
		  {
			@out.Add(tok);
		  }
		}
		return @out;
	  }

	  private IList<string> processTok(string tok)
	  {
		bool tokAdded = false;
		string original = tok;
		IList<string> @out = new List<string>();
		LinkedList<string> suffix = new LinkedList<string>();
		char first = tok[0];
		if (first == '«')
		{
		  @out.Add(char.ToString(first));
		  tok = tok.Substring(1);
		}
		char last = tok[tok.Length - 1];
		if (last == '»' || last == ':' || last == ',' || last == '!')
		{
		  suffix.AddLast(char.ToString(last));
		  tok = tok.Substring(0, tok.Length - 1);
		}

		// lets split all hyphens
		if (this.splitHyphenatedTokens && tok.Contains("-") && tok.Length > 1)
		{
		  Matcher matcher = hyphenPattern.matcher(tok);

		  string firstTok = null;
		  string hyphen = "-";
		  string secondTok = null;
		  string rest = null;

		  if (matcher.matches())
		  {
			if (matcher.group(1) != null)
			{
			  firstTok = matcher.group(2);
			}
			else if (matcher.group(3) != null)
			{
			  secondTok = matcher.group(4);
			  rest = matcher.group(5);
			}
			else if (matcher.group(6) != null)
			{
			  firstTok = matcher.group(7);
			  secondTok = matcher.group(8);
			  rest = matcher.group(9);
			}

			addIfNotEmpty(firstTok, @out);
			addIfNotEmpty(hyphen, @out);
			addIfNotEmpty(secondTok, @out);
			addIfNotEmpty(rest, @out);
			tokAdded = true;
		  }
		}
		if (!tokAdded)
		{
		  if (!original.Equals(tok) && tok.Length > 1 && !alphanumericPattern.matcher(tok).matches())
		  {
			@out.AddRange(processTok(tok));
		  }
		  else
		  {
			@out.Add(tok);
		  }
		}
		@out.AddRange(suffix.ToList());
		return @out;
	  }

	  private void addIfNotEmpty(string firstTok, IList<string> @out)
	  {
		if (firstTok != null && firstTok.Length > 0)
		{
		  @out.AddRange(processTok(firstTok));
		}
	  }

	  /// <summary>
	  /// Parse a NER tag in Arvores Deitadas format.
	  /// </summary>
	  /// <param name="tags">
	  ///          the NER tag in Arvores Deitadas format </param>
	  /// <returns> the NER tag, or null if not a NER tag in Arvores Deitadas format </returns>
	  private static string getNER(string tags)
	  {
		if (tags.Contains("<NER2>"))
		{
		  return null;
		}
        var regex = new Regex("\\s+");
	    string[] tag = regex.Split(tags);
		foreach (string t in tag)
		{
		  Matcher matcher = tagPattern.matcher(t);
		  if (matcher.matches())
		  {
			string ner = matcher.group(2);
			if (HAREM.ContainsKey(ner))
			{
			  return HAREM[ner];
			}
		  }
		}
		return null;
	  }

	  public virtual void reset()
	  {
		adSentenceStream.reset();
	  }

	  public virtual void close()
	  {
		adSentenceStream.close();
	  }

	  internal enum Type
	  {
		ama,
		cie,
		lit
	  }

	  private Type? corpusType = null;

	  private Pattern metaPattern;

	  // works for Amazonia
	//  private static final Pattern meta1 = Pattern
	//      .compile("^(?:[a-zA-Z\\-]*(\\d+)).*?p=(\\d+).*");
	//  
	//  // works for selva cie
	//  private static final Pattern meta2 = Pattern
	//    .compile("^(?:[a-zA-Z\\-]*(\\d+)).*?p=(\\d+).*");

	  private int textIdMeta2 = -1;
	  private string textMeta2 = "";

	  private int getTextID(ADSentenceStream.Sentence paragraph)
	  {

		string meta = paragraph.Metadata;

		if (corpusType == null)
		{
		  if (meta.StartsWith("LIT", StringComparison.Ordinal))
		  {
			corpusType = Type.lit;
			metaPattern = Pattern.compile("^([a-zA-Z\\-]+)(\\d+).*?p=(\\d+).*");
		  }
		  else if (meta.StartsWith("CIE", StringComparison.Ordinal))
		  {
			corpusType = Type.cie;
			metaPattern = Pattern.compile("^.*?source=\"(.*?)\".*");
		  } // ama
		  else
		  {
			corpusType = Type.ama;
			metaPattern = Pattern.compile("^(?:[a-zA-Z\\-]*(\\d+)).*?p=(\\d+).*");
		  }
		}

		if (corpusType.Equals(Type.lit))
		{
		  Matcher m2 = metaPattern.matcher(meta);
		  if (m2.matches())
		  {
			string textId = m2.group(1);
			if (!textId.Equals(textMeta2))
			{
			  textIdMeta2++;
			  textMeta2 = textId;
			}
			return textIdMeta2;
		  }
		  else
		  {
			throw new Exception("Invalid metadata: " + meta);
		  }
		}
		else if (corpusType.Equals(Type.cie))
		{
		  Matcher m2 = metaPattern.matcher(meta);
		  if (m2.matches())
		  {
			string textId = m2.group(1);
			if (!textId.Equals(textMeta2))
			{
			  textIdMeta2++;
			  textMeta2 = textId;
			}
			return textIdMeta2;
		  }
		  else
		  {
			throw new Exception("Invalid metadata: " + meta);
		  }
		}
		else if (corpusType.Equals(Type.ama))
		{
		  Matcher m2 = metaPattern.matcher(meta);
		  if (m2.matches())
		  {
			return Convert.ToInt32(m2.group(1));
			// currentPara = Integer.parseInt(m.group(2));
		  }
		  else
		  {
			throw new Exception("Invalid metadata: " + meta);
		  }
		}

		return 0;
	  }

	}

}