using System.Collections.Generic;
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

namespace opennlp.tools.formats.ad
{


	/// <summary>
	/// Utility class to handle Portuguese contractions.
	/// <para>
	/// Some Corpora splits contractions in its parts, for example, "da" > "de" +
	/// "a", but according to the fase of language processing, NER for instance, we
	/// can't decide if to split a contraction or not, specially because contractions
	/// inside names are not separated, but outside are.
	/// 
	/// </para>
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class PortugueseContractionUtility
	{

	  protected internal static readonly IDictionary<string, string> CONTRACTIONS;

	  static PortugueseContractionUtility()
	  {
		IDictionary<string, string> elems = new Dictionary<string, string>();
		// 103 CONTRACTIONS.
		elems["a+a"] = "\u00e0";
		elems["a+as"] = "\u00e0s";
		elems["a+aquele"] = "\u00e0quele";
		elems["a+aqueles"] = "\u00e0queles";
		elems["a+aquela"] = "\u00e0quela";
		elems["a+aquelas"] = "\u00e0quelas";
		elems["a+aquilo"] = "\u00e0quilo";
		elems["a+o"] = "ao";
		elems["a+os"] = "aos";
		elems["com+mim"] = "comigo";
		elems["com+n\u00f2s"] = "conosco";
		elems["com+si"] = "consigo";
		elems["com+ti"] = "contigo";
		elems["com+v\u00f2s"] = "convosco";
		elems["de+a\u00ed"] = "da\u00ed";
		elems["de+algu\u00e9m"] = "dalgu\u00e9m";
		elems["de+algum"] = "dalgum";
		elems["de+alguma"] = "dalguma";
		elems["de+alguns"] = "dalguns";
		elems["de+algumas"] = "dalgumas";
		elems["de+ali"] = "dali";
		elems["de+aqu\u00e9m"] = "daqu\u00e9m";
		elems["de+aquele"] = "daquele";
		elems["de+aquela"] = "daquela";
		elems["de+aqueles"] = "daqueles";
		elems["de+aquelas"] = "daquelas";
		elems["de+aqui"] = "daqui";
		elems["de+aquilo"] = "daquilo";
		elems["de+ele"] = "dele";
		elems["de+ela"] = "dela";
		elems["de+eles"] = "deles";
		elems["de+elas"] = "delas";
		elems["de+entre"] = "dentre";
		elems["de+esse"] = "desse";
		elems["de+essa"] = "dessa";
		elems["de+esses"] = "desses";
		elems["de+essas"] = "dessas";
		elems["de+este"] = "deste";
		elems["de+esta"] = "desta";
		elems["de+estes"] = "destes";
		elems["de+estas"] = "destas";
		elems["de+isso"] = "disso";
		elems["de+isto"] = "disto";
		elems["de+o"] = "do";
		elems["de+a"] = "da";
		elems["de+os"] = "dos";
		elems["de+as"] = "das";
		elems["de+outrem"] = "doutrem";
		elems["de+outro"] = "doutro";
		elems["de+outra"] = "doutra";
		elems["de+outros"] = "doutros";
		elems["de+outras"] = "doutras";
		elems["de+um"] = "dum";
		elems["de+uma"] = "duma";
		elems["de+uns"] = "duns";
		elems["de+umas"] = "dumas";
		elems["esse+outro"] = "essoutro";
		elems["essa+outra"] = "essoutra";
		elems["este+outro"] = "estoutro";
		elems["este+outra"] = "estoutra";
		elems["ele+o"] = "lho";
		elems["ele+a"] = "lha";
		elems["ele+os"] = "lhos";
		elems["ele+as"] = "lhas";
		elems["em+algum"] = "nalgum";
		elems["em+alguma"] = "nalguma";
		elems["em+alguns"] = "nalguns";
		elems["em+algumas"] = "nalgumas";
		elems["em+aquele"] = "naquele";
		elems["em+aquela"] = "naquela";
		elems["em+aqueles"] = "naqueles";
		elems["em+aquelas"] = "naquelas";
		elems["em+aquilo"] = "naquilo";
		elems["em+ele"] = "nele";
		elems["em+ela"] = "nela";
		elems["em+eles"] = "neles";
		elems["em+elas"] = "nelas";
		elems["em+esse"] = "nesse";
		elems["em+essa"] = "nessa";
		elems["em+esses"] = "nesses";
		elems["em+essas"] = "nessas";
		elems["em+este"] = "neste";
		elems["em+esta"] = "nesta";
		elems["em+estes"] = "nestes";
		elems["em+estas"] = "nestas";
		elems["em+isso"] = "nisso";
		elems["em+isto"] = "nisto";
		elems["em+o"] = "no";
		elems["em+a"] = "na";
		elems["em+os"] = "nos";
		elems["em+as"] = "nas";
		elems["em+outro"] = "noutro";
		elems["em+outra"] = "noutra";
		elems["em+outros"] = "noutros";
		elems["em+outras"] = "noutras";
		elems["em+um"] = "num";
		elems["em+uma"] = "numa";
		elems["em+uns"] = "nuns";
		elems["em+umas"] = "numas";
		elems["por+o"] = "pelo";
		elems["por+a"] = "pela";
		elems["por+os"] = "pelos";
		elems["por+as"] = "pelas";
		elems["para+a"] = "pra";
		elems["para+o"] = "pro";
		elems["para+as"] = "pras";
		elems["para+os"] = "pros";
		CONTRACTIONS = elems;
	  }

	  /// <summary>
	  /// Merges a contraction
	  /// </summary>
	  /// <param name="left">
	  ///          the left component </param>
	  /// <param name="right">
	  ///          the right component </param>
	  /// <returns> the merged contraction </returns>
	  public static string toContraction(string left, string right)
	  {
		string key = left + "+" + right;
		if (CONTRACTIONS.ContainsKey(key))
		{
		  return CONTRACTIONS[key];
		}
		else
		{
		  StringBuilder sb = new StringBuilder();
		  string[] parts = left.Split('_');
		  for (int i = 0; i < parts.Length - 1; i++)
		  {
			sb.Append(parts[i]).Append(" ");
		  }
		  key = parts[parts.Length - 1] + "+" + right;
		  if (CONTRACTIONS.ContainsKey(key))
		  {
			sb.Append(CONTRACTIONS[key]);
			return sb.ToString();
		  }

		  if (right.Contains("_"))
		  {
			parts = right.Split('_');

			key = left + "+" + parts[0];
			if (CONTRACTIONS.ContainsKey(key))
			{
			  sb.Append(CONTRACTIONS[key]).Append(" ");

			  for (int i = 1; i < parts.Length; i++)
			  {
				sb.Append(parts[i]).Append(" ");
			  }

			  return sb.ToString();
			}

		  }

		  string leftLower = parts[parts.Length - 1].ToLower();
		  key = leftLower + "+" + right;
		  if (CONTRACTIONS.ContainsKey(key))
		  {
			string r = CONTRACTIONS[key];
			string firstChar = r.Substring(0, 1);
			r = firstChar.ToUpper() + r.Substring(1);
			sb.Append(r);
			return sb.ToString();
		  }
		}

		return null;
	  }
	}

}