
#if !(_WINDOWS_CE) && !(_SILVERLIGHT) && !(WINDOWS_PHONE)

using System;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Web;


namespace Amib.Threading.Internal
{
#region CallerThreadContext class

	/// <summary>
	/// This class stores the caller call context in order to restore
	/// it when the work item is executed in the thread pool environment. 
	/// </summary>
	internal class CallerThreadContext 
	{
#region Prepare reflection information

		// Cached type information.
		private static readonly MethodInfo getLogicalCallContextMethodInfo =
			typeof(Thread).GetMethod("GetLogicalCallContext", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly MethodInfo setLogicalCallContextMethodInfo =
			typeof(Thread).GetMethod("SetLogicalCallContext", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion

#region Private fields

        #endregion

		/// <summary>
		/// Constructor
		/// </summary>
		private CallerThreadContext()
		{
		}
	}

    #endregion
}
#endif
