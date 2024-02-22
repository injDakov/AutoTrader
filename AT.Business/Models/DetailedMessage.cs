using AT.Domain;

namespace AT.Business.Models
{
    public class DetailedMessage
    {
        public string Text { get; set; }

        public Order NewOrder { get; set; }
    }
}