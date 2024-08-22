using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responese;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Constants = ServerLibrary.Helpers.Constants;

namespace ServerLibrary.Repositories.Implementations
{
	public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccount
	{
		//CREATE NEW USER
		public async Task<GeneralResponse> CreateAsync(Register user) //create new user
		{
			//check if user is null, this method will return a response GeneralResponse with state is false and a message
			if (user is null) return new GeneralResponse(false, "Model is empty");

			//call FindUserByEmail method to check email exists already?, Result will be saved to checkUser variable
			var checkUser = await FindUserByEmail(user.Email!);
			//check if user is not null => user is exists already and return a response with state is false
			if (checkUser != null) return new GeneralResponse(false, "User registered already.");

			//SAVE USER ON DATABASE
			//create new ApplicationUser with information from Register user object	
			//then call AddToDatabase method to save this object into database
			var applicationUser = await AddToDatabase(new ApplicationUser()
			{
				Fullname = user.Fullname,
				Email = user.Email,
				Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
			});

			//CHECK, CREATE AND ASSIGN ROLE
			//check Admin role exists in database already ?, use FirstOrDefaultAsync method to get first role match with "Admin" name.
			//find on SystemRole in database if Admin role exist already or no.
			var checkAdminRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.Equals(Constants.Admin));
			//if addmin role is not exist, need to create.
			if(checkAdminRole is null)
			{
				//create new SystemRole object with name is Admin, save it into database.
				var createAdminRole = await AddToDatabase(new SystemRole() { Name = Constants.Admin });
				//assign Admin role on new user by create new user role and save on database.
				await AddToDatabase(new UserRole() { RoleId = createAdminRole.Id, UserId = applicationUser.Id });
				//return a response with state is true, and message if the account is created successfully.
				return new GeneralResponse(true, "Account created!");
			}
			
			//find on SystemRole in database if User role exist or no 
			var checkUserRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.Equals(Constants.User));
			//create new SystemRole object is response to save the result.
			SystemRole response = new();
			//check if UserRole is not exist, need to create new role.
			if(checkUserRole is null)
			{
				//Create new User role and save on database
				response = await AddToDatabase(new SystemRole() { Name = Constants.User });
				//assign User role on new user.
				await AddToDatabase(new UserRole() { RoleId = response.Id, UserId = applicationUser.Id });
			} 
			else
			{
				//if User role is exist already, just assign this role on new user. 
				await AddToDatabase(new UserRole() { RoleId = checkUserRole.Id, UserId = applicationUser.Id });
			}
			//return the response with state is true and message.
			return new GeneralResponse(true, "Account created!");
		}

		//LOGIN
		public async Task<LoginResponse> SignInAsync(Login user)
		{
			if (user is null) return new LoginResponse(false, "Model is empty");

			//find user by email in ApplicationUser table
			var applicationUser = await FindUserByEmail(user.Email!);
			if (applicationUser is null) return new LoginResponse(false, "User not found");

			//verify password
			if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
				return new LoginResponse(false, "Email/Password not valid");

			//get user role by user id
			var getUserRole = await FindUserRole(applicationUser.Id);
			if (getUserRole is null) return new LoginResponse(false, "User role not found");

			//get role name by role id
			var getRoleName = await FindRoleName(getUserRole.RoleId);
			if (getRoleName is null) return new LoginResponse(false, "User role not found");

			//create jwt token and refresh token
			string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
			string refreshToken = GenerateRefreshToken();

			//Save the refresh token to the database
			var findUser = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UsertId == applicationUser.Id);
			if (findUser is not null)
			{
				findUser!.Token = refreshToken;
				await appDbContext.SaveChangesAsync();
			}
			else
			{
				await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UsertId = applicationUser.Id});
			}

			return new LoginResponse(true, "Login successfully", jwtToken, refreshToken);
        }

		//GENERATE TOKEN
		public string GenerateToken(ApplicationUser user, string role)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			var userClaims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Fullname!),
				new Claim(ClaimTypes.Email, user.Email!),
				new Claim(ClaimTypes.Role, role!)
			};

			var token = new JwtSecurityToken(
				issuer: config.Value.Issuer,
				audience: config.Value.Audience,
				claims: userClaims,
				expires: DateTime.Now.AddDays(1),
				signingCredentials: credentials
				);
			//generate token to use for user authentication in other api requests
			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		//FIND USER ROLE IN UserRole TABLE IN DATABASE
		private async Task<UserRole> FindUserRole(int userId) => await appDbContext.UserRoles.FirstOrDefaultAsync(_ => _.UserId == userId);

		//FIND ROLE NAME IN SystemRole TABLE IN DATABASE
		private async Task<SystemRole> FindRoleName(int roleId) => await appDbContext.SystemRoles.FirstOrDefaultAsync(_=> _.Id == roleId);

		private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		
		//this method will find the user in database by email.
		private async Task<ApplicationUser> FindUserByEmail(string email) =>
			//query the database to find user with match email, ToLower() makes comparisons case-insensitive.
			await appDbContext.ApplicationUsers.FirstOrDefaultAsync(_ => _.Email!.ToLower()!.Equals(email!.ToLower()));

		//add any entities on database
		private async Task<T> AddToDatabase<T>(T model)
		{
			//add model entity on DbContext. '!' allow to skip null check.
			var result = appDbContext.Add(model!);
			//save changes on database
			await appDbContext.SaveChangesAsync();
			//return the entity saved on database.
			return (T)result.Entity;
		}

		public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
		{
			if (token is null) return new LoginResponse(false, "Model is empty.");

			var findToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.Token!.Equals(token.Token));
			if (findToken is null) return new LoginResponse(false, "Refresh token is required") ;

			//GET USER DETAILS
			var user = await appDbContext.ApplicationUsers.FirstOrDefaultAsync(_=> _.Id == findToken.Id);
			if (user is null) return new LoginResponse(false, "Refresh token could not be generated because user not found") ;

			var userRole = await FindUserRole(user.Id);
			var roleName = await FindRoleName(userRole.RoleId);
			string jwt = GenerateToken(user, roleName.Name!);
			string refreshToken = GenerateRefreshToken();

			var updateRefreshToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UsertId == user.Id);
			if (updateRefreshToken is null) return new LoginResponse(false, "Refresh token could not be generated because user has not signed in") ;

			updateRefreshToken.Token = refreshToken;
			await appDbContext.SaveChangesAsync();
			return new LoginResponse(true, "Refresh token successfully", jwt, refreshToken);
		}
	}
}
