using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_TestProject.Models
{
    class Exchanges
    {
        public string exchangeId { get; set; }
        public string rank { get; set; }
        public string percentTotalVolume { get; set; }
        public string name { get; set; }
        public decimal? volumeUsd { get; set; }
        public string tradingPairs { get; set; }
        public string socket { get; set; }
        public string exchangeUrl { get; set; }
        public string updated { get; set; }

        public Exchanges [] data { get; set; }
    }
}
