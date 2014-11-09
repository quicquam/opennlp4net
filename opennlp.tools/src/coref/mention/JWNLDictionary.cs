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

namespace opennlp.tools.coref.mention
{


	using JWNLException = net.didion.jwnl.JWNLException;
	using Adjective = net.didion.jwnl.data.Adjective;
	using FileDictionaryElementFactory = net.didion.jwnl.data.FileDictionaryElementFactory;
	using IndexWord = net.didion.jwnl.data.IndexWord;
	using POS = net.didion.jwnl.data.POS;
	using Pointer = net.didion.jwnl.data.Pointer;
	using PointerType = net.didion.jwnl.data.PointerType;
	using Synset = net.didion.jwnl.data.Synset;
	using VerbFrame = net.didion.jwnl.data.VerbFrame;
	using FileBackedDictionary = net.didion.jwnl.dictionary.FileBackedDictionary;
	using MorphologicalProcessor = net.didion.jwnl.dictionary.MorphologicalProcessor;
	using FileManager = net.didion.jwnl.dictionary.file_manager.FileManager;
	using FileManagerImpl = net.didion.jwnl.dictionary.file_manager.FileManagerImpl;
	using DefaultMorphologicalProcessor = net.didion.jwnl.dictionary.morph.DefaultMorphologicalProcessor;
	using DetachSuffixesOperation = net.didion.jwnl.dictionary.morph.DetachSuffixesOperation;
	using LookupExceptionsOperation = net.didion.jwnl.dictionary.morph.LookupExceptionsOperation;
	using LookupIndexWordOperation = net.didion.jwnl.dictionary.morph.LookupIndexWordOperation;
	using Operation = net.didion.jwnl.dictionary.morph.Operation;
	using TokenizerOperation = net.didion.jwnl.dictionary.morph.TokenizerOperation;
	using PrincetonWN17FileDictionaryElementFactory = net.didion.jwnl.princeton.data.PrincetonWN17FileDictionaryElementFactory;
	using PrincetonRandomAccessDictionaryFile = net.didion.jwnl.princeton.file.PrincetonRandomAccessDictionaryFile;

	/// <summary>
	/// An implementation of the Dictionary interface using the JWNL library.
	/// </summary>
	public class JWNLDictionary : Dictionary
	{

	  private net.didion.jwnl.dictionary.Dictionary dict;
	  private MorphologicalProcessor morphy;
	  private static string[] empty = new string[0];

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JWNLDictionary(String searchDirectory) throws java.io.IOException, net.didion.jwnl.JWNLException
	  public JWNLDictionary(string searchDirectory)
	  {
		PointerType.initialize();
		Adjective.initialize();
		VerbFrame.initialize();
		IDictionary<POS, String[][]> suffixMap = new Dictionary<POS, String[][]>();
		suffixMap[POS.NOUN] = new string[][]
		{
			new string[] {"s",""},
			new string[] {"ses","s"},
			new string[] {"xes","x"},
			new string[] {"zes","z"},
			new string[] {"ches","ch"},
			new string[] {"shes","sh"},
			new string[] {"men","man"},
			new string[] {"ies","y"}
		};
		suffixMap[POS.VERB] = new string[][]
		{
			new string[] {"s",""},
			new string[] {"ies","y"},
			new string[] {"es","e"},
			new string[] {"es",""},
			new string[] {"ed","e"},
			new string[] {"ed",""},
			new string[] {"ing","e"},
			new string[] {"ing",""}
		};
		suffixMap[POS.ADJECTIVE] = new string[][]
		{
			new string[] {"er",""},
			new string[] {"est",""},
			new string[] {"er","e"},
			new string[] {"est","e"}
		};
		DetachSuffixesOperation tokDso = new DetachSuffixesOperation(suffixMap);
		tokDso.addDelegate(DetachSuffixesOperation.OPERATIONS,new Operation[] {new LookupIndexWordOperation(),new LookupExceptionsOperation()});
		TokenizerOperation tokOp = new TokenizerOperation(new string[] {" ","-"});
		tokOp.addDelegate(TokenizerOperation.TOKEN_OPERATIONS,new Operation[] {new LookupIndexWordOperation(),new LookupExceptionsOperation(),tokDso});
		DetachSuffixesOperation morphDso = new DetachSuffixesOperation(suffixMap);
		morphDso.addDelegate(DetachSuffixesOperation.OPERATIONS,new Operation[] {new LookupIndexWordOperation(),new LookupExceptionsOperation()});
		Operation[] operations = new Operation[] {new LookupExceptionsOperation(), morphDso, tokOp};
		morphy = new DefaultMorphologicalProcessor(operations);
		FileManager manager = new FileManagerImpl(searchDirectory,typeof(PrincetonRandomAccessDictionaryFile));
		FileDictionaryElementFactory factory = new PrincetonWN17FileDictionaryElementFactory();
		FileBackedDictionary.install(manager, morphy,factory,true);
		dict = net.didion.jwnl.dictionary.Dictionary.Instance;
		morphy = dict.MorphologicalProcessor;
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public String[] getLemmas(String word, String tag)
	  public virtual string[] getLemmas(string word, string tag)
	  {
		try
		{
		  POS pos;
		  if (tag.StartsWith("N", StringComparison.Ordinal) || tag.StartsWith("n", StringComparison.Ordinal))
		  {
			pos = POS.NOUN;
		  }
		  else if (tag.StartsWith("V", StringComparison.Ordinal) || tag.StartsWith("v", StringComparison.Ordinal))
		  {
			pos = POS.VERB;
		  }
		  else if (tag.StartsWith("J", StringComparison.Ordinal) || tag.StartsWith("a", StringComparison.Ordinal))
		  {
			pos = POS.ADJECTIVE;
		  }
		  else if (tag.StartsWith("R", StringComparison.Ordinal) || tag.StartsWith("r", StringComparison.Ordinal))
		  {
			pos = POS.ADVERB;
		  }
		  else
		  {
			pos = POS.NOUN;
		  }
		  IList<string> lemmas = morphy.lookupAllBaseForms(pos,word);
		  return lemmas.ToArray();
		}
		catch (JWNLException e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		  return null;
		}
	  }

	  public virtual string getSenseKey(string lemma, string pos, int sense)
	  {
		try
		{
		  IndexWord iw = dict.getIndexWord(POS.NOUN,lemma);
		  if (iw == null)
		  {
			return null;
		  }
		  return Convert.ToString(iw.SynsetOffsets[sense]);
		}
		catch (JWNLException e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		  return null;
		}

	  }

	  public virtual int getNumSenses(string lemma, string pos)
	  {
		try
		{
		  IndexWord iw = dict.getIndexWord(POS.NOUN,lemma);
		  if (iw == null)
		  {
			return 0;
		  }
		  return iw.SenseCount;
		}
		catch (JWNLException)
		{
		  return 0;
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void getParents(net.didion.jwnl.data.Synset synset, java.util.List<String> parents) throws net.didion.jwnl.JWNLException
	  private void getParents(Synset synset, IList<string> parents)
	  {
		Pointer[] pointers = synset.Pointers;
		for (int pi = 0,pn = pointers.Length;pi < pn;pi++)
		{
		  if (pointers[pi].Type == PointerType.HYPERNYM)
		  {
			Synset parent = pointers[pi].TargetSynset;
			parents.Add(Convert.ToString(parent.Offset));
			getParents(parent,parents);
		  }
		}
	  }

	  public virtual string[] getParentSenseKeys(string lemma, string pos, int sense)
	  {
		//System.err.println("JWNLDictionary.getParentSenseKeys: lemma="+lemma);
		try
		{
		  IndexWord iw = dict.getIndexWord(POS.NOUN,lemma);
		  if (iw != null)
		  {
			Synset synset = iw.getSense(sense+1);
			IList<string> parents = new List<string>();
			getParents(synset,parents);
			return parents.ToArray();
		  }
		  else
		  {
			return empty;
		  }
		}
		catch (JWNLException e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		  return null;
		}
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException, net.didion.jwnl.JWNLException
	  public static void Main(string[] args)
	  {
		string searchDir = System.getProperty("WNSEARCHDIR");
		Console.Error.WriteLine("searchDir=" + searchDir);
		if (searchDir != null)
		{
		  Dictionary dict = new JWNLDictionary(System.getProperty("WNSEARCHDIR"));
		  string word = args[0];
		  string[] lemmas = dict.getLemmas(word,"NN");
		  for (int li = 0,ln = lemmas.Length;li < ln;li++)
		  {
			for (int si = 0,sn = dict.getNumSenses(lemmas[li],"NN");si < sn;si++)
			{
			  Console.WriteLine(lemmas[li] + " (" + si + ")\t" + dict.getParentSenseKeys(lemmas[li],"NN",si));
			}
		  }
		}
	  }
	}

}