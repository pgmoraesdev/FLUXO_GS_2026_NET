using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Fluxo_GS_2026_2TDSPR.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"), b => b.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();