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

using System;
using j4n.Interfaces;
using j4n.IO.OutputStream;

namespace opennlp.tools.cmdline.chunker
{

	using ChunkSample = opennlp.tools.chunker.ChunkSample;
	using ChunkerEvaluationMonitor = opennlp.tools.chunker.ChunkerEvaluationMonitor;
	using opennlp.tools.cmdline;
	using opennlp.tools.util.eval;

	/// <summary>
	/// A default implementation of <seealso cref="EvaluationMonitor"/> that prints
	/// to an output stream.
	/// 
	/// </summary>
	public class ChunkEvaluationErrorListener : EvaluationErrorPrinter<ChunkSample>, ChunkerEvaluationMonitor
	{

	  /// <summary>
	  /// Creates a listener that will print to System.err
	  /// </summary>
	  public ChunkEvaluationErrorListener() : base(Console.Error)
	  {
	  }

	  /// <summary>
	  /// Creates a listener that will print to a given <seealso cref="OutputStream"/>
	  /// </summary>
	  public ChunkEvaluationErrorListener(OutputStream outputStream) : base(outputStream)
	  {
	  }

	  public override void missclassified(ChunkSample reference, ChunkSample prediction)
	  {
		printError(reference.PhrasesAsSpanList, prediction.PhrasesAsSpanList, reference, prediction, reference.Sentence);
	  }

	}

}