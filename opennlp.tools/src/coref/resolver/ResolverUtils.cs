using System;
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
using j4n.Lang;

namespace opennlp.tools.coref.resolver
{
    using MentionContext = opennlp.tools.coref.mention.MentionContext;
    using Parse = opennlp.tools.coref.mention.Parse;
    using GenderEnum = opennlp.tools.coref.sim.GenderEnum;
    using NumberEnum = opennlp.tools.coref.sim.NumberEnum;
    using TestSimilarityModel = opennlp.tools.coref.sim.TestSimilarityModel;

    /// <summary>
    /// This class provides a set of utilities for turning mentions into normalized strings and features.
    /// </summary>
    public class ResolverUtils
    {
        private static readonly Pattern ENDS_WITH_PERIOD = Pattern.compile("\\.$");
        private static readonly Pattern initialCaps = Pattern.compile("^[A-Z]");

        /// <summary>
        /// Regular expression for English singular third person pronouns. </summary>
        public static readonly Pattern singularThirdPersonPronounPattern =
            Pattern.compile("^(he|she|it|him|her|his|hers|its|himself|herself|itself)$", Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English plural third person pronouns. </summary>
        public static readonly Pattern pluralThirdPersonPronounPattern =
            Pattern.compile("^(they|their|theirs|them|themselves)$", Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English speech pronouns. </summary>
        public static readonly Pattern speechPronounPattern = Pattern.compile(
            "^(I|me|my|you|your|you|we|us|our|ours)$", Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English female pronouns. </summary>
        public static readonly Pattern femalePronounPattern = Pattern.compile("^(she|her|hers|herself)$",
            Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English neuter pronouns. </summary>
        public static readonly Pattern neuterPronounPattern = Pattern.compile("^(it|its|itself)$",
            Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English first person pronouns. </summary>
        public static readonly Pattern firstPersonPronounPattern = Pattern.compile("^(I|me|my|we|our|us|ours)$",
            Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English singular second person pronouns. </summary>
        public static readonly Pattern secondPersonPronounPattern = Pattern.compile("^(you|your|yours)$",
            Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English third person pronouns. </summary>
        public static readonly Pattern thirdPersonPronounPattern =
            Pattern.compile(
                "^(he|she|it|him|her|his|hers|its|himself|herself|itself|they|their|theirs|them|themselves)$",
                Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English singular pronouns. </summary>
        public static readonly Pattern singularPronounPattern =
            Pattern.compile("^(I|me|my|he|she|it|him|her|his|hers|its|himself|herself|itself)$",
                Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English plural pronouns. </summary>
        public static readonly Pattern pluralPronounPattern =
            Pattern.compile("^(we|us|our|ours|they|their|theirs|them|themselves)$", Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English male pronouns. </summary>
        public static readonly Pattern malePronounPattern = Pattern.compile("^(he|him|his|himself)$",
            Pattern.TextCase.CASE_INSENSITIVE);

        /// <summary>
        /// Regular expression for English honorifics. </summary>
        public static readonly Pattern honorificsPattern = Pattern.compile("[A-Z][a-z]+\\.$|^[A-Z][b-df-hj-np-tv-xz]+$");

        /// <summary>
        /// Regular expression for English corporate designators. </summary>
        public static readonly Pattern designatorsPattern =
            Pattern.compile("[a-z]\\.$|^[A-Z][b-df-hj-np-tv-xz]+$|^Co(rp)?$");


        private const string NUM_COMPATIBLE = "num.compatible";
        private const string NUM_INCOMPATIBLE = "num.incompatible";
        private const string NUM_UNKNOWN = "num.unknown";

        private const string GEN_COMPATIBLE = "gen.compatible";
        private const string GEN_INCOMPATIBLE = "gen.incompatible";
        private const string GEN_UNKNOWN = "gen.unknown";
        private const string SIM_COMPATIBLE = "sim.compatible";
        private const string SIM_INCOMPATIBLE = "sim.incompatible";
        private const string SIM_UNKNOWN = "sim.unknown";


        private const double MIN_SIM_PROB = 0.60;


        /// <summary>
        /// Returns a list of features based on the surrounding context of the specified mention. </summary>
        /// <param name="mention"> he mention whose surround context the features model. </param>
        /// <returns> a list of features based on the surrounding context of the specified mention </returns>
        public static IList<string> getContextFeatures(MentionContext mention)
        {
            IList<string> features = new List<string>();
            if (mention.PreviousToken != null)
            {
                features.Add("pt=" + mention.PreviousToken.SyntacticType);
                features.Add("pw=" + mention.PreviousToken.ToString());
            }
            else
            {
                features.Add("pt=BOS");
                features.Add("pw=BOS");
            }
            if (mention.NextToken != null)
            {
                features.Add("nt=" + mention.NextToken.SyntacticType);
                features.Add("nw=" + mention.NextToken.ToString());
            }
            else
            {
                features.Add("nt=EOS");
                features.Add("nw=EOS");
            }
            if (mention.NextTokenBasal != null)
            {
                features.Add("bnt=" + mention.NextTokenBasal.SyntacticType);
                features.Add("bnw=" + mention.NextTokenBasal.ToString());
            }
            else
            {
                features.Add("bnt=EOS");
                features.Add("bnw=EOS");
            }
            return (features);
        }

        /// <summary>
        /// Returns a list of word features for the specified tokens. </summary>
        /// <param name="token"> The token for which features are to be computed. </param>
        /// <returns> a list of word features for the specified tokens. </returns>
        public static IList<string> getWordFeatures(Parse token)
        {
            IList<string> wordFeatures = new List<string>();
            string word = token.ToString().ToLower();
            string wf = "";
            if (ENDS_WITH_PERIOD.matcher(word).find())
            {
                wf = ",endWithPeriod";
            }
            string tokTag = token.SyntacticType;
            wordFeatures.Add("w=" + word + ",t=" + tokTag + wf);
            wordFeatures.Add("t=" + tokTag + wf);
            return wordFeatures;
        }

        public static HashSet<string> constructModifierSet(Parse[] tokens, int headIndex)
        {
            HashSet<string> modSet = new HashSet<string>();
            for (int ti = 0; ti < headIndex; ti++)
            {
                Parse tok = tokens[ti];
                modSet.Add(tok.ToString().ToLower());
            }
            return (modSet);
        }

        public static string excludedDeterminerMentionString(MentionContext ec)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            Parse[] mtokens = ec.TokenParses;
            for (int ti = 0, tl = mtokens.Length; ti < tl; ti++)
            {
                Parse token = mtokens[ti];
                string tag = token.SyntacticType;
                if (!tag.Equals("DT"))
                {
                    if (!first)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(token.ToString());
                    first = false;
                }
            }
            return sb.ToString();
        }

        public static string excludedHonorificMentionString(MentionContext ec)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            object[] mtokens = ec.Tokens;
            for (int ti = 0, tl = mtokens.Length; ti < tl; ti++)
            {
                string token = mtokens[ti].ToString();
                if (!honorificsPattern.matcher(token).matches())
                {
                    if (!first)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(token);
                    first = false;
                }
            }
            return sb.ToString();
        }

        public static string excludedTheMentionString(MentionContext ec)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            object[] mtokens = ec.Tokens;
            for (int ti = 0, tl = mtokens.Length; ti < tl; ti++)
            {
                string token = mtokens[ti].ToString();
                if (!token.Equals("the") && !token.Equals("The") && !token.Equals("THE"))
                {
                    if (!first)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(token);
                    first = false;
                }
            }
            return sb.ToString();
        }

        public static string getExactMatchFeature(MentionContext ec, MentionContext xec)
        {
            //System.err.println("getExactMatchFeature: ec="+mentionString(ec)+" mc="+mentionString(xec));
            if (mentionString(ec).Equals(mentionString(xec)))
            {
                return "exactMatch";
            }
            else if (excludedHonorificMentionString(ec).Equals(excludedHonorificMentionString(xec)))
            {
                return "exactMatchNoHonor";
            }
            else if (excludedTheMentionString(ec).Equals(excludedTheMentionString(xec)))
            {
                return "exactMatchNoThe";
            }
            else if (excludedDeterminerMentionString(ec).Equals(excludedDeterminerMentionString(xec)))
            {
                return "exactMatchNoDT";
            }
            return null;
        }

        /// <summary>
        /// Returns string-match features for the the specified mention and entity. </summary>
        /// <param name="mention"> The mention. </param>
        /// <param name="entity"> The entity. </param>
        /// <returns> list of string-match features for the the specified mention and entity. </returns>
        public static IList<string> getStringMatchFeatures(MentionContext mention, DiscourseEntity entity)
        {
            bool sameHead = false;
            bool modsMatch = false;
            bool titleMatch = false;
            bool nonTheModsMatch = false;
            IList<string> features = new List<string>();
            Parse[] mtokens = mention.TokenParses;
            HashSet<string> ecModSet = constructModifierSet(mtokens, mention.HeadTokenIndex);
            string mentionHeadString = mention.HeadTokenText.ToLower();
            HashSet<string> featureSet = new HashSet<string>();
            for (IEnumerator<MentionContext> ei = entity.Mentions; ei.MoveNext();)
            {
                MentionContext entityMention = ei.Current;
                string exactMatchFeature = getExactMatchFeature(entityMention, mention);
                if (exactMatchFeature != null)
                {
                    featureSet.Add(exactMatchFeature);
                }
                else if (entityMention.Parse.CoordinatedNounPhrase && !mention.Parse.CoordinatedNounPhrase)
                {
                    featureSet.Add("cmix");
                }
                else
                {
                    string mentionStrip = stripNp(mention);
                    string entityMentionStrip = stripNp(entityMention);
                    if (mentionStrip != null && entityMentionStrip != null)
                    {
                        if (isSubstring(mentionStrip, entityMentionStrip))
                        {
                            featureSet.Add("substring");
                        }
                    }
                }
                Parse[] xtoks = entityMention.TokenParses;
                int headIndex = entityMention.HeadTokenIndex;
                //if (!mention.getHeadTokenTag().equals(entityMention.getHeadTokenTag())) {
                //  //System.err.println("skipping "+mention.headTokenText+" with "+xec.headTokenText+" because "+mention.headTokenTag+" != "+xec.headTokenTag);
                //  continue;
                //}  want to match NN NNP
                string entityMentionHeadString = entityMention.HeadTokenText.ToLower();
                // model lexical similarity
                if (mentionHeadString.Equals(entityMentionHeadString))
                {
                    sameHead = true;
                    featureSet.Add("hds=" + mentionHeadString);
                    if (!modsMatch || !nonTheModsMatch) //only check if we haven't already found one which is the same
                    {
                        modsMatch = true;
                        nonTheModsMatch = true;
                        HashSet<string> entityMentionModifierSet = constructModifierSet(xtoks, headIndex);
                        for (IEnumerator<string> mi = ecModSet.GetEnumerator(); mi.MoveNext();)
                        {
                            string mw = mi.Current;
                            if (!entityMentionModifierSet.Contains(mw))
                            {
                                modsMatch = false;
                                if (!mw.Equals("the"))
                                {
                                    nonTheModsMatch = false;
                                    featureSet.Add("mmw=" + mw);
                                }
                            }
                        }
                    }
                }
                HashSet<string> descModSet = constructModifierSet(xtoks, entityMention.NonDescriptorStart);
                if (descModSet.Contains(mentionHeadString))
                {
                    titleMatch = true;
                }
            }
            if (featureSet.Count > 0)
            {
                foreach (var feature in featureSet)
                {
                    features.Add(feature);
                }
            }
            if (sameHead)
            {
                features.Add("sameHead");
                if (modsMatch)
                {
                    features.Add("modsMatch");
                }
                else if (nonTheModsMatch)
                {
                    features.Add("nonTheModsMatch");
                }
                else
                {
                    features.Add("modsMisMatch");
                }
            }
            if (titleMatch)
            {
                features.Add("titleMatch");
            }
            return features;
        }

        public static bool isSubstring(string ecStrip, string xecStrip)
        {
            //System.err.println("MaxentResolver.isSubstring: ec="+ecStrip+" xec="+xecStrip);
            int io = xecStrip.IndexOf(ecStrip, StringComparison.Ordinal);
            if (io != -1)
            {
                //check boundries
                if (io != 0 && xecStrip[io - 1] != ' ')
                {
                    return false;
                }
                int end = io + ecStrip.Length;
                if (end != xecStrip.Length && xecStrip[end] != ' ')
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static string mentionString(MentionContext ec)
        {
            StringBuilder sb = new StringBuilder();
            object[] mtokens = ec.Tokens;
            sb.Append(mtokens[0].ToString());
            for (int ti = 1, tl = mtokens.Length; ti < tl; ti++)
            {
                string token = mtokens[ti].ToString();
                sb.Append(" ").Append(token);
            }
            //System.err.println("mentionString "+ec+" == "+sb.toString()+" mtokens.length="+mtokens.length);
            return sb.ToString();
        }

        /// <summary>
        /// Returns a string for the specified mention with punctuation, honorifics,
        /// designators, and determiners removed.
        /// </summary>
        /// <param name="mention"> The mention to be striped.
        /// </param>
        /// <returns> a normalized string representation of the specified mention. </returns>
        public static string stripNp(MentionContext mention)
        {
            int start = mention.NonDescriptorStart; //start after descriptors

            Parse[] mtokens = mention.TokenParses;
            int end = mention.HeadTokenIndex + 1;
            if (start == end)
            {
                //System.err.println("stripNp: return null 1");
                return null;
            }
            //strip determiners
            if (mtokens[start].SyntacticType.Equals("DT"))
            {
                start++;
            }
            if (start == end)
            {
                //System.err.println("stripNp: return null 2");
                return null;
            }
            //get to first NNP
            string type;
            for (int i = start; i < end; i++)
            {
                type = mtokens[start].SyntacticType;
                if (type.StartsWith("NNP", StringComparison.Ordinal))
                {
                    break;
                }
                start++;
            }
            if (start == end)
            {
                //System.err.println("stripNp: return null 3");
                return null;
            }
            if (start + 1 != end) // don't do this on head words, to keep "U.S."
            {
                //strip off honorifics in begining
                if (honorificsPattern.matcher(mtokens[start].ToString()).find())
                {
                    start++;
                }
                if (start == end)
                {
                    //System.err.println("stripNp: return null 4");
                    return null;
                }
                //strip off and honerifics on the end
                if (designatorsPattern.matcher(mtokens[mtokens.Length - 1].ToString()).find())
                {
                    end--;
                }
            }
            if (start == end)
            {
                //System.err.println("stripNp: return null 5");
                return null;
            }
            string strip = "";
            for (int i = start; i < end; i++)
            {
                strip += mtokens[i].ToString() + ' ';
            }
            return strip.Trim();
        }

        public static MentionContext getProperNounExtent(DiscourseEntity de)
        {
            for (IEnumerator<MentionContext> ei = de.Mentions; ei.MoveNext();) //use first extent which is propername
            {
                MentionContext xec = ei.Current;
                string xecHeadTag = xec.HeadTokenTag;
                if (xecHeadTag.StartsWith("NNP", StringComparison.Ordinal) ||
                    initialCaps.matcher(xec.HeadTokenText).find())
                {
                    return xec;
                }
            }
            return null;
        }

        private static IDictionary<string, string> getPronounFeatureMap(string pronoun)
        {
            IDictionary<string, string> pronounMap = new Dictionary<string, string>();
            if (malePronounPattern.matcher(pronoun).matches())
            {
                pronounMap["gender"] = "male";
            }
            else if (femalePronounPattern.matcher(pronoun).matches())
            {
                pronounMap["gender"] = "female";
            }
            else if (neuterPronounPattern.matcher(pronoun).matches())
            {
                pronounMap["gender"] = "neuter";
            }
            if (singularPronounPattern.matcher(pronoun).matches())
            {
                pronounMap["number"] = "singular";
            }
            else if (pluralPronounPattern.matcher(pronoun).matches())
            {
                pronounMap["number"] = "plural";
            }
            /*
		if (Linker.firstPersonPronounPattern.matcher(pronoun).matches()) {
		  pronounMap.put("person","first");
		}
		else if (Linker.secondPersonPronounPattern.matcher(pronoun).matches()) {
		  pronounMap.put("person","second");
		}
		else if (Linker.thirdPersonPronounPattern.matcher(pronoun).matches()) {
		  pronounMap.put("person","third");
		}
		*/
            return pronounMap;
        }

        /// <summary>
        /// Returns features indicating whether the specified mention is compatible with the pronouns
        /// of the specified entity. </summary>
        /// <param name="mention"> The mention. </param>
        /// <param name="entity"> The entity. </param>
        /// <returns> list of features indicating whether the specified mention is compatible with the pronouns
        /// of the specified entity. </returns>
        public static IList<string> getPronounMatchFeatures(MentionContext mention, DiscourseEntity entity)
        {
            bool foundCompatiblePronoun = false;
            bool foundIncompatiblePronoun = false;
            if (mention.HeadTokenTag.StartsWith("PRP", StringComparison.Ordinal))
            {
                IDictionary<string, string> pronounMap = getPronounFeatureMap(mention.HeadTokenText);
                //System.err.println("getPronounMatchFeatures.pronounMap:"+pronounMap);
                for (IEnumerator<MentionContext> mi = entity.Mentions; mi.MoveNext();)
                {
                    MentionContext candidateMention = mi.Current;
                    if (candidateMention.HeadTokenTag.StartsWith("PRP", StringComparison.Ordinal))
                    {
                        if (mention.HeadTokenText.Equals(candidateMention.HeadTokenText,
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            foundCompatiblePronoun = true;
                            break;
                        }
                        else
                        {
                            IDictionary<string, string> candidatePronounMap =
                                getPronounFeatureMap(candidateMention.HeadTokenText);
                            //System.err.println("getPronounMatchFeatures.candidatePronounMap:"+candidatePronounMap);
                            bool allKeysMatch = true;
                            for (IEnumerator<string> ki = pronounMap.Keys.GetEnumerator(); ki.MoveNext();)
                            {
                                string key = ki.Current;
                                string cfv = candidatePronounMap[key];
                                if (cfv != null)
                                {
                                    if (!pronounMap[key].Equals(cfv))
                                    {
                                        foundIncompatiblePronoun = true;
                                        allKeysMatch = false;
                                    }
                                }
                                else
                                {
                                    allKeysMatch = false;
                                }
                            }
                            if (allKeysMatch)
                            {
                                foundCompatiblePronoun = true;
                            }
                        }
                    }
                }
            }
            IList<string> pronounFeatures = new List<string>();
            if (foundCompatiblePronoun)
            {
                pronounFeatures.Add("compatiblePronoun");
            }
            if (foundIncompatiblePronoun)
            {
                pronounFeatures.Add("incompatiblePronoun");
            }
            return pronounFeatures;
        }

        /// <summary>
        /// Returns distance features for the specified mention and entity. </summary>
        /// <param name="mention"> The mention. </param>
        /// <param name="entity"> The entity. </param>
        /// <returns> list of distance features for the specified mention and entity. </returns>
        public static IList<string> getDistanceFeatures(MentionContext mention, DiscourseEntity entity)
        {
            IList<string> features = new List<string>();
            MentionContext cec = entity.LastExtent;
            int entityDistance = mention.NounPhraseDocumentIndex - cec.NounPhraseDocumentIndex;
            int sentenceDistance = mention.SentenceNumber - cec.SentenceNumber;
            int hobbsEntityDistance;
            if (sentenceDistance == 0)
            {
                hobbsEntityDistance = cec.NounPhraseSentenceIndex;
            }
            else
            {
                //hobbsEntityDistance = entityDistance - (entities within sentence from mention to end) + (entities within sentence form start to mention)
                //hobbsEntityDistance = entityDistance - (cec.maxNounLocation - cec.getNounPhraseSentenceIndex) + cec.getNounPhraseSentenceIndex;
                hobbsEntityDistance = entityDistance + (2*cec.NounPhraseSentenceIndex) - cec.MaxNounPhraseSentenceIndex;
            }
            features.Add("hd=" + hobbsEntityDistance);
            features.Add("de=" + entityDistance);
            features.Add("ds=" + sentenceDistance);
            //features.add("ds=" + sdist + pronoun);
            //features.add("dn=" + cec.sentenceNumber);
            //features.add("ep=" + cec.nounLocation);
            return (features);
        }

        /// <summary>
        /// Returns whether the specified token is a definite article. </summary>
        /// <param name="tok"> The token. </param>
        /// <param name="tag"> The pos-tag for the specified token. </param>
        /// <returns> whether the specified token is a definite article. </returns>
        public static bool definiteArticle(string tok, string tag)
        {
            tok = tok.ToLower();
            if (tok.Equals("the") || tok.Equals("these") || tok.Equals("these") || tag.Equals("PRP$"))
            {
                return (true);
            }
            return (false);
        }

        public static string getNumberCompatibilityFeature(MentionContext ec, DiscourseEntity de)
        {
            NumberEnum en = de.Number;
            if (en == NumberEnum.UNKNOWN || ec.Number == NumberEnum.UNKNOWN)
            {
                return NUM_UNKNOWN;
            }
            else if (ec.Number == en)
            {
                return NUM_COMPATIBLE;
            }
            else
            {
                return NUM_INCOMPATIBLE;
            }
        }


        /// <summary>
        /// Returns features indicating whether the specified mention and the specified entity are compatible. </summary>
        /// <param name="mention"> The mention. </param>
        /// <param name="entity"> The entity. </param>
        /// <returns> list of features indicating whether the specified mention and the specified entity are compatible. </returns>
        public static IList<string> getCompatibilityFeatures(MentionContext mention, DiscourseEntity entity,
            TestSimilarityModel simModel)
        {
            IList<string> compatFeatures = new List<string>();
            string semCompatible = getSemanticCompatibilityFeature(mention, entity, simModel);
            compatFeatures.Add(semCompatible);
            string genCompatible = getGenderCompatibilityFeature(mention, entity);
            compatFeatures.Add(genCompatible);
            string numCompatible = ResolverUtils.getNumberCompatibilityFeature(mention, entity);
            compatFeatures.Add(numCompatible);
            if (semCompatible.Equals(SIM_COMPATIBLE) && genCompatible.Equals(GEN_COMPATIBLE) &&
                numCompatible.Equals(ResolverUtils.NUM_COMPATIBLE))
            {
                compatFeatures.Add("all.compatible");
            }
            else if (semCompatible.Equals(SIM_INCOMPATIBLE) || genCompatible.Equals(GEN_INCOMPATIBLE) ||
                     numCompatible.Equals(ResolverUtils.NUM_INCOMPATIBLE))
            {
                compatFeatures.Add("some.incompatible");
            }
            return compatFeatures;
        }

        public static string getGenderCompatibilityFeature(MentionContext ec, DiscourseEntity de)
        {
            GenderEnum eg = de.Gender;
            //System.err.println("getGenderCompatibility: mention="+ec.getGender()+" entity="+eg);
            if (eg == GenderEnum.UNKNOWN || ec.Gender == GenderEnum.UNKNOWN)
            {
                return GEN_UNKNOWN;
            }
            else if (ec.Gender == eg)
            {
                return GEN_COMPATIBLE;
            }
            else
            {
                return GEN_INCOMPATIBLE;
            }
        }

        public static string getSemanticCompatibilityFeature(MentionContext ec, DiscourseEntity de,
            TestSimilarityModel simModel)
        {
            if (simModel != null)
            {
                double best = 0;
                for (IEnumerator<MentionContext> xi = de.Mentions; xi.MoveNext();)
                {
                    MentionContext ec2 = xi.Current;
                    double sim = simModel.compatible(ec, ec2);
                    if (sim > best)
                    {
                        best = sim;
                    }
                }
                if (best > MIN_SIM_PROB)
                {
                    return SIM_COMPATIBLE;
                }
                else if (best > (1 - MIN_SIM_PROB))
                {
                    return SIM_UNKNOWN;
                }
                else
                {
                    return SIM_INCOMPATIBLE;
                }
            }
            else
            {
                Console.Error.WriteLine("MaxentResolver: Uninitialized Semantic Model");
                return SIM_UNKNOWN;
            }
        }

        public static string getMentionCountFeature(DiscourseEntity de)
        {
            if (de.NumMentions >= 5)
            {
                return ("mc=5+");
            }
            else
            {
                return ("mc=" + de.NumMentions);
            }
        }

        /// <summary>
        /// Returns a string representing the gender of the specified pronoun. </summary>
        /// <param name="pronoun"> An English pronoun. </param>
        /// <returns> the gender of the specified pronoun. </returns>
        public static string getPronounGender(string pronoun)
        {
            if (malePronounPattern.matcher(pronoun).matches())
            {
                return "m";
            }
            else if (femalePronounPattern.matcher(pronoun).matches())
            {
                return "f";
            }
            else if (neuterPronounPattern.matcher(pronoun).matches())
            {
                return "n";
            }
            else
            {
                return "u";
            }
        }
    }
}