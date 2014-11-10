﻿using System;
using j4n.Lang;

namespace opennlp.tools.stemmer
{

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

	/*
	
	   Porter stemmer in Java. The original paper is in
	
	       Porter, 1980, An algorithm for suffix stripping, Program, Vol. 14,
	       no. 3, pp 130-137,
	
	   See also http://www.tartarus.org/~martin/PorterStemmer/index.html
	
	   Bug 1 (reported by Gonzalo Parra 16/10/99) fixed as marked below.
	   Tthe words 'aed', 'eed', 'oed' leave k at 'a' for step 3, and b[k-1]
	   is then out outside the bounds of b.
	
	   Similarly,
	
	   Bug 2 (reported by Steve Dyrdahl 22/2/00) fixed as marked below.
	   'ion' by itself leaves j = -1 in the test for 'ion' in step 5, and
	   b[j] is then outside the bounds of b.
	
	   Release 3.
	
	   [ This version is derived from Release 3, modified by Brian Goetz to
	     optimize for fewer object creations.  ]
	
	*/

	/// 
	/// <summary>
	/// Stemmer, implementing the Porter Stemming Algorithm
	/// 
	/// The Stemmer class transforms a word into its root form.  The input
	/// word can be provided a character at time (by calling add()), or at once
	/// by calling one of the various stem(something) methods.
	/// </summary>

	internal class PorterStemmer : Stemmer
	{
	  private char[] b;
	  private int i, j, k, k0; // offset into b
	  private bool dirty = false;
	  private const int INC = 50;

	  public PorterStemmer()
	  {
		b = new char[INC];
		i = 0;
	  }

	  /// <summary>
	  /// reset() resets the stemmer so it can stem another word.  If you invoke
	  /// the stemmer by calling add(char) and then stem(), you must call reset()
	  /// before starting another word.
	  /// </summary>
	  public virtual void reset()
	  {
		  i = 0;
		  dirty = false;
	  }

	  /// <summary>
	  /// Add a character to the word being stemmed.  When you are finished
	  /// adding characters, you can call stem(void) to process the word.
	  /// </summary>
	  public virtual void add(char ch)
	  {
		if (b.Length == i)
		{

		  char[] new_b = new char[i + INC];
		  for (int c = 0; c < i; c++)
		  {
			  new_b[c] = b[c];
		  }
		  {
			b = new_b;
		  }
		}
		b[i++] = ch;
	  }

	  /// <summary>
	  /// After a word has been stemmed, it can be retrieved by toString(),
	  /// or a reference to the internal buffer can be retrieved by getResultBuffer
	  /// and getResultLength (which is generally more efficient.)
	  /// </summary>
	  public override string ToString()
	  {
		  return new string(b,0,i);
	  }

	  /// <summary>
	  /// Returns the length of the word resulting from the stemming process.
	  /// </summary>
	  public virtual int ResultLength
	  {
		  get
		  {
			  return i;
		  }
	  }

	  /// <summary>
	  /// Returns a reference to a character buffer containing the results of
	  /// the stemming process.  You also need to consult getResultLength()
	  /// to determine the length of the result.
	  /// </summary>
	  public virtual char[] ResultBuffer
	  {
		  get
		  {
			  return b;
		  }
	  }

	  /* cons(i) is true <=> b[i] is a consonant. */

	  private bool cons(int i)
	  {
		switch (b[i])
		{
		case 'a':
	case 'e':
	case 'i':
	case 'o':
	case 'u':
		  return false;
		case 'y':
		  return (i == k0) ? true :!cons(i - 1);
		default:
		  return true;
		}
	  }

	  /* m() measures the number of consonant sequences between k0 and j. if c is
	     a consonant sequence and v a vowel sequence, and <..> indicates arbitrary
	     presence,
	
	          <c><v>       gives 0
	          <c>vc<v>     gives 1
	          <c>vcvc<v>   gives 2
	          <c>vcvcvc<v> gives 3
	          ....
	  */

	  private int m()
	  {
		int n = 0;
		int i = k0;
		while (true)
		{
		  if (i > j)
		  {
			return n;
		  }
		  if (!cons(i))
		  {
			break;
		  }
		  i++;
		}
		i++;
		while (true)
		{
		  while (true)
		  {
			if (i > j)
			{
			  return n;
			}
			if (cons(i))
			{
			  break;
			}
			i++;
		  }
		  i++;
		  n++;
		  while (true)
		  {
			if (i > j)
			{
			  return n;
			}
			if (!cons(i))
			{
			  break;
			}
			i++;
		  }
		  i++;
		}
	  }

	  /* vowelinstem() is true <=> k0,...j contains a vowel */

	  private bool vowelinstem()
	  {
		int i;
		for (i = k0; i <= j; i++)
		{
		  if (!cons(i))
		  {
			return true;
		  }
		}
		return false;
	  }

	  /* doublec(j) is true <=> j,(j-1) contain a double consonant. */

	  private bool doublec(int j)
	  {
		if (j < k0 + 1)
		{
		  return false;
		}
		if (b[j] != b[j - 1])
		{
		  return false;
		}
		return cons(j);
	  }

	  /* cvc(i) is true <=> i-2,i-1,i has the form consonant - vowel - consonant
	     and also if the second c is not w,x or y. this is used when trying to
	     restore an e at the end of a short word. e.g.
	
	          cav(e), lov(e), hop(e), crim(e), but
	          snow, box, tray.
	
	  */

	  private bool cvc(int i)
	  {
		if (i < k0 + 2 || !cons(i) || cons(i - 1) || !cons(i - 2))
		{
		  return false;
		}
		else
		{
		  int ch = b[i];
		  if (ch == 'w' || ch == 'x' || ch == 'y')
		  {
			  return false;
		  }
		}
		return true;
	  }

	  private bool ends(string s)
	  {
		int l = s.Length;
		int o = k - l + 1;
		if (o < k0)
		{
		  return false;
		}
		for (int i = 0; i < l; i++)
		{
		  if (b[o + i] != s[i])
		  {
			return false;
		  }
		}
		j = k - l;
		return true;
	  }

	  /* setto(s) sets (j+1),...k to the characters in the string s, readjusting
	     k. */

	  internal virtual void setto(string s)
	  {
		int l = s.Length;
		int o = j + 1;
		for (int i = 0; i < l; i++)
		{
		  b[o + i] = s[i];
		}
		k = j + l;
		dirty = true;
	  }

	  /* r(s) is used further down. */

	  internal virtual void r(string s)
	  {
		  if (m() > 0)
		  {
			  setto(s);
		  }
	  }

	  /* step1() gets rid of plurals and -ed or -ing. e.g.
	
	           caresses  ->  caress
	           ponies    ->  poni
	           ties      ->  ti
	           caress    ->  caress
	           cats      ->  cat
	
	           feed      ->  feed
	           agreed    ->  agree
	           disabled  ->  disable
	
	           matting   ->  mat
	           mating    ->  mate
	           meeting   ->  meet
	           milling   ->  mill
	           messing   ->  mess
	
	           meetings  ->  meet
	
	  */

	  private void step1()
	  {
		if (b[k] == 's')
		{
		  if (ends("sses"))
		  {
			  k -= 2;
		  }
		  else if (ends("ies"))
		  {
			  setto("i");
		  }
		  else if (b[k - 1] != 's')
		  {
			  k--;
		  }
		}
		if (ends("eed"))
		{
		  if (m() > 0)
		  {
			k--;
		  }
		}
		else if ((ends("ed") || ends("ing")) && vowelinstem())
		{
		  k = j;
		  if (ends("at"))
		  {
			  setto("ate");
		  }
		  else if (ends("bl"))
		  {
			  setto("ble");
		  }
		  else if (ends("iz"))
		  {
			  setto("ize");
		  }
		  else if (doublec(k))
		  {
			int ch = b[k--];
			if (ch == 'l' || ch == 's' || ch == 'z')
			{
			  k++;
			}
		  }
		  else if (m() == 1 && cvc(k))
		  {
			setto("e");
		  }
		}
	  }

	  /* step2() turns terminal y to i when there is another vowel in the stem. */

	  private void step2()
	  {
		if (ends("y") && vowelinstem())
		{
		  b[k] = 'i';
		  dirty = true;
		}
	  }

	  /* step3() maps double suffices to single ones. so -ization ( = -ize plus
	     -ation) maps to -ize etc. note that the string before the suffix must give
	     m() > 0. */

	  private void step3()
	  {
		if (k == k0) // For Bug 1
		{
			return;
		}
		switch (b[k - 1])
		{
		case 'a':
		  if (ends("ational"))
		  {
			  r("ate");
			  break;
		  }
		  if (ends("tional"))
		  {
			  r("tion");
			  break;
		  }
		  break;
		case 'c':
		  if (ends("enci"))
		  {
			  r("ence");
			  break;
		  }
		  if (ends("anci"))
		  {
			  r("ance");
			  break;
		  }
		  break;
		case 'e':
		  if (ends("izer"))
		  {
			  r("ize");
			  break;
		  }
		  break;
		case 'l':
		  if (ends("bli"))
		  {
			  r("ble");
			  break;
		  }
		  if (ends("alli"))
		  {
			  r("al");
			  break;
		  }
		  if (ends("entli"))
		  {
			  r("ent");
			  break;
		  }
		  if (ends("eli"))
		  {
			  r("e");
			  break;
		  }
		  if (ends("ousli"))
		  {
			  r("ous");
			  break;
		  }
		  break;
		case 'o':
		  if (ends("ization"))
		  {
			  r("ize");
			  break;
		  }
		  if (ends("ation"))
		  {
			  r("ate");
			  break;
		  }
		  if (ends("ator"))
		  {
			  r("ate");
			  break;
		  }
		  break;
		case 's':
		  if (ends("alism"))
		  {
			  r("al");
			  break;
		  }
		  if (ends("iveness"))
		  {
			  r("ive");
			  break;
		  }
		  if (ends("fulness"))
		  {
			  r("ful");
			  break;
		  }
		  if (ends("ousness"))
		  {
			  r("ous");
			  break;
		  }
		  break;
		case 't':
		  if (ends("aliti"))
		  {
			  r("al");
			  break;
		  }
		  if (ends("iviti"))
		  {
			  r("ive");
			  break;
		  }
		  if (ends("biliti"))
		  {
			  r("ble");
			  break;
		  }
		  break;
		case 'g':
		  if (ends("logi"))
		  {
			  r("log");
			  break;
		  }
          break;
		}
	  }

	  /* step4() deals with -ic-, -full, -ness etc. similar strategy to step3. */

	  private void step4()
	  {
		switch (b[k])
		{
		case 'e':
		  if (ends("icate"))
		  {
			  r("ic");
			  break;
		  }
		  if (ends("ative"))
		  {
			  r("");
			  break;
		  }
		  if (ends("alize"))
		  {
			  r("al");
			  break;
		  }
		  break;
		case 'i':
		  if (ends("iciti"))
		  {
			  r("ic");
			  break;
		  }
		  break;
		case 'l':
		  if (ends("ical"))
		  {
			  r("ic");
			  break;
		  }
		  if (ends("ful"))
		  {
			  r("");
			  break;
		  }
		  break;
		case 's':
		  if (ends("ness"))
		  {
			  r("");
			  break;
		  }
		  break;
		}
	  }

	  /* step5() takes off -ant, -ence etc., in context <c>vcvc<v>. */

	  private void step5()
	  {
		if (k == k0) // for Bug 1
		{
			return;
		}
		switch (b[k - 1])
		{
		case 'a':
		  if (ends("al"))
		  {
			  break;
		  }
		  return;
		case 'c':
		  if (ends("ance"))
		  {
			  break;
		  }
		  if (ends("ence"))
		  {
			  break;
		  }
		  return;
		case 'e':
		  if (ends("er"))
		  {
			  break;
		  }
		  return;
		case 'i':
		  if (ends("ic"))
		  {
			  break;
		  }
		  return;
		case 'l':
		  if (ends("able"))
		  {
			  break;
		  }
		  if (ends("ible"))
		  {
			  break;
		  }
		  return;
		case 'n':
		  if (ends("ant"))
		  {
			  break;
		  }
		  if (ends("ement"))
		  {
			  break;
		  }
		  if (ends("ment"))
		  {
			  break;
		  }
		  /* element etc. not stripped before the m */
		  if (ends("ent"))
		  {
			  break;
		  }
		  return;
		case 'o':
		  if (ends("ion") && j >= 0 && (b[j] == 's' || b[j] == 't'))
		  {
			  break;
		  }
		  /* j >= 0 fixes Bug 2 */
		  if (ends("ou"))
		  {
			  break;
		  }
		  return;
		  /* takes care of -ous */
		case 's':
		  if (ends("ism"))
		  {
			  break;
		  }
		  return;
		case 't':
		  if (ends("ate"))
		  {
			  break;
		  }
		  if (ends("iti"))
		  {
			  break;
		  }
		  return;
		case 'u':
		  if (ends("ous"))
		  {
			  break;
		  }
		  return;
		case 'v':
		  if (ends("ive"))
		  {
			  break;
		  }
		  return;
		case 'z':
		  if (ends("ize"))
		  {
			  break;
		  }
		  return;
		default:
		  return;
		}
		if (m() > 1)
		{
		  k = j;
		}
	  }

	  /* step6() removes a final -e if m() > 1. */

	  private void step6()
	  {
		j = k;
		if (b[k] == 'e')
		{
		  int a = m();
		  if (a > 1 || a == 1 && !cvc(k - 1))
		  {
			k--;
		  }
		}
		if (b[k] == 'l' && doublec(k) && m() > 1)
		{
		  k--;
		}
	  }


	  /// <summary>
	  /// Stem a word provided as a String.  Returns the result as a String.
	  /// </summary>
	  public virtual string stem(string s)
	  {
		if (stem(s.ToCharArray(), s.Length))
		{
		  return ToString();
		}
		else
		{
		  return s;
		}
	  }

	  /// <summary>
	  /// Stem a word provided as a CharSequence.
	  /// Returns the result as a CharSequence.
	  /// </summary>
	  public virtual CharSequence stem(CharSequence word)
	  {
		return new CharSequence(stem(word.ToString()));
	  }

	  /// <summary>
	  /// Stem a word contained in a char[].  Returns true if the stemming process
	  /// resulted in a word different from the input.  You can retrieve the
	  /// result with getResultLength()/getResultBuffer() or toString().
	  /// </summary>
	  public virtual bool stem(char[] word)
	  {
		return stem(word, word.Length);
	  }

	  /// <summary>
	  /// Stem a word contained in a portion of a char[] array.  Returns
	  /// true if the stemming process resulted in a word different from
	  /// the input.  You can retrieve the result with
	  /// getResultLength()/getResultBuffer() or toString().
	  /// </summary>
	  public virtual bool stem(char[] wordBuffer, int offset, int wordLen)
	  {
		reset();
		if (b.Length < wordLen)
		{
		  b = new char[wordLen - offset];
		}
		Array.Copy(wordBuffer, offset, b, 0, wordLen);
		i = wordLen;
		return stem(0);
	  }

	  /// <summary>
	  /// Stem a word contained in a leading portion of a char[] array.
	  /// Returns true if the stemming process resulted in a word different
	  /// from the input.  You can retrieve the result with
	  /// getResultLength()/getResultBuffer() or toString().
	  /// </summary>
	  public virtual bool stem(char[] word, int wordLen)
	  {
		return stem(word, 0, wordLen);
	  }

	  /// <summary>
	  /// Stem the word placed into the Stemmer buffer through calls to add().
	  /// Returns true if the stemming process resulted in a word different
	  /// from the input.  You can retrieve the result with
	  /// getResultLength()/getResultBuffer() or toString().
	  /// </summary>
	  public virtual bool stem()
	  {
		return stem(0);
	  }

	  public virtual bool stem(int i0)
	  {
		k = i - 1;
		k0 = i0;
		if (k > k0 + 1)
		{
		  step1();
		  step2();
		  step3();
		  step4();
		  step5();
		  step6();
		}
		// Also, a word is considered dirty if we lopped off letters
		// Thanks to Ifigenia Vairelles for pointing this out.
		if (i != k + 1)
		{
		  dirty = true;
		}
		i = k + 1;
		return dirty;
	  }
	}


}