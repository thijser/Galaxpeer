using System;
using UnityEngine;
using Galaxpeer;

public static class Conversion
{
	public static UnityEngine.Vector3 ToUnity (Galaxpeer.Vector3 g)
	{
		return new UnityEngine.Vector3 (g.X, g.Y, g.Z);
	}

	public static UnityEngine.Quaternion ToUnity (Galaxpeer.Quaternion g)
	{
		return new UnityEngine.Quaternion (g.X, g.Y, g.Z, g.W);
	}
}

