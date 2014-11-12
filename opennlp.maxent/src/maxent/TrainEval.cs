﻿using System;
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
using j4n.IO.Reader;

namespace opennlp.maxent
{
    using Event = opennlp.model.Event;
    using EventStream = opennlp.model.EventStream;
    using MaxentModel = opennlp.model.MaxentModel;

    /// <summary>
    /// Trains or evaluates maxent components which have implemented the Evalable
    /// interface.
    /// </summary>
    public class TrainEval
    {
        public static void eval(MaxentModel model, Reader r, Evalable e)
        {
            eval(model, r, e, false);
        }

        public static void eval(MaxentModel model, Reader r, Evalable e, bool verbose)
        {
            float totPos = 0, truePos = 0, falsePos = 0;
            Event[] events = (e.getEventCollector(r)).getEvents(true);
            //MaxentModel model = e.getModel(dir, name);
            string negOutcome = e.NegativeOutcome;
            foreach (Event @event in events)
            {
                string guess = model.getBestOutcome(model.eval(@event.Context));
                string ans = @event.Outcome;
                if (verbose)
                {
                    Console.WriteLine(ans + " " + guess);
                }

                if (!ans.Equals(negOutcome))
                {
                    totPos++;
                }

                if (!guess.Equals(negOutcome) && !guess.Equals(ans))
                {
                    falsePos++;
                }
                else if (ans.Equals(guess))
                {
                    truePos++;
                }
            }

            Console.WriteLine("Precision: " + truePos/(truePos + falsePos));
            Console.WriteLine("Recall:    " + truePos/totPos);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static opennlp.model.MaxentModel train(opennlp.model.EventStream events, int cutoff) throws java.io.IOException
        public static MaxentModel train(EventStream events, int cutoff)
        {
            return GIS.trainModel(events, 100, cutoff);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void run(String[] args, Evalable e) throws java.io.IOException
        public static void run(string[] args, Evalable e)
        {
            // TOM: Was commented out to remove dependency on gnu getopt.    	

            //	String dir = "./";
            //	String stem = "maxent";
            //	int cutoff = 0; // default to no cutoff
            //	boolean train = false;
            //	boolean verbose = false;
            //	boolean local = false;
            //	gnu.getopt.Getopt g =
            //	    new gnu.getopt.Getopt("maxent", args, "d:s:c:tvl");
            //	int c;
            //	while ((c = g.getopt()) != -1) {
            //	    switch(c) {
            //	    case 'd':
            //		dir = g.getOptarg()+"/";
            //		break;
            //	    case 's':
            //		stem = g.getOptarg();
            //		break;
            //	    case 'c':
            //		cutoff = Integer.parseInt(g.getOptarg());
            //		break;
            //	    case 't':
            //		train = true;
            //		break;
            //	    case 'l':
            //		local = true;
            //		break;
            //	    case 'v':
            //		verbose = true;
            //		break;
            //	    }
            //	}
            //
            //	int lastIndex = g.getOptind();
            //	if (lastIndex >= args.length) {
            //	    System.out.println("This is a usage message from opennlp.maxent.TrainEval. You have called the training procedure for a maxent application with the incorrect arguments.  These are the options:");
            //
            //	    System.out.println("\nOptions for defining the model location and name:");
            //	    System.out.println(" -d <directoryName>");
            //	    System.out.println("\tThe directory in which to store the model.");
            //	    System.out.println(" -s <modelName>");
            //	    System.out.println("\tThe name of the model, e.g. EnglishPOS.bin.gz or NameFinder.txt.");
            //	    
            //	    System.out.println("\nOptions for training:");
            //	    System.out.println(" -c <cutoff>");
            //	    System.out.println("\tAn integer cutoff level to reduce infrequent contextual predicates.");
            //	    System.out.println(" -t\tTrain a model. If absent, the given model will be loaded and evaluated.");
            //	    System.out.println("\nOptions for evaluation:");
            //	    System.out.println(" -l\t the evaluation method of class that uses the model. If absent, TrainEval's eval method is used.");
            //	    System.out.println(" -v\t verbose.");
            //	    System.out.println("\nThe final argument is the data file to be loaded and used for either training or evaluation.");
            //	    System.out.println("\nAs an example for training:\n java opennlp.grok.preprocess.postag.POSTaggerME -t -d ./ -s EnglishPOS.bin.gz -c 7 postag.data");
            //	    System.exit(0);
            //	}
            //
            //	FileReader datafr = new FileReader(args[lastIndex]);
            //	
            //	if (train) {
            //	    MaxentModel m =
            //		train(new EventCollectorAsStream(e.getEventCollector(datafr)),
            //		      cutoff);
            //	    new SuffixSensitiveGISModelWriter((AbstractModel)m,
            //					      new File(dir+stem)).persist();
            //	}
            //	else {
            //	    MaxentModel model =
            //		new SuffixSensitiveGISModelReader(new File(dir+stem)).getModel();
            //	    if (local) {
            //		e.localEval(model, datafr, e, verbose);
            //	    } else {
            //		eval(model, datafr, e, verbose);
            //	    }
            //	}
        }
    }
}