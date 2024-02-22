using AT.Business.Models;
using AT.Business.Models.Exchange;
using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects.Models;

namespace AT.Business.AutoMapper
{
    public class ExchangeToPrivateProfile : Profile
    {
        public ExchangeToPrivateProfile()
        {
            // Bitfinex
            CreateMap<BitfinexOrder, Order>()
                .ForMember(o => o.Amount, opt => opt.MapFrom(bo => bo.Quantity)) // TODO: Rename the property Amount to Quantity
                .ForMember(o => o.AmountOriginal, opt => opt.MapFrom(bo => bo.Quantity)); // TODO: Remove the property AmountOriginal

            CreateMap<BitfinexOrder, PlacedOrder>()
                .ForMember(pl => pl.OrderId, opt => opt.MapFrom(bo => bo.Id))
                .ForMember(pl => pl.CreateDate, opt => opt.MapFrom(bo => bo.CreateTime))
                .ForMember(pl => pl.Amount, opt => opt.MapFrom(bo => bo.Quantity))
                .ForMember(pl => pl.AmountOriginal, opt => opt.MapFrom(bo => bo.Quantity));

            CreateMap<BitfinexSymbolOverview, SymbolOverview>();

            CreateMap<OrderSide, OrderSideEnum>()
                .ConvertUsingEnumMapping(opt => opt.MapByName());

            // Binance
            // TODO:
        }
    }
}