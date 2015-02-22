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

namespace opennlp.maxent.io
{
    using QNModel = opennlp.maxent.quasinewton.QNModel;
    using AbstractModel = opennlp.model.AbstractModel;
    using AbstractModelWriter = opennlp.model.AbstractModelWriter;
    using Context = opennlp.model.Context;
    using opennlp.model;

    public abstract class QNModelWriter : AbstractModelWriter
    {
        protected internal string[] outcomeNames;
        protected internal string[] predNames;
        protected internal Context[] cParameters;
        protected internal double[] predParams;
        //protected EvalParameters evalParam;

        protected internal IndexHashTable<string> pmap;
        protected internal double[] parameters;

        public QNModelWriter(AbstractModel model)
        {
            object[] data = model.DataStructures;
            cParameters = (Context[])data[0];
            pmap = (IndexHashTable<string>) data[1];
            outcomeNames = (string[]) data[2];

            QNModel qnModel = (QNModel) model;
            parameters = qnModel.Parameters;
        }

        public override void persist()
        {
            // the type of model (QN)
            writeUTF("QN");

            // predNames
            predNames = new string[pmap.size()];
            pmap.toArray(predNames);
            writeInt(predNames.Length);
            for (int i = 0; i < predNames.Length; i++)
            {
                writeUTF(predNames[i]);
            }

            // outcomeNames
            writeInt(outcomeNames.Length);
            for (int i = 0; i < outcomeNames.Length; i++)
            {
                writeUTF(outcomeNames[i]);
            }

            // parameters
            writeInt(parameters.Length);
            foreach (Context currContext in cParameters)
            {
                writeInt(currContext.Outcomes.Length);
                for (int i = 0; i < currContext.Outcomes.Length; i++)
                {
                    writeInt(currContext.Outcomes[i]);
                }
                writeInt(currContext.Parameters.Length);
                for (int i = 0; i < currContext.Parameters.Length; i++)
                {
                    writeDouble(currContext.Parameters[i]);
                }
            }

            // parameters 2
            writeInt(parameters.Length);
            for (int i = 0; i < parameters.Length; i++)
            {
                writeDouble(parameters[i]);
            }
            close();
        }
    }
}