using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelAPUSettings.Backend
{
    internal class Handler
    {
        private IntelAdj _adj;

        public Handler()
        {
            _adj = IntelAdj.Open();
        }

        public void Register(Communication comm)
        {
            comm.ConnectedEvent += OnConnected;
            comm.ReceivedEvent += OnReceived;
        }

        void OnConnected(object sender, EventArgs e)
        {
            (sender as Communication).Send("connected");
        }

        void OnReceived(object sender, string message)
        {
            var comm = sender as Communication;
            string[] args = message.Split(' ');
            if (args.Length == 0)
                return;
            switch (args[0])
            {
                case "set-tdp":
                    {
                        Console.WriteLine($"[Handler] Setting TDP to {args[1]} W");
                        var tdp = float.Parse(args[1]);
                        _adj.StapmLimit_W = tdp;
                        _adj.FastLimit_W = tdp;
                        _adj.SlowLimit_W = tdp;
                    }
                    break;
                case "get-tdp":
                    {
                        _adj.RefreshTable();
                        var currentTdp = Math.Max(_adj.StapmLimit_W, Math.Max(_adj.FastLimit_W, _adj.SlowLimit_W));
                        var currentTdpInt = (int)Math.Round(currentTdp);
                        comm.Send($"tdp {currentTdpInt}");
                    }
                    break;
                case "get-tdp-limit":
                    {
                        Console.WriteLine("[Handler] Get TDP Limit");
                        _adj.RefreshTable();
                        var originStapmLimit = _adj.StapmLimit_W;
                        var originFastLimit = _adj.FastLimit_W;
                        var originSlowLimit = _adj.SlowLimit_W;
                        Console.WriteLine($"[Handler] Origin StapmLimit: {originStapmLimit} W, FastLimit: {originFastLimit} W, SlowLimit: {originSlowLimit} W");
                        var currentTdp = Math.Max(originStapmLimit, Math.Max(originFastLimit, originSlowLimit));
                        var currentTdpInt = (int)Math.Round(currentTdp);
                        comm.Send($"tdp {currentTdpInt}");
                        const float testMaxTdp = 400, testMinTdp = 0;
                        _adj.StapmLimit_W = testMaxTdp;
                        _adj.FastLimit_W = testMaxTdp;
                        _adj.SlowLimit_W = testMaxTdp;
                        _adj.RefreshTable();
                        Console.WriteLine($"[Handler] Max StapmLimit: {_adj.StapmLimit_W} W, FastLimit: {_adj.FastLimit_W} W, SlowLimit: {_adj.SlowLimit_W} W");
                        var maxTdp = Math.Max(_adj.StapmLimit_W, Math.Max(_adj.FastLimit_W, _adj.SlowLimit_W));
                        _adj.StapmLimit_W = testMinTdp;
                        _adj.FastLimit_W = testMinTdp;
                        _adj.SlowLimit_W = testMinTdp;
                        _adj.RefreshTable();
                        Console.WriteLine($"[Handler] Min StapmLimit: {_adj.StapmLimit_W} W, FastLimit: {_adj.FastLimit_W} W, SlowLimit: {_adj.SlowLimit_W} W");
                        var minTdp = Math.Min(_adj.StapmLimit_W, Math.Min(_adj.FastLimit_W, _adj.SlowLimit_W));
                        _adj.StapmLimit_W = originStapmLimit;
                        _adj.FastLimit_W = originFastLimit;
                        _adj.SlowLimit_W = originSlowLimit;
                        var maxTdpInt = (int)Math.Round(maxTdp);
                        var minTdpInt = (int)Math.Round(minTdp);
                        Console.WriteLine($"[Handler] Max TDP: {maxTdpInt} W, Min TDP: {minTdpInt} W");
                        comm.Send($"tdp-limit {maxTdpInt} {minTdpInt}");
                    }
                    break;
            }
        }
    }
}
