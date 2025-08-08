using JamesEsg.Common.Library.Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JamesEsg.Identity.API.Context;

public class ApplicationDbContext : IdentityDbContext<ApiUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}