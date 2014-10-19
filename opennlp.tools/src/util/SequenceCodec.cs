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

namespace opennlp.tools.util
{

	public interface SequenceCodec<T>
	{

	  /// <summary>
	  /// Decodes a sequence T objects into Span objects.
	  /// </summary>
	  /// <param name="c">
	  /// 
	  /// @return </param>
	  Span[] decode(IList<T> c);

	  /// <summary>
	  /// Encodes Span objects into a sequence of T objects.
	  /// </summary>
	  /// <param name="names"> </param>
	  /// <param name="length">
	  /// 
	  /// @return </param>
	  T[] encode(Span[] names, int length);

	  /// <summary>
	  /// Creates a sequence validator which can validate a sequence of outcomes.
	  /// 
	  /// @return
	  /// </summary>
	  SequenceValidator<T> createSequenceValidator();

	  /// <summary>
	  /// Checks if the outcomes of the model are compatible with the codec.
	  /// </summary>
	  /// <param name="outcomes"> all possible model outcomes
	  /// 
	  /// @return </param>
	  bool areOutcomesCompatible(string[] outcomes);
	}

}