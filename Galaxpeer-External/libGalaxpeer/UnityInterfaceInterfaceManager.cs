﻿using System;
using Galaxpeer;
namespace Galaxpeer
{
	public class UnityInterfaceInterfaceManager
	{
		private static bool unityInstance=true;
		private static UnityInterfaceInterface backingUnityInterface;
		public static UnityInterfaceInterface InstanceUnintyInterface
		{
			get 
			{
				if (backingUnityInterface == null) 
					{
						if (unityInstance) {
							backingUnityInterface = new UnityUnityInterface ();
						} else {
							backingUnityInterface = new DumyUnityInterface ();
						}
					}
				
	
			return backingUnityInterface;

			}
		}

	}
}
