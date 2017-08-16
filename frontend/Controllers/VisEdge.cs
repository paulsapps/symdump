﻿using Newtonsoft.Json;

namespace frontend.Controllers
{
    public class VisEdge
    {
        public class VisSmooth
        {
            [JsonProperty("type")]
            public string Type { get; set; } = "cubicBezier";
        }

        [JsonIgnore] public VisNode From;

        [JsonProperty("from")]
        public string FromId => From.Id;

        [JsonIgnore] public VisNode To;

        [JsonProperty("to")]
        public string ToId => To.Id;

        [JsonProperty("arrows")]
        public string Arrows { get; set; } = "to";

        [JsonProperty("physics")]
        public bool Physics { get; set; } = false;

        [JsonProperty("smooth")]
        public VisSmooth Smooth { get; set; } = new VisSmooth();
    }
}
