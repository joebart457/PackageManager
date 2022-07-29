
using PackageManagerServer.Models.Entities;
using PackageManagerServer.Services;
using PackageManagerServer.Web;

namespace PackageManagerServer;

internal class Program
{
    static void Main(string[] args)
    {
        ContextService.BuildSafeListConnection("/usr/local/safelist/data.db");

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();
        app.UseMiddleware<AdminSafeListMiddleware>(ContextService.SafeListConnection.Select<IpEntryEntity>().Select(e => e.IpAddress).ToList());
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}
