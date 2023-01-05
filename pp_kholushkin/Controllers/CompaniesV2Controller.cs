﻿using Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace pp_kholushkin.Controllers
{
	[Route("api/companies")]
	[ApiController]
	public class CompaniesV2Controller : ControllerBase
	{
		private readonly IRepositoryManager _repository;
		public CompaniesV2Controller(IRepositoryManager repository)
		{
			_repository = repository;
		}

		[HttpGet]
		public async Task<IActionResult> GetCompanies()
		{
			var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);
			return Ok(companies);
		}
	}
}