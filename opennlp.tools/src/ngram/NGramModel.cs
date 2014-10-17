using System;
using System.Collections;
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
using System.IO;
using j4n.Exceptions;
using j4n.Interfaces;
using j4n.IO.InputStream;


namespace opennlp.tools.ngram
{


	using Dictionary = opennlp.tools.dictionary.Dictionary;
	using Attributes = opennlp.tools.dictionary.serializer.Attributes;
	using DictionarySerializer = opennlp.tools.dictionary.serializer.DictionarySerializer;
	using Entry = opennlp.tools.dictionary.serializer.Entry;
	using EntryInserter = opennlp.tools.dictionary.serializer.EntryInserter;
	using InvalidFormatException = opennlp.tools.util.InvalidFormatException;
	using StringList = opennlp.tools.util.StringList;

	/// <summary>
	/// The <seealso cref="NGramModel"/> can be used to crate ngrams and character ngrams.
	/// </summary>
	/// <seealso cref= StringList </seealso>
	public class NGramModel : IEnumerable<StringList>
	{

	  protected internal const string COUNT = "count";

	  private IDictionary<StringList, int?> mNGrams = new Dictionary<StringList, int?>();

	  /// <summary>
	  /// Initializes an empty instance.
	  /// </summary>
	  public NGramModel()
	  {
	  }

	  /// <summary>
	  /// Initializes the current instance.
	  /// </summary>
	  /// <param name="in"> </param>
	  /// <exception cref="IOException"> </exception>
	  /// <exception cref="InvalidFormatException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NGramModel(java.io.InputStream in) throws java.io.IOException, opennlp.tools.util.InvalidFormatException
	  public NGramModel(InputStream @in)
	  {
          DictionarySerializer.create(@in, new EntryInserterAnonymousInnerClassHelper(this));
	  }

	  private class EntryInserterAnonymousInnerClassHelper : EntryInserter
	  {
		  private readonly NGramModel outerInstance;

		  public EntryInserterAnonymousInnerClassHelper(NGramModel outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void insert(opennlp.tools.dictionary.serializer.Entry entry) throws opennlp.tools.util.InvalidFormatException
		  public virtual void insert(Entry entry)
		  {

			int count;
			string countValueString = null;

			try
			{
			  countValueString = entry.Attributes.getValue(COUNT);

			  if (countValueString == null)
			  {
				  throw new InvalidFormatException("The count attribute must be set!");
			  }

			  count = Convert.ToInt32(countValueString);
			}
			catch (NumberFormatException e)
			{
			  throw new InvalidFormatException("The count attribute '" + countValueString + "' must be a number!", e);
			}

			outerInstance.add(entry.Tokens);
			outerInstance.setCount(entry.Tokens, count);
		  }
	  }

	  /// <summary>
	  /// Retrieves the count of the given ngram.
	  /// </summary>
	  /// <param name="ngram">
	  /// </param>
	  /// <returns> count of the ngram or 0 if it is not contained
	  ///  </returns>
	  public virtual int getCount(StringList ngram)
	  {

		int? count = mNGrams[ngram];

		if (count == null)
		{
		  return 0;
		}

		return count.Value;
	  }

	  /// <summary>
	  /// Sets the count of an existing ngram.
	  /// </summary>
	  /// <param name="ngram"> </param>
	  /// <param name="count"> </param>
	  public virtual void setCount(StringList ngram, int count)
	  {

		int? oldCount = mNGrams[ngram] = count;

		if (oldCount == null)
		{
		  mNGrams.Remove(ngram);
		  throw new NoSuchElementException("");
		}
	  }

	  /// <summary>
	  /// Adds one NGram, if it already exists the count increase by one.
	  /// </summary>
	  /// <param name="ngram"> </param>
	  public virtual void add(StringList ngram)
	  {
		if (contains(ngram))
		{
		  setCount(ngram, getCount(ngram) + 1);
		}
		else
		{
		  mNGrams[ngram] = 1;
		}
	  }

	  /// <summary>
	  /// Adds NGrams up to the specified length to the current instance.
	  /// </summary>
	  /// <param name="ngram"> the tokens to build the uni-grams, bi-grams, tri-grams, ..
	  ///     from. </param>
	  /// <param name="minLength"> - minimal length </param>
	  /// <param name="maxLength"> - maximal length </param>
	  public virtual void add(StringList ngram, int minLength, int maxLength)
	  {

		if (minLength < 1 || maxLength < 1)
		{
			throw new System.ArgumentException("minLength and maxLength param must be at least 1. " + "minLength=" + minLength + ", maxLength= " + maxLength);
		}

		if (minLength > maxLength)
		{
			throw new System.ArgumentException("minLength param must not be larger than " + "maxLength param. minLength=" + minLength + ", maxLength= " + maxLength);
		}

		for (int lengthIndex = minLength; lengthIndex < maxLength + 1; lengthIndex++)
		{
		  for (int textIndex = 0; textIndex + lengthIndex - 1 < ngram.size(); textIndex++)
		  {

			string[] grams = new string[lengthIndex];

			for (int i = textIndex; i < textIndex + lengthIndex; i++)
			{
			  grams[i - textIndex] = ngram.getToken(i);
			}

			add(new StringList(grams));
		  }
		}
	  }

	  /// <summary>
	  /// Adds character NGrams to the current instance.
	  /// </summary>
	  /// <param name="chars"> </param>
	  /// <param name="minLength"> </param>
	  /// <param name="maxLength"> </param>
	  public virtual void add(string chars, int minLength, int maxLength)
	  {

		for (int lengthIndex = minLength; lengthIndex < maxLength + 1; lengthIndex++)
		{
		  for (int textIndex = 0; textIndex + lengthIndex - 1 < chars.Length; textIndex++)
		  {

			string gram = chars.Substring(textIndex, lengthIndex).ToLower();

			add(new StringList(new string[]{gram}));
		  }
		}
	  }

	  /// <summary>
	  /// Removes the specified tokens form the NGram model, they are just dropped.
	  /// </summary>
	  /// <param name="tokens"> </param>
	  public virtual void remove(StringList tokens)
	  {
		mNGrams.Remove(tokens);
	  }

	  /// <summary>
	  /// Checks fit he given tokens are contained by the current instance.
	  /// </summary>
	  /// <param name="tokens">
	  /// </param>
	  /// <returns> true if the ngram is contained </returns>
	  public virtual bool contains(StringList tokens)
	  {
		return mNGrams.ContainsKey(tokens);
	  }

	  /// <summary>
	  /// Retrieves the number of <seealso cref="StringList"/> entries in the current instance.
	  /// </summary>
	  /// <returns> number of different grams </returns>
	  public virtual int size()
	  {
		return mNGrams.Count;
	  }

	  /// <summary>
	  /// Retrieves an <seealso cref="Iterator"/> over all <seealso cref="StringList"/> entries.
	  /// </summary>
	  /// <returns> iterator over all grams </returns>
	  public virtual IEnumerator<StringList> GetEnumerator()
	  {
		return mNGrams.Keys.GetEnumerator();
	  }

	  /// <summary>
	  /// Retrieves the total count of all Ngrams.
	  /// </summary>
	  /// <returns> total count of all ngrams </returns>
	  public virtual int numberOfGrams()
	  {
		int counter = 0;

		foreach (StringList ngram in this)
		{
		  counter += getCount(ngram);
		}

		return counter;
	  }

	  /// <summary>
	  /// Deletes all ngram which do appear less than the cutoffUnder value
	  /// and more often than the cutoffOver value.
	  /// </summary>
	  /// <param name="cutoffUnder"> </param>
	  /// <param name="cutoffOver"> </param>
	  public virtual void cutoff(int cutoffUnder, int cutoffOver)
	  {

		if (cutoffUnder > 0 || cutoffOver < int.MaxValue)
		{

		  for (IEnumerator<StringList> it = GetEnumerator(); it.MoveNext();)
		  {

			StringList ngram = it.Current;

			int count = getCount(ngram);

			if (count < cutoffUnder || count > cutoffOver)
			{
			  remove(ngram);
			}
		  }
		}
	  }

	  /// <summary>
	  /// Creates a dictionary which contain all <seealso cref="StringList"/> which
	  /// are in the current <seealso cref="NGramModel"/>.
	  /// 
	  /// Entries which are only different in the case are merged into one.
	  /// 
	  /// Calling this method is the same as calling <seealso cref="#toDictionary(boolean)"/> with true.
	  /// </summary>
	  /// <returns> a dictionary of the ngrams </returns>
	  public virtual Dictionary toDictionary()
	  {
		return toDictionary(false);
	  }

	  /// <summary>
	  /// Creates a dictionary which contains all <seealso cref="StringList"/>s which
	  /// are in the current <seealso cref="NGramModel"/>.
	  /// </summary>
	  /// <param name="caseSensitive"> Specifies whether case distinctions should be kept in the creation of the dictionary.
	  /// </param>
	  /// <returns> a dictionary of the ngrams </returns>
	  public virtual Dictionary toDictionary(bool caseSensitive)
	  {

		Dictionary dict = new Dictionary(caseSensitive);

		foreach (StringList stringList in this)
		{
		  dict.put(stringList);
		}

		return dict;
	  }

	  /// <summary>
	  /// Writes the ngram instance to the given <seealso cref="OutputStream"/>.
	  /// </summary>
	  /// <param name="out">
	  /// </param>
	  /// <exception cref="IOException"> if an I/O Error during writing occurs </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void serialize(java.io.OutputStream out) throws java.io.IOException
	  public virtual void serialize(OutputStream @out)
	  {
			IEnumerator<Entry> entryIterator = new IteratorAnonymousInnerClassHelper(this);

			DictionarySerializer.serialize(@out, entryIterator, false);
	  }

	  private class IteratorAnonymousInnerClassHelper : IEnumerator<Entry>
	  {
		  private readonly NGramModel outerInstance;

		  public IteratorAnonymousInnerClassHelper(NGramModel outerInstance)
		  {
			  this.outerInstance = outerInstance;
			  mDictionaryIterator = outerInstance.GetEnumerator();
		  }

		  private IEnumerator<StringList> mDictionaryIterator;

		  public virtual bool hasNext()
		  {
			return mDictionaryIterator.hasNext();
		  }

		  public virtual Entry next()
		  {

			StringList tokens = mDictionaryIterator.next();

			Attributes attributes = new Attributes();

			attributes.setValue(COUNT, Convert.ToString(outerInstance.getCount(tokens)));

			return new Entry(tokens, attributes);
		  }

		  public virtual void remove()
		  {
			throw new System.NotSupportedException();
		  }

	      public void Dispose()
	      {
	          throw new NotImplementedException();
	      }

	      public bool MoveNext()
	      {
	          throw new NotImplementedException();
	      }

	      public void Reset()
	      {
	          throw new NotImplementedException();
	      }

	      public Entry Current { get; private set; }

	      object IEnumerator.Current
	      {
	          get { return Current; }
	      }
	  }

	  public override bool Equals(object obj)
	  {
		bool result;

		if (obj == this)
		{
		  result = true;
		}
		else if (obj is NGramModel)
		{
		  NGramModel model = (NGramModel) obj;

		  result = mNGrams.Equals(model.mNGrams);
		}
		else
		{
		  result = false;
		}

		return result;
	  }

	  public override string ToString()
	  {
		return "Size: " + size();
	  }

	  public override int GetHashCode()
	  {
		return mNGrams.GetHashCode();
	  }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }
	}

}