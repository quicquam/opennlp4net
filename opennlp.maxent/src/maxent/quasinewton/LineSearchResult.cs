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
namespace opennlp.maxent.quasinewton
{

	/// <summary>
	/// class to store lineSearch result
	/// </summary>
	public class LineSearchResult
	{
	  public static LineSearchResult getInitialObject(double valueAtX, double[] gradAtX, double[] x, int maxFctEval)
	  {
		return new LineSearchResult(0.0, 0.0, valueAtX, null, gradAtX, null, x, maxFctEval);
	  }

	  public static LineSearchResult getInitialObject(double valueAtX, double[] gradAtX, double[] x)
	  {
		return new LineSearchResult(0.0, 0.0, valueAtX, null, gradAtX, null, x, QNTrainer.DEFAULT_MAX_FCT_EVAL);
	  }

	  private int fctEvalCount;
	  private double stepSize;
	  private double valueAtCurr;
	  private double valueAtNext;
	  private double[] gradAtCurr;
	  private double[] gradAtNext;
	  private double[] currPoint;
	  private double[] nextPoint;

	  public LineSearchResult(double stepSize, double valueAtX, double valurAtX_1, double[] gradAtX, double[] gradAtX_1, double[] currPoint, double[] nextPoint, int fctEvalCount)
	  {
		this.stepSize = stepSize;
		this.valueAtCurr = valueAtX;
		this.valueAtNext = valurAtX_1;
		this.gradAtCurr = gradAtX;
		this.gradAtNext = gradAtX_1;
		this.currPoint = currPoint;
		this.nextPoint = nextPoint;
		this.FctEvalCount = fctEvalCount;
	  }

	  public virtual double StepSize
	  {
		  get
		  {
			return stepSize;
		  }
		  set
		  {
			this.stepSize = value;
		  }
	  }
	  public virtual double ValueAtCurr
	  {
		  get
		  {
			return valueAtCurr;
		  }
		  set
		  {
			this.valueAtCurr = value;
		  }
	  }
	  public virtual double ValueAtNext
	  {
		  get
		  {
			return valueAtNext;
		  }
		  set
		  {
			this.valueAtNext = value;
		  }
	  }
	  public virtual double[] GradAtCurr
	  {
		  get
		  {
			return gradAtCurr;
		  }
		  set
		  {
			this.gradAtCurr = value;
		  }
	  }
	  public virtual double[] GradAtNext
	  {
		  get
		  {
			return gradAtNext;
		  }
		  set
		  {
			this.gradAtNext = value;
		  }
	  }
	  public virtual double[] CurrPoint
	  {
		  get
		  {
			return currPoint;
		  }
		  set
		  {
			this.currPoint = value;
		  }
	  }
	  public virtual double[] NextPoint
	  {
		  get
		  {
			return nextPoint;
		  }
		  set
		  {
			this.nextPoint = value;
		  }
	  }
	  public virtual int FctEvalCount
	  {
		  get
		  {
			return fctEvalCount;
		  }
		  set
		  {
			this.fctEvalCount = value;
		  }
	  }
	}
}