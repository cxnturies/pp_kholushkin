using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
	public class AuthenticationManager : IAuthenticationManager
	{
		private readonly UserManager<User> _userManager;
		private readonly UserManager<Customer> _customerManager;
		private readonly IConfiguration _configuration;
		private User _user;
		private Customer _customer;

		public AuthenticationManager(UserManager<User> userManager, UserManager<Customer> customerManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_configuration = configuration;
			_customerManager = customerManager;
		}

		public async Task<bool> ValidateUser(UserForAuthenticationDto userForAuth)
		{
			_user = await _userManager.FindByNameAsync(userForAuth.UserName);
			return (_user != null && await _userManager.CheckPasswordAsync(_user, userForAuth.Password));
		}

		public async Task<string> CreateToken()
		{
			var signingCredentials = GetSigningCredentials();
			var claims = await GetClaims();
			var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
			return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
		}

		private SigningCredentials GetSigningCredentials()
		{
			var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"));
			var secret = new SymmetricSecurityKey(key);
			return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
		}

		private async Task<List<Claim>> GetClaims()
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, _user.UserName)
			};
			var roles = await _userManager.GetRolesAsync(_user);
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}
			return claims;
		}

		private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
		{
			var jwtSettings = _configuration.GetSection("JwtSettings");
			var tokenOptions = new JwtSecurityToken
			(
				issuer: jwtSettings.GetSection("validIssuer").Value,
				audience: jwtSettings.GetSection("validAudience").Value,
				claims: claims,
				expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("expires").Value)
			), signingCredentials: signingCredentials);
			return tokenOptions;
		}

		public async Task<bool> ValidateCustomer(CustomerForAuthenticationDto customerForAuth)
		{
			_customer = await _customerManager.FindByNameAsync(customerForAuth.UserName);
			return (_customer != null && await _customerManager.CheckPasswordAsync(_customer, customerForAuth.Password));
		}

		public async Task<string> CreateTokenCustomer()
		{
			var signingCredentials = GetSigningCredentials();
			var claims = await GetClaimsCustomer();
			var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
			return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
		}

		private async Task<List<Claim>> GetClaimsCustomer()
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, _customer.UserName)
			};
			var roles = await _customerManager.GetRolesAsync(_customer);
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}
			return claims;
		}
	}
}