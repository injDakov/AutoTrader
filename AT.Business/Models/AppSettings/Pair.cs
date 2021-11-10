namespace AT.Business.Models.AppSettings
{
    /// <summary>Pair class.</summary>
    public class Pair
    {
        /// <summary>Gets or sets the name for this pair.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the order amount for this pair.</summary>
        /// <value>The order amount.</value>
        public decimal OrderAmount { get; set; }

        /// <summary>Gets or sets the maximum order level for this pair.</summary>
        /// <value>The maximum order level.</value>
        public int MaxOrderLevel { get; set; }

        /// <summary>Gets or sets a value indicating whether this pair is active.</summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; set; }
    }
}