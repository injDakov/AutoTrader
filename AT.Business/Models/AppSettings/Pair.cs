using System;

namespace AT.Business.Models.AppSettings
{
    public class Pair
    {
        private string _name;
        private decimal _orderAmount;
        private int _maxOrderLevelCount;

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings ExchangeSettings Pair.Name' it must not be empty !");
                }

                _name = value;
            }
        }

        public decimal OrderAmount
        {
            get
            {
                return _orderAmount;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings ExchangeSettings Pair.OrderAmount' has to be bigger than '0', but the current value is '{value}' !");
                }

                _orderAmount = value;
            }
        }

        public int MaxOrderLevelCount
        {
            get
            {
                return _maxOrderLevelCount;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings ExchangeSettings Pair.MaxOrderLevelCount' has to be '0' or bigger, but the current value is '{value}' !");
                }

                _maxOrderLevelCount = value;
            }
        }

        public bool IsActive { get; set; }
    }
}