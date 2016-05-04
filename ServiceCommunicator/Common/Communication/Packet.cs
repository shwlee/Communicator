
namespace Common.Communication
{
	public class Packet
	{
		public int RequestHash { get; set; }

		public string InterfaceName { get; set; }

		public string MethodName { get; set; }

		public object Argument { get; set; }
	}
}
