﻿/*
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


namespace opennlp.tools.tokenize
{

	using Span = opennlp.tools.util.Span;

	/// <summary>
	/// The interface for tokenizers, which segment a string into its tokens.
	/// <para>
	/// Tokenization is a necessary step before more complex NLP tasks can be applied,
	/// these usually process text on a token level. The quality of tokenization is
	/// important because it influences the performance of high-level task applied to it.
	/// </para>
	/// <para>
	/// In segmented languages like English most words are segmented by white spaces
	/// expect for punctuations, etc. which is directly attached to the word without a white space
	/// in between, it is not possible to just split at all punctuations because in abbreviations dots
	/// are a part of the token itself. A tokenizer is now responsible to split these tokens
	/// correctly.
	/// </para>
	/// <para>
	/// In non-segmented languages like Chinese tokenization is more difficult since words
	/// are not segmented by a whitespace.
	/// </para>
	/// <para>
	/// Tokenizers can also be used to segment already identified tokens further into more
	/// atomic parts to get a deeper understanding. This approach helps more complex task
	/// to gain insight into tokens which do not represent words like numbers, units or tokens
	/// which are part of a special notation.
	/// </para>
	/// <para>
	/// For most further task it is desirable to over tokenize rather than under tokenize.
	/// </para>
	/// </summary>
	public interface Tokenizer
	{

		/// <summary>
		/// Splits a string into its atomic parts
		/// </summary>
		/// <param name="s"> The string to be tokenized. </param>
		/// <returns>  The String[] with the individual tokens as the array
		///          elements. </returns>
		string[] tokenize(string s);

		/// <summary>
		/// Finds the boundaries of atomic parts in a string.
		/// </summary>
		/// <param name="s"> The string to be tokenized. </param>
		/// <returns> The Span[] with the spans (offsets into s) for each
		/// token as the individuals array elements. </returns>
		Span[] tokenizePos(string s);
	}

}