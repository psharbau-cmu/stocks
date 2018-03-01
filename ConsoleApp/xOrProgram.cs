using System;
using System.IO;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class xOrProgram
    {
        static void Main(string[] args)
        {
            NetDescription description = null;

            using (var reader = new StreamReader(File.OpenRead("xOrNet.json")))
            {
                var text = reader.ReadToEnd();
                description = JsonConvert.DeserializeObject<NetDescription>(text);
            }

            var net = Net.FromDescription(description);

            Console.WriteLine(net.GetTrainingFunctionCode());

            Console.ReadKey();
        }
    }
}
