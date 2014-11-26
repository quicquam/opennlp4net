using System;
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

namespace opennlp.tools.parser
{
    /*
    public sealed class ParserType
    {
        public static readonly ParserType CHUNKING = new ParserType("CHUNKING", InnerEnum.CHUNKING);
        public static readonly ParserType TREEINSERT = new ParserType("TREEINSERT", InnerEnum.TREEINSERT);

        private static readonly IList<ParserType> valueList = new List<ParserType>();

        static ParserType()
        {
            valueList.Add(CHUNKING);
            valueList.Add(TREEINSERT);
        }

        public enum InnerEnum
        {
            CHUNKING,
            TREEINSERT
        }

        private readonly string nameValue;
        private readonly int ordinalValue;
        private readonly InnerEnum innerEnumValue;
        private static int nextOrdinal = 0;
        public string name;

        private ParserType(string name, InnerEnum innerEnum)
        {
            nameValue = name;
            innerEnumValue = innerEnum;
        }

        public static ParserType parse(string type)
        {
            if (ParserType.CHUNKING.name.Equals(type))
            {
                return ParserType.CHUNKING;
            }
            else if (ParserType.TREEINSERT.name.Equals(type))
            {
                return ParserType.TREEINSERT;
            }
            else
            {
                return null;
            }
        }

        public static IList<ParserType> values()
        {
            return valueList;
        }

        public InnerEnum InnerEnumValue()
        {
            return innerEnumValue;
        }

        public int ordinal()
        {
            return ordinalValue;
        }

        public override string ToString()
        {
            return nameValue;
        }

        public static ParserType valueOf(string name)
        {
            foreach (ParserType enumInstance in ParserType.values())
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new System.ArgumentException(name);
        }
    }
    */
    public enum ParserTypeEnum
    {
        CHUNKING,
        TREEINSERT,
        UNKNOWN
    }
    public static class ParserType
    {
        public static ParserTypeEnum parse(String type)
        {
            if (ParserTypeEnum.CHUNKING.ToString("g") == type)
            {
                return ParserTypeEnum.CHUNKING;
            }
            return ParserTypeEnum.TREEINSERT.ToString("g") == type ? ParserTypeEnum.TREEINSERT : ParserTypeEnum.UNKNOWN;
        }
    }
}