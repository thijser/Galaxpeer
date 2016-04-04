using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Galaxpeer
{
	public delegate void MessageHandler<T>(T message);
	
	public abstract class Message
	{
		public static sbyte MAX_HOPS = 2;

		public virtual IPEndPoint SourceIp { get; set; }
		public Client SourceClient;
		public sbyte Hops;
		public long Timestamp;

		public Message()
		{
			Hops = MAX_HOPS;
			Timestamp = DateTime.UtcNow.Ticks;
		}

		public byte[] StringToBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		public string BytesToString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		// Shamelessly copied from https://stackoverflow.com/questions/3278827/how-to-convert-a-structure-to-a-byte-array-in-c
		public byte[] ToBytes(object str)
		{
			int size = Marshal.SizeOf(str);
			byte[] arr = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(str, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);

			return arr;
		}

		public T FromBytes<T>(byte[] arr)
		{
			T str = default(T);

			int size = Marshal.SizeOf(str);
			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.Copy(arr, 0, ptr, size);

			str = (T)Marshal.PtrToStructure(ptr, str.GetType());
			Marshal.FreeHGlobal(ptr);

			return str;
		}

		public abstract byte[] Serialize();
		public abstract void Dispatch();
	}

	public abstract class TMessage<T> : Message where T : TMessage<T>
	{
		public static event MessageHandler<T> OnReceive;

		public override void Dispatch()
		{
			if (OnReceive != null) {
				OnReceive ((T)this);
			}
		}
	}
}
