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
			CreateMap<Employee, EmployeeDto>();
			CreateMap<CompanyForCreationDto, Company>();
			CreateMap<EmployeeForCreationDto, Employee>();
			CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();
			CreateMap<CompanyForUpdateDto, Company>();
			CreateMap<UserForRegistrationDto, User>();

			CreateMap<Order, OrderDto>().ForMember(c => c.DateAndTime, opt => opt.MapFrom(x => string.Join(' ', x.Date, x.Time)));
			CreateMap<Product, ProductDto>();
			CreateMap<OrderForCreationDto, Order>();
			CreateMap<ProductForCreationDto, Product>();
			CreateMap<ProductForUpdateDto, Product>().ReverseMap();
			CreateMap<OrderForUpdateDto, Order>();
			CreateMap<CustomerForRegistrationDto, Customer>();
		}
	}
}