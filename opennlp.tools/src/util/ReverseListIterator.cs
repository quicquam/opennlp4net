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


	/// <summary>
	/// An iterator for a list which returns values in the opposite order as the typical list iterator.
	/// </summary>
	public class ReverseListIterator<T> : IEnumerator<T>
	{

	  private int index;
	  private IList<T> list;

	  public ReverseListIterator(IList<T> list)
	  {
		index = list.Count - 1;
		this.list = list;
	  }

	  public virtual T next()
	  {
		return list[index--];
	  }

	  public virtual bool hasNext()
	  {
		return index >= 0;
	  }

	  public virtual void remove()
	  {
		throw new System.NotSupportedException();
	  }
	}

}