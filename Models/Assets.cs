﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT_TestProject.Models
{
    class Assets
    {
        public string id { get; set; }
        public string rank { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string supply { get; set; }
        public string maxSupply { get; set; }
        public string marketCapUsd { get; set; }
        public string volumeUsd24Hr { get; set; }
        public decimal priceUsd { get; set; }
        public string changePercent24Hr { get; set; }
        public string vwap24Hr { get; set; }

        public string explorer { get; set; }

        public DateTimeOffset dateTimeOffset { get; set; }
        public Assets[] data { get; set; }

    }

    class AssetsResponse
    {
        public Assets data { get; set; }
        public long timestamp { get; set; }

    }
    class SelectedAsset
    {
        public decimal priceUsd { get; set; }
        public long time { get; set; }

        public SelectedAsset[] data { get; set; }
    }
}
