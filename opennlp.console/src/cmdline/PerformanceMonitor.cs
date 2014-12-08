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


using System;
using System.IO;
using j4n.Exceptions;
using System.Threading;

namespace opennlp.console.cmdline
{


	/// <summary>
	/// The <seealso cref="PerformanceMonitor"/> measures increments to a counter.
	/// During the computation it prints out current and average throughput
	/// per second. After the computation is done it prints a final performance
	/// report.
	/// <para>
	/// <b>Note:</b>
	/// This class is not thread safe. <br>
	/// Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class PerformanceMonitor
	{
	    private TextWriter _textWriter;
        private readonly string _unit;

        private long _startTime = -1;

        private int _counter;
        
        public PerformanceMonitor(TextWriter error, string unit)
        {
            _textWriter = error;
            _unit = unit;
        }

	    public PerformanceMonitor(string unit)
	    {
	        _textWriter = Console.Error;
	        _unit = unit;
	    }

        public virtual bool Started
        {
            get
            {
                return _startTime != -1;
            }
        }

        public virtual void incrementCounter(int increment)
        {

            if (!Started)
            {
                throw new IllegalStateException("Must be started first!");
            }

            if (increment < 0)
            {
                throw new System.ArgumentException("increment must be zero or positive but was " + increment + "!");
            }

            _counter += increment;
        }

        public virtual void incrementCounter()
        {
            incrementCounter(1);
        }

        public virtual void start()
        {

            if (Started)
            {
                throw new IllegalStateException("Already started!");
            }

            _startTime = DateTime.Now.Ticks;
        }

        public virtual void startAndPrintThroughput()
        {

            start();

            var helper = new RunnableAnonymousInnerClassHelper(this);

            Thread thread = new Thread(new ThreadStart(helper.run));
            thread.Start();
        }

        private class RunnableAnonymousInnerClassHelper
        {
            private readonly PerformanceMonitor _outerInstance;

            public RunnableAnonymousInnerClassHelper(PerformanceMonitor outerInstance)
            {
                _outerInstance = outerInstance;
                _lastTimeStamp = outerInstance._startTime;
                _lastCount = outerInstance._counter;
            }

            private long _lastTimeStamp;
            private int _lastCount;

            public virtual void run()
            {

                int deltaCount = _outerInstance._counter - _lastCount;

                long timePassedSinceLastCount = DateTime.Now.Ticks - _lastTimeStamp;

                double currentThroughput;

                if (timePassedSinceLastCount > 0)
                {
                    currentThroughput = deltaCount / ((double)timePassedSinceLastCount / 1000);
                }
                else
                {
                    currentThroughput = 0;
                }

                long totalTimePassed = DateTime.Now.Ticks - _outerInstance._startTime;

                double averageThroughput;
                if (totalTimePassed > 0)
                {
                    averageThroughput = _outerInstance._counter / (((double)totalTimePassed) / 1000);
                }
                else
                {
                    averageThroughput = 0;
                }

                _outerInstance._textWriter.WriteLine("current: {0} " + _outerInstance._unit + "/s avg: {1} " + _outerInstance._unit + "/s total: {2} " + _outerInstance._unit + "%n", currentThroughput, averageThroughput, _outerInstance._counter);

                _lastTimeStamp = DateTime.Now.Ticks;
                _lastCount = _outerInstance._counter;
            }
        }

        /*
               public virtual void stopAndPrintFinalResult()
              {

                if (!Started)
                {
                  throw new IllegalStateException("Must be started first!");
                }

                if (beeperHandle != null)
                {
                  // yeah we have time to finish current
                  // printing if there is one
                  beeperHandle.cancel(false);
                }

                scheduler.shutdown();

                long timePassed = DateTimeHelperClass.CurrentUnixTimeMillis() - startTime;

                double average;
                if (timePassed > 0)
                {
                  average = counter / (timePassed / 1000d);
                }
                else
                {
                  average = 0;
                }

                @out.println();
                @out.println();

                @out.printf("Average: %.1f " + unit + "/s %n", average);
                @out.println("Total: " + counter + " " + unit);
                @out.println("Runtime: " + timePassed / 1000d + "s");
              } */

        internal void stopAndPrintFinalResult()
        {
            throw new System.NotImplementedException();
        }
	}

}