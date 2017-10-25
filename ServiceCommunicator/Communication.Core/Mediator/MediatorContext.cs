using System;

namespace Communication.Core.Mediator
{
	public class MediatorContext
	{
		public Type InterfaceType { get; set; }

		public string InterfaceName => this.InterfaceType.Name;

		public string Method { get; set; }

		public Type ArgumentType { get; set; }

		//public Type ReturnType { get; set; }

		public Delegate Execute { get; set; }
	}
}
