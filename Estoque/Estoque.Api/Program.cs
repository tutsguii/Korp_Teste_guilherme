using BuildingBlocks.SharedKernel;
using BuildingBlocks.SharedKernel.Messaging;
using Estoque.Api.Configuration;
using Estoque.Api.Contracts;
using Estoque.Api.Consumers;
using Estoque.Api.Validators;
using Estoque.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Values
            .SelectMany(x => x.Errors)
            .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Requisicao invalida." : x.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Message = "Falha de validacao.",
            Errors = errors,
            TraceId = context.HttpContext.TraceIdentifier
        });
    };
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<CriarProdutoRequest>, CriarProdutoRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMq:UserName"]}:{builder.Configuration["RabbitMq:Password"]}@{builder.Configuration["RabbitMq:HostName"]}:5672");

builder.Services.AddSingleton<IRabbitMqService>(_ =>
    new RabbitMqService(
        builder.Configuration["RabbitMq:HostName"]!,
        builder.Configuration["RabbitMq:UserName"]!,
        builder.Configuration["RabbitMq:Password"]!));

builder.Services.AddHostedService<NotaFechamentoSolicitadoConsumer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.UseGlobalExceptionHandling();
app.MapControllers();
app.MapHealthChecks("/health");

await DbInitializer.SeedAsync(app);

app.Run();
