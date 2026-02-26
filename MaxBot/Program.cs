using MaxBot.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace MaxBot;


public interface IBotBehaviour
{
    void StartPolling();
    void StopPolling();
}

public class Bot
{
    private readonly string _apiKey;
    private readonly string _webhookUrl;
    private Task _pollingTask;
    private CancellationToken _cancellationTokenPolling;
    
    public Bot(string apiKey, string webhookUrl = "")
    {
        _apiKey = apiKey;
        _webhookUrl = webhookUrl;
    }

    public async Task StartPolling()
    {
        Task task = Task.Run(async() =>
        {
            if (_pollingTask != null)
                throw new Exception("Already started");
            _cancellationTokenPolling = new CancellationToken();

            var builder = WebApplication.CreateBuilder();

            builder.WebHost.ConfigureKestrel((option) =>
            {
                option.ListenLocalhost(5000);
            });

            // Add services to the container.
            // Add always manually because dont work as library.

            builder.Services.AddControllers()
                .AddApplicationPart(typeof(WeatherForecastController).Assembly);
            
            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();
        });

    }


    public void StopPolling()
    {
    }
}