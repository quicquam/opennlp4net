using System;
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
using opennlp.nonjava.helperclasses;

namespace opennlp.model
{

	/// <summary>
	/// A maxent event representation which we can use to sort based on the
	/// predicates indexes contained in the events.
	/// </summary>
	public class ComparableEvent : IComparable<ComparableEvent>
	{
	  public int outcome;
	  public int[] predIndexes;
	  public int seen = 1; // the number of times this event
						   // has been seen.

	  public float[] values;

	  public ComparableEvent(int oc, int[] pids, float[] values)
	  {
		outcome = oc;
		if (values == null)
		{
		  Arrays.sort(pids);
		}
		else
		{
		  sort(pids, values);
		}
		this.values = values; // needs to be sorted like pids
		predIndexes = pids;
	  }

	  public ComparableEvent(int oc, int[] pids) : this(oc, pids, null)
	  {
	  }

	  public virtual int CompareTo(ComparableEvent ce)
	  {

		if (outcome < ce.outcome)
		{
		  return -1;
		}
		else if (outcome > ce.outcome)
		{
		  return 1;
		}

		int smallerLength = (predIndexes.Length > ce.predIndexes.Length ? ce.predIndexes.Length : predIndexes.Length);

		for (int i = 0; i < smallerLength; i++)
		{
		  if (predIndexes[i] < ce.predIndexes[i])
		  {
			return -1;
		  }
		  else if (predIndexes[i] > ce.predIndexes[i])
		  {
			return 1;
		  }
		  if (values != null && ce.values != null)
		  {
			if (values[i] < ce.values[i])
			{
			  return -1;
			}
			else if (values[i] > ce.values[i])
			{
			  return 1;
			}
		  }
		  else if (values != null)
		  {
			if (values[i] < 1)
			{
			  return -1;
			}
			else if (values[i] > 1)
			{
			  return 1;
			}
		  }
		  else if (ce.values != null)
		  {
			if (1 < ce.values[i])
			{
			  return -1;
			}
			else if (1 > ce.values[i])
			{
			  return 1;
			}
		  }
		}

		if (predIndexes.Length < ce.predIndexes.Length)
		{
		  return -1;
		}
		else if (predIndexes.Length > ce.predIndexes.Length)
		{
		  return 1;
		}

		return 0;
	  }

	  public override string ToString()
	  {
		StringBuilder s = (new StringBuilder()).Append(outcome).Append(":");
		for (int i = 0; i < predIndexes.Length; i++)
		{
		  s.Append(" ").Append(predIndexes[i]);
		  if (values != null)
		  {
			s.Append("=").Append(values[i]);
		  }
		}
		return s.ToString();
	  }

	  private void sort(int[] pids, float[] values)
	  {
		for (int mi = 0; mi < pids.Length; mi++)
		{
		  int min = mi;
		  for (int pi = mi + 1; pi < pids.Length; pi++)
		  {
			if (pids[min] > pids[pi])
			{
			  min = pi;
			}
		  }
		  int pid = pids[mi];
		  pids[mi] = pids[min];
		  pids[min] = pid;
		  float val = values[mi];
		  values[mi] = values[min];
		  values[min] = val;
		}
	  }
	}


}