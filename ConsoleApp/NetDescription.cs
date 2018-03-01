using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    public class NetDescription
    {
        public NodeDescription[] Nodes { get; set; }
        public int[] Outputs { get; set; }
    }

    public class NodeDescription
    {
        public int NodeId { get; set; }
        public NodeInputDescription[] Inputs { get; set; }
        public string Aggregator { get; set; }
        public string Processor { get; set; }
    }
    public class NodeInputDescription
    {
        public bool FromInputVector { get; set; }
        public int InputId { get; set; }
        public float Weight { get; set; }
    }

}
