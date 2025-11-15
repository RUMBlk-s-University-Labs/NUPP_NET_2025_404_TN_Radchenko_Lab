using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Polls.Common;
using Polls.Infrastructure;
using Polls.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PollsContext>(options =>
    options.UseNpgsql(connectionString));

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

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PollsContext>();
        await dbContext.Database.MigrateAsync();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
