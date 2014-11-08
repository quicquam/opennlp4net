using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using j4n.Interfaces;
using j4n.IO.InputStream;
using j4n.IO.OutputStream;
using j4n.IO.Reader;
using opennlp.nonjava.helperclasses;
using UncloseableInputStream = opennlp.tools.util.model.UncloseableInputStream;


namespace opennlp.tools.postag
{


	using Attributes = opennlp.tools.dictionary.serializer.Attributes;
	using DictionarySerializer = opennlp.tools.dictionary.serializer.DictionarySerializer;
	using Entry = opennlp.tools.dictionary.serializer.Entry;
	using EntryInserter = opennlp.tools.dictionary.serializer.EntryInserter;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using StringList = opennlp.tools.util.StringList;
	using StringUtil = opennlp.tools.util.StringUtil;

	/// <summary>
	/// Provides a means of determining which tags are valid for a particular word
	/// based on a tag dictionary read from a file.
	/// </summary>
	public class POSDictionary : IEnumerable<string>, MutableTagDictionary
	{

	  private IDictionary<string, String[]> dictionary;

	  private bool caseSensitive = true;

	  /// <summary>
	  /// Initializes an empty case sensitive <seealso cref="POSDictionary"/>.
	  /// </summary>
	  public POSDictionary() : this(true)
	  {
	  }

	  /// <summary>
	  /// Initializes an empty <seealso cref="POSDictionary"/>. </summary>
	  /// <param name="caseSensitive"> the <seealso cref="POSDictionary"/> case sensitivity </param>
	  public POSDictionary(bool caseSensitive)
	  {
		dictionary = new Dictionary<string, String[]>();
		this.caseSensitive = caseSensitive;
	  }

	  /// <summary>
	  /// Creates a tag dictionary with contents of specified file.
	  /// </summary>
	  /// <param name="file"> The file name for the tag dictionary.
	  /// </param>
	  /// <exception cref="IOException"> when the specified file can not be read.
	  /// </exception>
	  /// @deprecated Use <seealso cref="POSDictionary#create(InputStream)"/> instead, old format might removed. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("Use <seealso cref="POSDictionary#create(java.io.InputStream)"/> instead, old format might removed.") public POSDictionary(String file) throws java.io.IOException
	  [Obsolete("Use <seealso cref=\"POSDictionary#create(java.io.InputStream)\"/> instead, old format might removed.")]
	  public POSDictionary(string file) : this(file, null, true)
	  {
	  }

	  /// <summary>
	  /// Creates a tag dictionary with contents of specified file and using specified
	  /// case to determine how to access entries in the tag dictionary.
	  /// </summary>
	  /// <param name="file"> The file name for the tag dictionary. </param>
	  /// <param name="caseSensitive"> Specifies whether the tag dictionary is case sensitive or not.
	  /// </param>
	  /// <exception cref="IOException"> when the specified file can not be read.
	  /// </exception>
	  /// @deprecated Use <seealso cref="POSDictionary#create(InputStream)"/> instead, old format might removed. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("Use <seealso cref="POSDictionary#create(java.io.InputStream)"/> instead, old format might removed.") public POSDictionary(String file, boolean caseSensitive) throws java.io.IOException
	  [Obsolete("Use <seealso cref=\"POSDictionary#create(java.io.InputStream)\"/> instead, old format might removed.")]
	  public POSDictionary(string file, bool caseSensitive) : this(file, null, caseSensitive)
	  {
	  }


	  /// <summary>
	  /// Creates a tag dictionary with contents of specified file and using specified case to determine how to access entries in the tag dictionary.
	  /// </summary>
	  /// <param name="file"> The file name for the tag dictionary. </param>
	  /// <param name="encoding"> The encoding of the tag dictionary file. </param>
	  /// <param name="caseSensitive"> Specifies whether the tag dictionary is case sensitive or not.
	  /// </param>
	  /// <exception cref="IOException"> when the specified file can not be read.
	  /// </exception>
	  /// @deprecated Use <seealso cref="POSDictionary#create(InputStream)"/> instead, old format might removed. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("Use <seealso cref="POSDictionary#create(java.io.InputStream)"/> instead, old format might removed.") public POSDictionary(String file, String encoding, boolean caseSensitive) throws java.io.IOException
	  [Obsolete("Use <seealso cref=\"POSDictionary#create(java.io.InputStream)\"/> instead, old format might removed.")]
	  public POSDictionary(string file, string encoding, bool caseSensitive) : this(new BufferedReader(encoding == null ? new FileReader(file) : new InputStreamReader(new FileInputStream(file),encoding)), caseSensitive)
	  {
	  }

	  /// <summary>
	  /// Create tag dictionary object with contents of specified file and using specified case to determine how to access entries in the tag dictionary.
	  /// </summary>
	  /// <param name="reader"> A reader for the tag dictionary. </param>
	  /// <param name="caseSensitive"> Specifies whether the tag dictionary is case sensitive or not.
	  /// </param>
	  /// <exception cref="IOException"> when the specified file can not be read.
	  /// </exception>
	  /// @deprecated Use <seealso cref="POSDictionary#create(InputStream)"/> instead, old format might removed. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Deprecated("Use <seealso cref="POSDictionary#create(java.io.InputStream)"/> instead, old format might removed.") public POSDictionary(java.io.BufferedReader reader, boolean caseSensitive) throws java.io.IOException
	  [Obsolete("Use <seealso cref=\"POSDictionary#create(java.io.InputStream)\"/> instead, old format might removed.")]
	  public POSDictionary(BufferedReader reader, bool caseSensitive)
	  {
		dictionary = new Dictionary<string, String[]>();
		this.caseSensitive = caseSensitive;
		for (string line = reader.readLine(); line != null; line = reader.readLine())
		{
		  string[] parts = line.Split(' ');
		  string[] tags = new string[parts.Length - 1];
		  for (int ti = 0, tl = parts.Length - 1; ti < tl; ti++)
		  {
			tags[ti] = parts[ti + 1];
		  }
		  if (caseSensitive)
		  {
			dictionary[parts[0]] = tags;
		  }
		  else
		  {
			dictionary[StringUtil.ToLower(parts[0])] = tags;
		  }
		}
	  }

	  /// <summary>
	  /// Returns a list of valid tags for the specified word.
	  /// </summary>
	  /// <param name="word"> The word.
	  /// </param>
	  /// <returns> A list of valid tags for the specified word or
	  /// null if no information is available for that word. </returns>
	  public virtual string[] getTags(string word)
	  {
		if (caseSensitive)
		{
		  return dictionary[word];
		}
		else
		{
		  return dictionary[word.ToLower()];
		}
	  }

	  /// <summary>
	  /// Associates the specified tags with the specified word. If the dictionary
	  /// previously contained the word, the old tags are replaced by the specified
	  /// ones.
	  /// </summary>
	  /// <param name="word">
	  ///          The word to be added to the dictionary. </param>
	  /// <param name="tags">
	  ///          The set of tags associated with the specified word.
	  /// </param>
	  /// @deprecated Use <seealso cref="#put(String, String[])"/> instead 
	  internal virtual void addTags(string word, params string[] tags)
	  {
		put(word, tags);
	  }

	  /// <summary>
	  /// Retrieves an iterator over all words in the dictionary.
	  /// </summary>
	  public virtual IEnumerator<string> GetEnumerator()
	  {
		return dictionary.Keys.GetEnumerator();
	  }

	  private static string tagsToString(string[] tags)
	  {

		StringBuilder tagString = new StringBuilder();

		foreach (string tag in tags)
		{
		  tagString.Append(tag);
		  tagString.Append(' ');
		}

		// remove last space
		if (tagString.Length > 0)
		{
		  tagString.Length = tagString.Length - 1;
		}

		return tagString.ToString();
	  }

	  /// <summary>
	  /// Writes the <seealso cref="POSDictionary"/> to the given <seealso cref="OutputStream"/>;
	  /// 
	  /// After the serialization is finished the provided
	  /// <seealso cref="OutputStream"/> remains open.
	  /// </summary>
	  /// <param name="out">
	  ///            the <seealso cref="OutputStream"/> to write the dictionary into.
	  /// </param>
	  /// <exception cref="IOException">
	  ///             if writing to the <seealso cref="OutputStream"/> fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void serialize(java.io.OutputStream out) throws java.io.IOException
	  public virtual void serialize(OutputStream @out)
	  {
          // Commented out 07/11/2014 MJJ

          throw new NotImplementedException();
	/*	IEnumerator<Entry> entries = new IteratorAnonymousInnerClassHelper(this);

		DictionarySerializer.serialize(@out, entries, caseSensitive); */
	  }
/*
	  private class IteratorAnonymousInnerClassHelper : IEnumerator<Entry>
	  {
		  private readonly POSDictionary outerInstance;

		  public IteratorAnonymousInnerClassHelper(POSDictionary outerInstance)
		  {
			  this.outerInstance = outerInstance;
			  iterator = outerInstance.dictionary.Keys.GetEnumerator();
		  }


		  internal IEnumerator<string> iterator;

		  public virtual bool hasNext()
		  {
			return iterator.hasNext();
		  }

		  public virtual Entry next()
		  {

			string word = iterator.next();

			Attributes tagAttribute = new Attributes();
			tagAttribute.setValue("tags", tagsToString(outerInstance.getTags(word)));

			return new Entry(new StringList(word), tagAttribute);
		  }

		  public virtual void remove()
		  {
			throw new System.NotSupportedException();
		  }
	  }
*/
	  public override bool Equals(object o)
	  {

		if (o == this)
		{
		  return true;
		}
		else if (o is POSDictionary)
		{
		  POSDictionary dictionary = (POSDictionary) o;

		  if (this.dictionary.Count == dictionary.dictionary.Count)
		  {

			foreach (string word in this)
			{

			  string[] aTags = getTags(word);
			  string[] bTags = dictionary.getTags(word);

			  if (!Equals(aTags, bTags))
			  {
				return false;
			  }
			}

			return true;
		  }
		}

		return false;
	  }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }

	    public override string ToString()
	  {
		// it is time consuming to output the dictionary entries.
		// will output something meaningful for debugging, like
		// POSDictionary{size=100, caseSensitive=true}

		return "POSDictionary{size=" + dictionary.Count + ", caseSensitive=" + this.caseSensitive + "}";
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="POSDictionary"/> from a provided <seealso cref="InputStream"/>.
	  /// 
	  /// After creation is finished the provided <seealso cref="InputStream"/> is closed.
	  /// </summary>
	  /// <param name="in">
	  /// </param>
	  /// <returns> the pos dictionary
	  /// </returns>
	  /// <exception cref="IOException"> </exception>
	  /// <exception cref="InvalidFormatException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static POSDictionary create(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
	  public static POSDictionary create(InputStream @in)
	  {

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final POSDictionary newPosDict = new POSDictionary();
		POSDictionary newPosDict = new POSDictionary();

	      bool isCaseSensitive = true;
          // 08/11/2014 MJJ Commented this out temporarily
          // was isCaseSensitive = DictionarySerializer.create(@in, new EntryInserterAnonymousInnerClassHelper(newPosDict));

		newPosDict.caseSensitive = isCaseSensitive;

		// TODO: The dictionary API needs to be improved to do this better!
		if (!isCaseSensitive)
		{
		  IDictionary<string, String[]> lowerCasedDictionary = new Dictionary<string, String[]>();

		  foreach (KeyValuePair<string, String[]> entry in newPosDict.dictionary)
		  {
			lowerCasedDictionary[StringUtil.ToLower(entry.Key)] = entry.Value;
		  }

		  newPosDict.dictionary = lowerCasedDictionary;
		}

		return newPosDict;
	  }

	  private class EntryInserterAnonymousInnerClassHelper : EntryInserter
	  {
		  private opennlp.tools.postag.POSDictionary newPosDict;

		  public EntryInserterAnonymousInnerClassHelper(opennlp.tools.postag.POSDictionary newPosDict)
		  {
			  this.newPosDict = newPosDict;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void insert(opennlp.tools.dictionary.serializer.Entry entry) throws opennlp.tools.util.InvalidFormatException
		  public virtual void insert(Entry entry)
		  {

			string tagString = entry.Attributes.getValue("tags");

			string[] tags = tagString.Split(' ');

			StringList word = entry.Tokens;

			if (word.size() != 1)
			{
			  throw new InvalidFormatException("Each entry must have exactly one token! " + word);
			}

			newPosDict.dictionary[word.getToken(0)] = tags;
		  }
	  }

	  public virtual string[] put(string word, params string[] tags)
	  {
		if (this.caseSensitive)
		{
		  return dictionary[word] = tags;
		}
		else
		{
		  return dictionary[word.ToLower()] = tags;
		}
	  }

	  public virtual bool CaseSensitive
	  {
		  get
		  {
			return this.caseSensitive;
		  }
	  }

	    public static POSDictionary create(UncloseableInputStream @in)
	    {
	        throw new NotImplementedException();
	    }
	}

}