using System;
using Galaxpeer;
namespace Galaxpeer
{
	public class UnityInterfaceInterfaceManager
	{
		private static object syncRoot = new Object();
		private static bool unityInstance=true;
		public static UnityInterfaceInterface InstanceUnintyInterface
		{
			get 
			{
				if (InstanceUnintyInterface == null) 
				{
					lock (syncRoot) 
					{
						if (InstanceUnintyInterface == null) {
							if (unityInstance) {
								InstanceUnintyInterface = new UnityUnityInterface ();
							} else
								InstanceUnintyInterface = new DumyUnityInferace ();
						}
					}
				}

				return instance;
			}
		}
	}
}

