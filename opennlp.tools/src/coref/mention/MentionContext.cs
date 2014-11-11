using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace opennlp.tools.coref.mention
{

	using Context = opennlp.tools.coref.sim.Context;
	using GenderEnum = opennlp.tools.coref.sim.GenderEnum;
	using NumberEnum = opennlp.tools.coref.sim.NumberEnum;
	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// Data structure representation of a mention with additional contextual information. 
	/// The contextual information is used in performing coreference resolution.
	/// </summary>
	public class MentionContext : Context
	{

	  /// <summary>
	  /// The index of first token which is not part of a descriptor.  This is 0 if no descriptor is present. 
	  /// </summary>
	  private int nonDescriptorStart;

	  /// <summary>
	  /// The Parse of the head constituent of this mention.
	  /// </summary>
	  private Parse head;

	  /// <summary>
	  /// Sentence-token-based span whose end is the last token of the mention.
	  /// </summary>
	  private Span indexSpan;

	  /// <summary>
	  /// Position of the NP in the sentence.
	  /// </summary>
	  private int nounLocation;

	  /// <summary>
	  /// Position of the NP in the document.
	  /// </summary>
	  private int nounNumber;

	  /// <summary>
	  /// Number of noun phrases in the sentence which contains this mention.
	  /// </summary>
	  private int maxNounLocation;

	  /// <summary>
	  /// Index of the sentence in the document which contains this mention. 
	  /// </summary>
	  private int sentenceNumber;

	  /// <summary>
	  /// The token preceding this mention's maximal noun phrase.
	  /// </summary>
	  private Parse prevToken;

	  /// <summary>
	  /// The token following this mention's maximal noun phrase.
	  /// </summary>
	  private Parse nextToken;

	  /// <summary>
	  /// The token following this mention's basal noun phrase.
	  /// </summary>
	  private Parse basalNextToken;

	  /// <summary>
	  /// The parse of the mention's head word. 
	  /// </summary>
	  private Parse headToken;

	  /// <summary>
	  /// The parse of the first word in the mention. 
	  /// </summary>
	  private Parse firstToken;

	  /// <summary>
	  /// The text of the first word in the mention.
	  /// </summary>
	  private string firstTokenText;

	  /// <summary>
	  /// The pos-tag of the first word in the mention. 
	  /// </summary>
	  private string firstTokenTag;

	  /// <summary>
	  /// The gender assigned to this mention. 
	  /// </summary>
	  private GenderEnum gender;

	  /// <summary>
	  /// The probability associated with the gender assignment. 
	  /// </summary>
	  private double genderProb;

	  /// <summary>
	  /// The number assigned to this mention.
	  /// </summary>
	  private NumberEnum number;

	  /// <summary>
	  /// The probability associated with the number assignment. 
	  /// </summary>
	  private double numberProb;

	  public MentionContext(Span span, Span headSpan, int entityId, Parse parse, string extentType, string nameType, int mentionIndex, int mentionsInSentence, int mentionIndexInDocument, int sentenceIndex, HeadFinder headFinder) : base(span,headSpan,entityId,parse,extentType,nameType,headFinder)
	  {
		nounLocation = mentionIndex;
		maxNounLocation = mentionsInSentence;
		nounNumber = mentionIndexInDocument;
		sentenceNumber = sentenceIndex;
		indexSpan = parse.Span;
		prevToken = parse.PreviousToken;
		nextToken = parse.NextToken;
		head = headFinder.getLastHead(parse);
		IList<Parse> headTokens = head.Tokens;
		tokens = headTokens.ToArray();
		basalNextToken = head.NextToken;
		//System.err.println("MentionContext.init: "+ent+" "+ent.getEntityId()+" head="+head);
		nonDescriptorStart = 0;
		initHeads(headFinder.getHeadIndex(head));
		gender = GenderEnum.UNKNOWN;
		this.genderProb = 0d;
		number = NumberEnum.UNKNOWN;
		this.numberProb = 0d;
	  }
	  /// <summary>
	  /// Constructs context information for the specified mention.
	  /// </summary>
	  /// <param name="mention"> The mention object on which this object is based. </param>
	  /// <param name="mentionIndexInSentence"> The mention's position in the sentence. </param>
	  /// <param name="mentionsInSentence"> The number of mentions in the sentence. </param>
	  /// <param name="mentionIndexInDocument"> The index of this mention with respect to the document. </param>
	  /// <param name="sentenceIndex"> The index of the sentence which contains this mention. </param>
	  /// <param name="headFinder"> An object which provides head information. </param>
	  public MentionContext(Mention mention, int mentionIndexInSentence, int mentionsInSentence, int mentionIndexInDocument, int sentenceIndex, HeadFinder headFinder) : this(mention.Span,mention.HeadSpan,mention.Id,mention.Parse,mention.type,mention.nameType, mentionIndexInSentence,mentionsInSentence,mentionIndexInDocument,sentenceIndex,headFinder)
	  {
	  }


	  /// <summary>
	  /// Constructs context information for the specified mention.
	  /// </summary>
	  /// <param name="mentionParse"> Mention parse structure for which context is to be constructed. </param>
	  /// <param name="mentionIndex"> mention position in sentence. </param>
	  /// <param name="mentionsInSentence"> Number of mentions in the sentence. </param>
	  /// <param name="mentionsInDocument"> Number of mentions in the document. </param>
	  /// <param name="sentenceIndex"> Sentence number for this mention. </param>
	  /// <param name="nameType"> The named-entity type for this mention. </param>
	  /// <param name="headFinder"> Object which provides head information. </param>
	  /*
	  public MentionContext(Parse mentionParse, int mentionIndex, int mentionsInSentence, int mentionsInDocument, int sentenceIndex, String nameType, HeadFinder headFinder) {
	    nounLocation = mentionIndex;
	    maxNounLocation = mentionsInDocument;
	    sentenceNumber = sentenceIndex;
	    parse = mentionParse;
	    indexSpan = mentionParse.getSpan();
	    prevToken = mentionParse.getPreviousToken();
	    nextToken = mentionParse.getNextToken();
	    head = headFinder.getLastHead(mentionParse);
	    List headTokens = head.getTokens();
	    tokens = (Parse[]) headTokens.toArray(new Parse[headTokens.size()]);
	    basalNextToken = head.getNextToken();
	    //System.err.println("MentionContext.init: "+ent+" "+ent.getEntityId()+" head="+head);
	    indexHeadSpan = head.getSpan();
	    nonDescriptorStart = 0;
	    initHeads(headFinder.getHeadIndex(head));
	    this.neType= nameType;
	    if (getHeadTokenTag().startsWith("NN") && !getHeadTokenTag().startsWith("NNP")) {
	      //if (headTokenTag.startsWith("NNP") && neType != null) {
	      this.synsets = getSynsetSet(this);
	    }
	    else {
	      this.synsets=Collections.EMPTY_SET;
	    }
	    gender = GenderEnum.UNKNOWN;
	    this.genderProb = 0d;
	    number = NumberEnum.UNKNOWN;
	    this.numberProb = 0d;
	  }
	  */

	  private void initHeads(int headIndex)
	  {
		this.headTokenIndex = headIndex;
		this.headToken = (Parse) tokens[HeadTokenIndex];
		this.headTokenText = headToken.ToString();
		this.headTokenTag = headToken.SyntacticType;
		this.firstToken = (Parse) tokens[0];
		this.firstTokenTag = firstToken.SyntacticType;
		this.firstTokenText = firstToken.ToString();
	  }

	  /// <summary>
	  /// Returns the parse of the head token for this mention.
	  /// </summary>
	  /// <returns> the parse of the head token for this mention. </returns>
	  public virtual Parse HeadTokenParse
	  {
		  get
		  {
			return headToken;
		  }
	  }

	  public virtual string HeadText
	  {
		  get
		  {
			StringBuilder headText = new StringBuilder();
			for (int hsi = 0; hsi < tokens.Length; hsi++)
			{
			  headText.Append(" ").Append(tokens[hsi].ToString());
			}
			return headText.ToString().Substring(1);
		  }
	  }

	  public virtual Parse Head
	  {
		  get
		  {
			return head;
		  }
	  }

	  public virtual int NonDescriptorStart
	  {
		  get
		  {
			return this.nonDescriptorStart;
		  }
	  }

	  /// <summary>
	  /// Returns a sentence-based token span for this mention.  If this mention consist
	  /// of the third, fourth, and fifth token, then this span will be 2..4.
	  /// </summary>
	  /// <returns> a sentence-based token span for this mention. </returns>
	  public virtual Span IndexSpan
	  {
		  get
		  {
			return indexSpan;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the noun phrase for this mention in a sentence.
	  /// </summary>
	  /// <returns> the index of the noun phrase for this mention in a sentence. </returns>
	  public virtual int NounPhraseSentenceIndex
	  {
		  get
		  {
			return nounLocation;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the noun phrase for this mention in a document.
	  /// </summary>
	  /// <returns> the index of the noun phrase for this mention in a document. </returns>
	  public virtual int NounPhraseDocumentIndex
	  {
		  get
		  {
			return nounNumber;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the last noun phrase in the sentence containing this mention.
	  /// This is one less than the number of noun phrases in the sentence which contains this mention.
	  /// </summary>
	  /// <returns> the index of the last noun phrase in the sentence containing this mention. </returns>
	  public virtual int MaxNounPhraseSentenceIndex
	  {
		  get
		  {
			return maxNounLocation;
		  }
	  }

	  public virtual Parse NextTokenBasal
	  {
		  get
		  {
			return basalNextToken;
		  }
	  }

	  public virtual Parse PreviousToken
	  {
		  get
		  {
			return prevToken;
		  }
	  }

	  public virtual Parse NextToken
	  {
		  get
		  {
			return nextToken;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the sentence which contains this mention.
	  /// </summary>
	  /// <returns> the index of the sentence which contains this mention. </returns>
	  public virtual int SentenceNumber
	  {
		  get
		  {
			return sentenceNumber;
		  }
	  }

	  /// <summary>
	  /// Returns the parse for the first token in this mention.
	  /// </summary>
	  /// <returns> The parse for the first token in this mention. </returns>
	  public virtual Parse FirstToken
	  {
		  get
		  {
			return firstToken;
		  }
	  }

	  /// <summary>
	  /// Returns the text for the first token of the mention.
	  /// </summary>
	  /// <returns> The text for the first token of the mention. </returns>
	  public virtual string FirstTokenText
	  {
		  get
		  {
			return firstTokenText;
		  }
	  }

	  /// <summary>
	  /// Returns the pos-tag of the first token of this mention.
	  /// </summary>
	  /// <returns> the pos-tag of the first token of this mention. </returns>
	  public virtual string FirstTokenTag
	  {
		  get
		  {
			return firstTokenTag;
		  }
	  }

	  /// <summary>
	  /// Returns the parses for the tokens which are contained in this mention.
	  /// </summary>
	  /// <returns> An array of parses, in order, for each token contained in this mention. </returns>
	  public virtual Parse[] TokenParses
	  {
		  get
		  {
			return (Parse[]) tokens;
		  }
	  }

	  /// <summary>
	  /// Returns the text of this mention.
	  /// </summary>
	  /// <returns> A space-delimited string of the tokens of this mention. </returns>
	  public virtual string toText()
	  {
		return parse.ToString();
	  }

	  /*
	  private static String[] getLemmas(MentionContext xec) {
	    //TODO: Try multi-word lemmas first.
	    String word = xec.getHeadTokenText();
	    return DictionaryFactory.getDictionary().getLemmas(word,"NN");
	  }
	
	  private static Set getSynsetSet(MentionContext xec) {
	    //System.err.println("getting synsets for mention:"+xec.toText());
	    Set synsetSet = new HashSet();
	    String[] lemmas = getLemmas(xec);
	    for (int li = 0; li < lemmas.length; li++) {
	      String[] synsets = DictionaryFactory.getDictionary().getParentSenseKeys(lemmas[li],"NN",0);
	      for (int si=0,sn=synsets.length;si<sn;si++) {
	        synsetSet.add(synsets[si]);
	      }
	    }
	    return (synsetSet);
	  }
	  */

	  /// <summary>
	  /// Assigns the specified gender with the specified probability to this mention.
	  /// </summary>
	  /// <param name="gender"> The gender to be given to this mention. </param>
	  /// <param name="probability"> The probability associated with the gender assignment. </param>
	  public virtual void setGender(GenderEnum gender, double probability)
	  {
		this.gender = gender;
		this.genderProb = probability;
	  }

	  /// <summary>
	  /// Returns the gender of this mention.
	  /// </summary>
	  /// <returns> The gender of this mention. </returns>
	  public virtual GenderEnum Gender
	  {
		  get
		  {
			return gender;
		  }
	  }

	  /// <summary>
	  /// Returns the probability associated with the gender assignment.
	  /// </summary>
	  /// <returns> The probability associated with the gender assignment. </returns>
	  public virtual double GenderProb
	  {
		  get
		  {
			return genderProb;
		  }
	  }

	  /// <summary>
	  /// Assigns the specified number with the specified probability to this mention.
	  /// </summary>
	  /// <param name="number"> The number to be given to this mention. </param>
	  /// <param name="probability"> The probability associated with the number assignment. </param>
	  public virtual void setNumber(NumberEnum number, double probability)
	  {
		this.number = number;
		this.numberProb = probability;
	  }

	  /// <summary>
	  /// Returns the number of this mention.
	  /// </summary>
	  /// <returns> The number of this mention. </returns>
	  public virtual NumberEnum Number
	  {
		  get
		  {
			return number;
		  }
	  }

	  /// <summary>
	  /// Returns the probability associated with the number assignment.
	  /// </summary>
	  /// <returns> The probability associated with the number assignment. </returns>
	  public virtual double NumberProb
	  {
		  get
		  {
			return numberProb;
		  }
	  }
	}

}