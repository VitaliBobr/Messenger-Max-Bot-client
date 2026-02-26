# MaxBot – .NET client for MAX Messenger API

[![NuGet version](https://img.shields.io/nuget/v/MaxBot)](https://www.nuget.org/packages/MaxBot)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![Build](https://github.com/your-username/MaxBot/actions/workflows/build.yml/badge.svg)](https://github.com/your-username/MaxBot/actions)

**MaxBot** is a fully typed, asynchronous .NET library to interact with the [MAX Messenger API](https://dev.max.ru).  
It simplifies building bots for MAX by providing a clean, testable interface and handling all the low‑level HTTP details.

Built on top of **Refit** and **Dusharp**, it offers:
- First‑class support for discriminated unions (updates, attachments, keyboards).
- Complete async/await pattern.
- Full test coverage with `MockHttp`.
- Easy to extend and integrate.

---

## ✨ Features

- ✅ **Full API coverage** – bots, chats, messages, subscriptions, uploads.
- ✅ **Strongly typed models** – no magic strings, full IntelliSense.
- ✅ **Discriminated unions** with `Match` for safe handling of different update/attachment types.
- ✅ **Flexible JSON serialization** – custom converters for exact API compliance.
- ✅ **Testable** – mock HTTP handlers with `MockHttpMessageHandler`.
- ✅ **Dependency injection ready** – register once, use everywhere.
- ✅ **Open‑source** – MIT or Apache 2.0 licensed (your choice).

---

## 📦 Installation

### NuGet package
```bash
dotnet add package MaxBot
Or via Package Manager:
```

## Powershell
```powershell
Install-Package MaxBot
```

🚀 Quick Start
## 1. Register the client (using Microsoft.Extensions.DependencyInjection)
``` csharp
using MaxBot.Client;
using Refit;

services.AddRefitClient<IMaxApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://platform-api.max.ru");
        c.DefaultRequestHeaders.Add("Authorization", "YOUR_BOT_TOKEN");
    });
```
## 2. Get bot information
``` csharp
public class MyService
{
    private readonly IMaxApi _api;
    public MyService(IMaxApi api) => _api = api;

    public async Task ShowBotInfoAsync()
    {
        var bot = await _api.GetMeAsync();
        Console.WriteLine($"Bot: {bot.FirstName} (@{bot.Username})");
        Console.WriteLine($"Description: {bot.Description}");
    }
}
```
## 3. Send a message to a chat
``` csharp
var message = new NewMessageBody
{
    Text = "Hello, MAX!",
    Notify = true
};

var response = await _api.SendMessageAsync(
    chatId: 123456789,
    request: message
);

Console.WriteLine($"Message sent with ID: {response.Message.Body.Mid}");
```
## 4. Handle incoming updates (webhook)
``` csharp
[HttpPost("webhook")]
public async Task<IActionResult> Webhook([FromBody] Update update)
{
    await update.Match(
        messageCreated: async (msg, locale) =>
        {
            Console.WriteLine($"New message: {msg.Body?.Text}");
            // reply, etc.
        },
        messageCallback: async (cb, _, _) =>
        {
            await _api.SendAnswerAsync(cb.CallbackId,
                new AnswerRequest { Notification = "Thanks for clicking!" });
        },
        // ... handle all other cases
        _ => Task.CompletedTask
    );
    return Ok();
}
```

📘 Documentation
Official MAX API Reference

Refit – HTTP client generator

Dusharp – discriminated unions for C#

For more examples and advanced usage, check the Wiki.

🧪 Testing
All methods are covered with unit tests using RichardSzalay.MockHttp.
To run tests:

``` bash
dotnet test
```
🤝 Contributing
Contributions are welcome! Please follow these steps:

Fork the repository.

Create a feature branch (git checkout -b feature/amazing).

Commit your changes (git commit -m 'Add some amazing feature').

Push to the branch (git push origin feature/amazing).

Open a Pull Request.

Make sure to add tests for any new functionality.

📄 License
This project is licensed under the Apache 2.0 License – see the LICENSE file for details.
You are free to use, modify, and distribute this library, even in commercial products, as long as you retain the copyright notice.

🙏 Acknowledgements
MAX for providing the API.

All the open‑source libraries that made this project possible.

⭐️ Support
If you find this library useful, please give it a ⭐ on GitHub – it helps others discover it.

For issues, questions, or feature requests, use the GitHub Issues.
