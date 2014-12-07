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
using j4n.IO.OutputStream;

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

/*
	  private ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);

	  private ScheduledFuture beeperHandle;

	  private readonly PrintStream @out;

	  public PerformanceMonitor(PrintStream @out, string unit)
	  {
		this.@out = @out;
		this.unit = unit;
	  }

	  public PerformanceMonitor(string unit) : this(System.out, unit)
	  {
	  }


	  public virtual void startAndPrintThroughput()
	  {

		start();

		Runnable beeper = new RunnableAnonymousInnerClassHelper(this);

	   beeperHandle = scheduler.scheduleAtFixedRate(beeper, 1, 1, TimeUnit.SECONDS);
	  }

	  private class RunnableAnonymousInnerClassHelper : Runnable
	  {
		  private readonly PerformanceMonitor outerInstance;

		  public RunnableAnonymousInnerClassHelper(PerformanceMonitor outerInstance)
		  {
			  this.outerInstance = outerInstance;
			  lastTimeStamp = outerInstance.startTime;
			  lastCount = outerInstance.counter;
		  }


		  private long lastTimeStamp;
		  private int lastCount;

		  public virtual void run()
		  {

			int deltaCount = outerInstance.counter - lastCount;

			long timePassedSinceLastCount = DateTimeHelperClass.CurrentUnixTimeMillis() - lastTimeStamp;

			double currentThroughput;

			if (timePassedSinceLastCount > 0)
			{
			  currentThroughput = deltaCount / ((double) timePassedSinceLastCount / 1000);
			}
			else
			{
			  currentThroughput = 0;
			}

			long totalTimePassed = DateTimeHelperClass.CurrentUnixTimeMillis() - outerInstance.startTime;

			double averageThroughput;
			if (totalTimePassed > 0)
			{
			  averageThroughput = outerInstance.counter / (((double) totalTimePassed) / 1000);
			}
			else
			{
			  averageThroughput = 0;
			}

			outerInstance.@out.printf("current: %.1f " + outerInstance.unit + "/s avg: %.1f " + outerInstance.unit + "/s total: %d " + outerInstance.unit + "%n", currentThroughput, averageThroughput, outerInstance.counter);

			lastTimeStamp = DateTimeHelperClass.CurrentUnixTimeMillis();
			lastCount = outerInstance.counter;
		  }
	  }

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

	    public void startAndPrintThroughput()
	    {
	        throw new System.NotImplementedException();
	    }
	}

}