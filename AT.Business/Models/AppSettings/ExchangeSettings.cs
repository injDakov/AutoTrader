using System.Collections.Generic;

namespace AT.Business.Models.AppSettings
{
    public class ExchangeSettings
    {
        public string Name { get; set; }

        public Client Client { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<Pair> Pairs { get; set; }
    }
}