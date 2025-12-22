using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Polls.Common;
using Polls.Infrastructure;
using Polls.Infrastructure.Models;
using Polls.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PollsContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentityApiEndpoints<PersonModel>()
    .AddRoles<IdentityRole<Guid>>() 
    .AddEntityFrameworkStores<PollsContext>();

builder.Services.AddScoped<PersonRepository>();
builder.Services.AddScoped<OptionRepository>();

builder.Services.AddScoped<IRepository<Person>>(provider => 
    provider.GetRequiredService<PersonRepository>());
builder.Services.AddScoped<IRepository<Option>>(provider => 
    provider.GetRequiredService<OptionRepository>());

builder.Services.AddScoped<IRepository<Poll>, PollRepository>();
builder.Services.AddScoped<IRepository<SingleVotePoll>, SingleVotePollRepository>();
builder.Services.AddScoped<IRepository<RankedPoll>, RankedPollRepository>();

builder.Services.AddScoped<ICrudServiceAsync<Person>, GenericCrudServiceAsync<Person>>();
builder.Services.AddScoped<ICrudServiceAsync<Option>, GenericCrudServiceAsync<Option>>();
builder.Services.AddScoped<ICrudServiceAsync<SingleVotePoll>, GenericCrudServiceAsync<SingleVotePoll>>();
builder.Services.AddScoped<ICrudServiceAsync<RankedPoll>, GenericCrudServiceAsync<RankedPoll>>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введіть токен у форматі: Bearer {ваш_токен}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();


var app = builder.Build();
app.MapGroup("/identity").MapIdentityApi<PersonModel>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PollsContext>();
    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PersonModel>>();
    
    string[] roleNames = { "Admin", "Moderator", "Voter" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    var allUsers = await userManager.Users.ToListAsync();
    foreach (var user in allUsers)
    {
        if (string.IsNullOrEmpty(user.Name))
        {
            user.Name = user.Email.Split('@')[0];
            await userManager.UpdateAsync(user);
        }

        if (string.IsNullOrEmpty(user.SecurityStamp))
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        var currentRoles = await userManager.GetRolesAsync(user);

        Console.WriteLine(!currentRoles.Contains("Moderator"));
        if (user.Name == "admin" && !currentRoles.Contains("Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
        else if (user.Name == "moderator" && !currentRoles.Contains("Moderator"))
        {
            await userManager.AddToRoleAsync(user, "Moderator");
        }

        var updatedRoles = await userManager.GetRolesAsync(user);
        if (updatedRoles.Count == 0)
        {
            await userManager.AddToRoleAsync(user, "Voter");
        }
    }
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
