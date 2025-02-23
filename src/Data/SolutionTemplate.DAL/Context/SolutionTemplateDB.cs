using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SolutionTemplate.DAL.Context;

// Наследуем от класса со встроенной системой идентификации
public class SolutionTemplateDB : IdentityDbContext 
{
    public SolutionTemplateDB(DbContextOptions<SolutionTemplateDB> options) : base(options)
    {
            
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        string adminName = "admin";
        string adminEmail = "admin@admin.com";
        string roleAdminId = "E0980EC5-839D-4A8A-9A66-40CDBE72D973";
        string userAdminId = "B8F1E18D-23CA-49CA-98DE-DB5262DB66FE";

        // Добавляем роль администратора
        model.Entity<IdentityRole>().HasData(new IdentityRole()
        {
            Id = roleAdminId,
            Name = adminName,
            NormalizedName = adminName.ToUpper()
        });

        // Добавляем нового IdentityUser в качестве администратора сайта
        model.Entity<IdentityUser>().HasData(new IdentityUser()
        {
            Id = userAdminId,
            UserName = adminName,
            NormalizedUserName = adminName.ToUpper(),
            Email = adminEmail,
            NormalizedEmail = adminEmail,
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(new IdentityUser(), adminName),
            SecurityStamp = string.Empty,
            PhoneNumberConfirmed = true
        });

        // Связываем пользователя с ролью
        model.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = roleAdminId,
            UserId = userAdminId
        });
    }
}