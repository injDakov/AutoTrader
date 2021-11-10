namespace AT.Business.Models.AppSettings
{
    /// <summary>ProfitPercents class.</summary>
    public class ProfitPercents
    {
        /// <summary>Gets or sets the list of sell levels.</summary>
        /// <value>The list of sell levels.</value>
        public decimal[] SellLevels { get; set; }

        /// <summary>Gets or sets the list of buy levels.</summary>
        /// <value>The list of buy levels.</value>
        public decimal[] BuyLevels { get; set; }
    }
}