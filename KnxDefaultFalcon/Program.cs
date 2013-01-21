using Knx.Infrastructure.DataTypes;
using Knx.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnxDefaultFalcon
{
	class Program
	{
		static void Main(string[] args)
		{
			KnxServer.Run((gate, write) =>
			{
				Console.WriteLine("Send test data to KNX: (XX/XX/XX VALUE)");
				while(true) {
					var input = Console.ReadLine().Split(' ');
					if (input.Count() != 2) {
						Console.WriteLine("Bad input! (XX/XX/XX VALUE)");
					}
					try
					{
						var address = EnmxAddress.Parse(input[0]);
						var value = int.Parse(input[1]);
						write.Write(address, value);
						Console.WriteLine("OK");
					}
					catch (Exception e)
					{
						Console.WriteLine("Failed to write, " + e.Message);
					}
				}
			}, false);
		}
	}
}
