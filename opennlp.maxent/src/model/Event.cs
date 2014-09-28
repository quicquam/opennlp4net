using System.Text;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace opennlp.model
{


	/// <summary>
	/// The context of a decision point during training.  This includes
	/// contextual predicates and an outcome.
	/// </summary>
	public class Event
	{
		private string outcome;
		private string[] context;
		private float[] values;

		public Event(string outcome, string[] context) : this(outcome,context,null)
		{
		}

		public Event(string outcome, string[] context, float[] values)
		{
		  this.outcome = outcome;
		  this.context = context;
		  this.values = values;
		}

		public virtual string Outcome
		{
			get
			{
			  return outcome;
			}
		}

		public virtual string[] Context
		{
			get
			{
			  return context;
			}
		}

		public virtual float[] Values
		{
			get
			{
			  return values;
			}
		}

		public override string ToString()
		{
		  StringBuilder sb = new StringBuilder();
		  sb.Append(outcome).Append(" [");
		  if (context.Length > 0)
		  {
			sb.Append(context[0]);
			if (values != null)
			{
			  sb.Append("=").Append(values[0]);
			}
		  }
		  for (int ci = 1;ci < context.Length;ci++)
		  {
			sb.Append(" ").Append(context[ci]);
			if (values != null)
			{
			  sb.Append("=").Append(values[ci]);
			}
		  }
		  sb.Append("]");
		  return sb.ToString();
		}

	}

}