using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_TestProject.Models
{
    class Markets
    {
        public string exchangeId { get; set; }
        public string rank { get; set; }

        public string baseSymbol { get; set; }
        public string baseId { get; set; }

        public string quoteSymbol { get; set; }
        public string quoteId { get; set; }

        public string priceQuote { get; set; }
        public decimal priceUsd { get; set; }
        public double? volumeUsd24Hr { get; set; }

        public string percentExchangeVolume { get; set; }

        public double? tradesCount24Hr { get; set; }

        public long updated { get; set; }

        public double? volumePercent { get; set; }
        public Markets[] data { get; set; }

    }
}
