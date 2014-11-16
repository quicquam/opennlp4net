using System;
using System.Threading;

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

namespace opennlp.tools.util.ext
{

	using BundleActivator = org.osgi.framework.BundleActivator;
	using BundleContext = org.osgi.framework.BundleContext;
	using Filter = org.osgi.framework.Filter;
	using FrameworkUtil = org.osgi.framework.FrameworkUtil;
	using InvalidSyntaxException = org.osgi.framework.InvalidSyntaxException;
	using ServiceTracker = org.osgi.util.tracker.ServiceTracker;

	/// <summary>
	/// OSGi bundle activator which can use an OSGi service as
	/// an OpenNLP extension.
	/// <para>
	/// <b>Note:</b> Do not use this class, internal use only!
	/// </para>
	/// </summary>
	public class OSGiExtensionLoader : BundleActivator
	{

	  private static OSGiExtensionLoader instance;

	  private BundleContext context;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void start(org.osgi.framework.BundleContext context) throws Exception
	  public virtual void start(BundleContext context)
	  {
		instance = this;
		this.context = context;

		ExtensionLoader.setOSGiAvailable();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void stop(org.osgi.framework.BundleContext context) throws Exception
	  public virtual void stop(BundleContext context)
	  {
		instance = null;
		this.context = null;
	  }

	  /// <summary>
	  /// Retrieves the 
	  /// </summary>
	  /// <param name="clazz"> </param>
	  /// <param name="id">
	  /// @return </param>
	  internal virtual T getExtension<T>(Type clazz, string id)
	  {

		if (context == null)
		{
		  throw new IllegalStateException("OpenNLP Tools Bundle is not active!");
		}

		Filter filter;
		try
		{
		  filter = FrameworkUtil.createFilter("(&(objectclass=" + clazz.Name + ")(" + "opennlp" + "=" + id + "))");
		}
		catch (InvalidSyntaxException e)
		{
		  // Might happen when the provided IDs are invalid in some way.
		  throw new ExtensionNotLoadedException(e);
		}

		// NOTE: In 4.3 the parameters are <T, T>
		ServiceTracker extensionTracker = new ServiceTracker(context, filter, null);

		T extension = null;

		try
		{
		  extensionTracker.open();

		  try
		  {
			extension = (T) extensionTracker.waitForService(30000);
		  }
		  catch (InterruptedException)
		  {
			Thread.CurrentThread.Interrupt();
		  }
		}
		finally
		{
		  extensionTracker.close();
		}

		if (extension == null)
		{
		  throw new ExtensionNotLoadedException("No suitable extension found. Extension name: " + id);
		}

		return extension;
	  }

	  internal static OSGiExtensionLoader Instance
	  {
		  get
		  {
			return instance;
		  }
	  }
	}

}