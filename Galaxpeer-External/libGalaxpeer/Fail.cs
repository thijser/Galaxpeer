using System;
using System.Threading;
using System.Diagnostics;

namespace Galaxpeer
{
	public class Fail
	{
		const double fail_lambda = 1f / (1000 * 60 * 2);
		const double start_lambda = 3 * fail_lambda;
		
		private Timer failTimer;
		public Fail()
		{
			double time = generateRand (fail_lambda);
			Console.WriteLine ("{0} will fail in {1} milliseconds", LocalPlayer.Instance.Uuid, time);
			failTimer = new Timer (onFail, null, (int) Math.Round(time), Timeout.Infinite);
		}

		static Random rnd = new Random ();
		double generateRand(double lambda)
		{
			double random = rnd.NextDouble ();
			double time = -(1 / lambda) * Math.Log (1 - random);
			return time;
		}

		void onFail (object _)
		{
			failTimer.Dispose ();
			double time = generateRand (start_lambda);

			int id = Process.GetCurrentProcess ().Id;
			bool die = rnd.NextDouble () > .5;

			Console.WriteLine ("{0} fails {1} for {2} seconds", LocalPlayer.Instance.Uuid, die ? "permanently" : "temporarily", time / 1000);

			Process.Start ("start_suspend.sh",  id + " " + (time / 1000) + " " + (die ? 1 : 0));

			Game.Fail = new Fail ();
		}
	}
}
