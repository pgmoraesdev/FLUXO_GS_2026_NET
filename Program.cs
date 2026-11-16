using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Fluxo_GS_2026_2TDSPR.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"), b => b.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

var app = builder.Build();

builder.Services.AddEndpointsApiExplorer();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();