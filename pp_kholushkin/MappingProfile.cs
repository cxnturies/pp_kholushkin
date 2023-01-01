using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace pp_kholushkin
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Company, CompanyDto>().ForMember(c => c.FullAddress, opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
			CreateMap<Order, OrderDto>().ForMember(c => c.DateAndTime, opt => opt.MapFrom(x => string.Join(' ', x.Date, x.Time)));
		}
	}
}
