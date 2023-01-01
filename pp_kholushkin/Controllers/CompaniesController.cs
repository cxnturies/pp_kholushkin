﻿using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using AutoMapper;
using Entities.DataTransferObjects;

namespace pp_kholushkin.Controllers
{
	[Route("api/companies")]
	[ApiController]
	public class CompaniesController : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;

		public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
		}
		[HttpGet]
		public IActionResult GetCompanies()
		{
			var companies = _repository.Company.GetAllCompanies(trackChanges: false);
			var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
			return Ok(companiesDto);
		}
	}
}
