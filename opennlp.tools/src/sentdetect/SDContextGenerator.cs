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

using j4n.Lang;

namespace opennlp.tools.sentdetect
{


	/// <summary>
	/// Interface for <seealso cref="SentenceDetectorME"/> context generators.
	/// </summary>
	public interface SDContextGenerator
	{

	  /// <summary>
	  /// Returns an array of contextual features for the potential sentence boundary at the
	  /// specified position within the specified string buffer.
	  /// </summary>
	  /// <param name="s"> The <seealso cref="String"/> for which sentences are being determined. </param>
	  /// <param name="position"> An index into the specified string buffer when a sentence boundary may occur.
	  /// </param>
	  /// <returns> an array of contextual features for the potential sentence boundary at the
	  /// specified position within the specified string buffer. </returns>
	  string[] getContext(CharSequence s, int position);
	}

}