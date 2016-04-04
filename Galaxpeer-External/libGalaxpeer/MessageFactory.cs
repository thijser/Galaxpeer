
using System;
using System.Collections;
using System.Reflection;

namespace Galaxpeer
{
	public class MessageFactory
	{
		static Hashtable constructors = new Hashtable ();

		public static Message Parse(byte[] bytes)
		{
			Type type = (Type)constructors [bytes [0]];
			Type[] argTypes = new Type[1];
			argTypes [0] = typeof(byte[]);
			ConstructorInfo constructor = type.GetConstructor(argTypes);
			Object[] args = new Object[1];
			args [0] = bytes;
			return (Message) constructor.Invoke(args);
		}

		public static void Register(char id, Type type)
		{
			constructors.Add ((byte) ((int) id), type);
		}

		static MessageFactory()
		{
			Register ('C', typeof(ConnectionMessage));
			Register ('L', typeof(LocationMessage));
			Register ('R', typeof(RequestConnectionsMessage));
			Register ('D', typeof(DestroyMessage));
			Register ('H', typeof(HandoverMessage));
		}
	}
}
