using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MaxBot.Client;
using MaxBot.Models;
using MaxBot.Models.MaxRefitModels;
using MaxBot.Models.MaxRefitModels.Answers;
using MaxBot.Models.MaxRefitModels.Chat;
using MaxBot.Models.MaxRefitModels.Message;
using MaxBot.Models.MaxRefitModels.NewMessage;
using MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;
using MaxBot.Models.MaxRefitModels.Requests;
using MaxBot.Models.MaxRefitModels.Responces;
using MaxBot.Models.MaxRefitModels.Update;
using Refit;
using RichardSzalay.MockHttp;
using ChatType = MaxBot.Models.MaxRefitModels.Chat.ChatType;

namespace MaxBot.Test;

public class MaxApiTests
{
    // Настройки сериализации, как в реальном приложении
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters =
        {
            new ImageRequestPayloadConverter(), // для ImageRequestPayload
            new AttachmentConverter(),
            new AttachmentRequestConverter(),
            new ButtonConverter(),
            new ImageRequestPayloadConverter(),
            new UpdateConverter(),
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false)
            // добавьте сюда все остальные конвертеры, которые используются в проекте
            // например, для AttachmentRequest, Update и т.д.
        }
    };
    
    protected const string BaseUrl = "https://platform-api.max.ru";
    
    public class BotsGetMeTests : MaxApiTests{
        [Fact]
        public async Task GetMeAsync_ShouldReturnBotInfo_WhenResponseIsValid()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            // Подготавливаем ожидаемый JSON-ответ с правильными полями commands
            var expectedJson = @"{
                ""user_id"": 12345,
                ""first_name"": ""TestBot"",
                ""username"": ""test_bot"",
                ""is_bot"": true,
                ""last_activity_time"": 1612345678901,
                ""description"": ""This is a test bot"",
                ""avatar_url"": ""https://example.com/avatar_small.jpg"",
                ""full_avatar_url"": ""https://example.com/avatar_full.jpg"",
                ""commands"": [
                    { ""name"": ""start"", ""description"": ""Start the bot"" },
                    { ""name"": ""help"", ""description"": ""Show help"" }
                ]
            }";

            mockHttp.When(HttpMethod.Get, $"{BaseUrl}/me")
                    .Respond("application/json", expectedJson);

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseUrl)
            };

            var api = RestService.For<IMaxApi>(httpClient);

            // Act
            var result = await api.GetMeAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12345, result.UserId);
            Assert.Equal("TestBot", result.FirstName);
            Assert.Equal("test_bot", result.Username);
            Assert.True(result.IsBot);
            Assert.Equal("This is a test bot", result.Description);

            Assert.NotNull(result.Commands);
            Assert.Equal(2, result.Commands.Count);
            Assert.Equal("start", result.Commands[0].Name);
            Assert.Equal("Start the bot", result.Commands[0].Description);
            Assert.Equal("help", result.Commands[1].Name);
            Assert.Equal("Show help", result.Commands[1].Description);
        }

        [Fact]
        public async Task GetMeAsync_ShouldThrowApiException_WhenUnauthorized()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Get, $"{BaseUrl}/me")
                    .Respond(System.Net.HttpStatusCode.Unauthorized);

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseUrl)
            };
            var api = RestService.For<IMaxApi>(httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => api.GetMeAsync());
        }
    }


    public class ChatsMethods : MaxApiTests
    {
        public class GetChatsAsyncTests : ChatsMethods
        {
            [Fact]
            public async Task GetChatsAsync_ShouldReturnChatsAndMarker_WhenResponseIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();

                // Пример JSON-ответа из документации (подставьте реальные поля)
                var expectedJson = @"{
                    ""chats"": [
                        {
                            ""chat_id"": 123456,
                            ""type"": ""chat"",
                            ""title"": ""Team Chat""
                        },
                        {
                            ""chat_id"": 789012,
                            ""type"": ""chat"",
                            ""title"": ""Dev Group""
                        }
                    ],
                    ""marker"": 999999
                }";

                // Настраиваем мок: ожидаем GET /chats с любыми параметрами
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats*")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                var api = RestService.For<IMaxApi>(httpClient);

                // Act
                var result = await api.GetChatsAsync();

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Chats);
                Assert.Equal(2, result.Chats.Count);
                Assert.Equal(123456, result.Chats[0].ChatId);
                Assert.Equal("Team Chat", result.Chats[0].Title);
                Assert.Equal(789012, result.Chats[1].ChatId);
                Assert.Equal("Dev Group", result.Chats[1].Title);
                Assert.Equal(999999, result.Marker);
            }

            [Fact]
            public async Task GetChatsAsync_ShouldSendCorrectQueryParameters()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();

                // Переменная для захвата запрошенного URL
                string? capturedUrl = null;
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats*")
                        .Respond(req =>
                        {
                            capturedUrl = req.RequestUri?.ToString();
                            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                            {
                                Content = new StringContent("{\"chats\":[],\"marker\":null}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                var api = RestService.For<IMaxApi>(httpClient);

                // Act
                await api.GetChatsAsync(count: 30, marker: 12345);

                // Assert
                Assert.NotNull(capturedUrl);
                Assert.Contains("count=30", capturedUrl);
                Assert.Contains("marker=12345", capturedUrl);
            }

            [Fact]
            public async Task GetChatsAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats*")
                        .Respond(System.Net.HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                var api = RestService.For<IMaxApi>(httpClient);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetChatsAsync());
            }
        }
        
        public class GetChatByIdAsyncTests : ChatsMethods
        {
            [Fact]
            public async Task GetChatByIdAsync_ShouldReturnChat_WhenResponseIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                // Полный JSON-ответ из документации (с pinned_message)
                var expectedJson = @"{
                    ""chat_id"": 123456,
                    ""type"": ""chat"",
                    ""status"": ""active"",
                    ""title"": ""Team Chat"",
                    ""icon"": { ""url"": ""https://example.com/icon.jpg"" },
                    ""last_event_time"": 1612345678901,
                    ""participants_count"": 5,
                    ""owner_id"": 42,
                    ""participants"": null,
                    ""is_public"": false,
                    ""link"": ""https://t.me/joinchat/abc123"",
                    ""description"": ""Chat for team discussion"",
                    ""dialog_with_user"": null,
                    ""chat_message_id"": null,
                    ""pinned_message"": {
                        ""sender"": { ""user_id"": 100, ""first_name"": ""Alice"" },
                        ""recipient"": { ""chat_id"": 123456, ""chat_type"": ""chat"" },
                        ""timestamp"": 1612345600000,
                        ""body"": { ""text"": ""Important announcement!"" }
                    }
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                var api = RestService.For<IMaxApi>(httpClient);

                // Act
                var result = await api.GetGroupChatByIdAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(123456, result.ChatId);
                Assert.Equal("chat", result.Type.ToString().ToLower());
                Assert.Equal("active", result.Status.ToString().ToLower());
                Assert.Equal("Team Chat", result.Title);
                Assert.NotNull(result.Icon);
                Assert.Equal("https://example.com/icon.jpg", result.Icon.Url);
                Assert.Equal(1612345678901, result.LastEventTime);
                Assert.Equal(5, result.ParticipantsCount);
                Assert.Equal(42, result.OwnerId);
                Assert.False(result.IsPublic);
                Assert.Equal("https://t.me/joinchat/abc123", result.Link);
                Assert.Equal("Chat for team discussion", result.Description);
                Assert.NotNull(result.PinnedMessage);
                Assert.Equal(100, result.PinnedMessage.Sender?.UserId);
                Assert.Equal("Important announcement!", result.PinnedMessage.Body?.Text);
            }

            [Fact]
            public async Task GetChatByIdAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}")
                        .Respond(System.Net.HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                var api = RestService.For<IMaxApi>(httpClient);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetGroupChatByIdAsync(chatId));
            }

            [Fact]
            public async Task GetChatByIdAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}")
                        .Respond(System.Net.HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                var api = RestService.For<IMaxApi>(httpClient);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetGroupChatByIdAsync(chatId));
            }

            [Fact]
            public async Task GetChatByIdAsync_ShouldCallCorrectUrl()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                string? capturedUrl = null;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}")
                        .Respond(req =>
                        {
                            capturedUrl = req.RequestUri?.ToString();
                            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""chat_id"":123456}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
                var api = RestService.For<IMaxApi>(httpClient);

                // Act
                await api.GetGroupChatByIdAsync(chatId);

                // Assert
                Assert.NotNull(capturedUrl);
                Assert.Equal($"{BaseUrl}/chats/{chatId}", capturedUrl);
            }
        }
        
        
        public class UpdateGroupChatApiTests
        {
            private const string BaseUrl = "https://platform-api.max.ru";

            // Настройки сериализации, как в реальном приложении
            private static readonly JsonSerializerOptions _jsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters =
                {
                    new ImageRequestPayloadConverter(), // для ImageRequestPayload
                    // добавьте сюда все остальные конвертеры, которые используются в проекте
                    // например, для AttachmentRequest, Update и т.д.
                }
            };

            [Fact]
            public async Task UpdateGroupChatAsync_ShouldReturnUpdatedChat_WhenRequestIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var icon = ImageRequestPayload.FromUrl("https://example.com/new-icon.jpg");
                var request = new UpdateChatRequest
                {
                    Icon = icon,
                    Title = "New Chat Title",
                    Notify = false
                };

                // Сериализуем ожидаемое тело запроса с использованием наших настроек
                var expectedRequestBody = JsonSerializer.Serialize(request, _jsonOptions);

                var expectedResponseJson = @"{
                    ""chat_id"": 123456,
                    ""type"": ""chat"",
                    ""status"": ""active"",
                    ""title"": ""New Chat Title"",
                    ""last_event_time"": 1612345678901,
                    ""participants_count"": 5,
                    ""is_public"": false,
                    ""pinned_message"": null
                }";

                mockHttp.Expect(HttpMethod.Patch, $"{BaseUrl}/chats/{chatId}")
                        .WithContent(expectedRequestBody)
                        .Respond("application/json", expectedResponseJson);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                // Создаём Refit-клиент с нашими настройками сериализации
                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.UpdateGroupChatAsync(chatId, request);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(chatId, result.ChatId);
                Assert.Equal(ChatType.Chat, result.Type);
                Assert.Equal(ChatStatus.Active, result.Status);
                Assert.Equal("New Chat Title", result.Title);
                Assert.Equal(5, result.ParticipantsCount);
                Assert.False(result.IsPublic);

                mockHttp.VerifyNoOutstandingExpectation();
            }

            // Аналогично исправьте остальные тесты, добавив RefitSettings
            [Fact]
            public async Task UpdateGroupChatAsync_ShouldSendCorrectBody()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                var icon = ImageRequestPayload.FromToken("icon-token-123");
                var request = new UpdateChatRequest
                {
                    Icon = icon,
                    Title = "Updated",
                    Pin = "msg_789",
                    Notify = true
                };

                string? capturedContent = null;
                mockHttp.Expect(HttpMethod.Patch, $"{BaseUrl}/chats/{chatId}")
                        .Respond(async req =>
                        {
                            capturedContent = await req.Content?.ReadAsStringAsync();
                            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""chat_id"":123456}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.UpdateGroupChatAsync(chatId, request);

                // Assert
                Assert.NotNull(capturedContent);
                // Для проверки содержимого тоже используем наши опции, но можно и вручную
                var jsonDoc = JsonDocument.Parse(capturedContent);
                var root = jsonDoc.RootElement;

                Assert.True(root.TryGetProperty("icon", out var iconEl));
                Assert.True(iconEl.TryGetProperty("token", out var tokenEl));
                Assert.Equal("icon-token-123", tokenEl.GetString());

                Assert.True(root.TryGetProperty("title", out var titleEl));
                Assert.Equal("Updated", titleEl.GetString());

                Assert.True(root.TryGetProperty("pin", out var pinEl));
                Assert.Equal("msg_789", pinEl.GetString());

                Assert.True(root.TryGetProperty("notify", out var notifyEl));
                Assert.True(notifyEl.GetBoolean());
            }
        }
        
        public class DeleteGroupChatApiTests
        {
            private const string BaseUrl = "https://platform-api.max.ru";

            // Используем те же настройки сериализации, что и в реальном приложении
            private static readonly JsonSerializerOptions _jsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters =
                {
                    // добавьте конвертеры, если они нужны (для этого метода они не требуются)
                }
            };

            [Fact]
            public async Task DeleteGroupChatAsync_ShouldReturnSuccess_WhenChatExists()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, _jsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.DeleteGroupChatAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);

                mockHttp.VerifyNoOutstandingExpectation();
            }

            [Fact]
            public async Task DeleteGroupChatAsync_ShouldReturnFailure_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Chat not found"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, _jsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}")
                        .Respond("application/json", expectedJson); // API может вернуть 200 с failure или 404

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.DeleteGroupChatAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Equal("Chat not found", result.Message);
            }

            [Fact]
            public async Task DeleteGroupChatAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}")
                        .Respond(System.Net.HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.DeleteGroupChatAsync(chatId));
            }

            [Fact]
            public async Task DeleteGroupChatAsync_ShouldCallCorrectUrl()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                string? capturedUrl = null;

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}")
                        .Respond(req =>
                        {
                            capturedUrl = req.RequestUri?.ToString();
                            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.DeleteGroupChatAsync(chatId);

                // Assert
                Assert.NotNull(capturedUrl);
                Assert.Equal($"{BaseUrl}/chats/{chatId}", capturedUrl);
                mockHttp.VerifyNoOutstandingExpectation();
            }
        }
        
        /// <summary>
        /// Тесты для метода отправки действия бота в групповой чат (POST /chats/{chatId}/actions).
        /// </summary>
        public class SendBotActionApiTests
        {
            private const string BaseUrl = "https://platform-api.max.ru";

            // Настройки сериализации, как в реальном приложении
            private static readonly JsonSerializerOptions _jsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, false)
                    // Добавьте необходимые конвертеры, если они есть (например, для enum)
                }
            };

            [Fact]
            public async Task SendBotActionAsync_ShouldReturnSuccess_WhenActionIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new SenderActionRequest
                {
                    Action = SenderAction.TypingOn
                };

                var expectedResponse = new ApiResultResponce // используем ту же модель ответа
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, _jsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/actions")
                        .WithContent(JsonSerializer.Serialize(request, _jsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendBotActionAsync(chatId, request);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);

                mockHttp.VerifyNoOutstandingExpectation();
            }

            [Fact]
            public async Task SendBotActionAsync_ShouldReturnFailure_WhenActionIsInvalid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new SenderActionRequest
                {
                    Action = SenderAction.InvalidAction
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Invalid action"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, _jsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/actions")
                        .WithContent(JsonSerializer.Serialize(request, _jsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendBotActionAsync(chatId, request);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Equal("Invalid action", result.Message);
            }

            [Fact]
            public async Task SendBotActionAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new SenderActionRequest
                {
                    Action = SenderAction.TypingOn
                };

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/actions")
                        .WithContent(JsonSerializer.Serialize(request, _jsonOptions))
                        .Respond(System.Net.HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.SendBotActionAsync(chatId, request));
            }

            [Fact]
            public async Task SendBotActionAsync_ShouldSendCorrectBody()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new SenderActionRequest
                {
                    Action = SenderAction.SendingPhoto
                };

                string? capturedContent = null;
                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/actions")
                        .Respond(async req =>
                        {
                            capturedContent = await req.Content?.ReadAsStringAsync();
                            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp)
                {
                    BaseAddress = new Uri(BaseUrl)
                };

                var settings = new RefitSettings
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(_jsonOptions)
                };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.SendBotActionAsync(chatId, request);

                // Assert
                Assert.NotNull(capturedContent);
                var jsonDoc = JsonDocument.Parse(capturedContent);
                var root = jsonDoc.RootElement;

                Assert.True(root.TryGetProperty("action", out var actionEl));
                Assert.Equal("sending_photo", actionEl.GetString());

                mockHttp.VerifyNoOutstandingExpectation();
            }
        }
        
        /// <summary>
        /// Тесты для метода получения закреплённого сообщения (GET /chats/{chatId}/pin).
        /// </summary>
        public class GetPinnedMessageAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет, что при наличии закреплённого сообщения метод возвращает корректный объект Message,
            /// а Mid извлекается из Message.Body.
            /// </summary>
            [Fact]
            public async Task GetPinnedMessageAsync_ShouldReturnMessage_WhenPinnedMessageExists()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                // Правильный JSON: поле message содержит объект с полем body,
                // внутри которого mid, seq, text и т.д.
                var expectedJson = @"{
                    ""message"": {
                        ""body"": {
                            ""mid"": ""msg_789"",
                            ""seq"": 42,
                            ""text"": ""Important pinned message""
                        },
                        ""sender"": { ""user_id"": 100, ""first_name"": ""Alice"" },
                        ""timestamp"": 1612345678901
                    }
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetPinnedMessageAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Message);
                Assert.NotNull(result.Message.Body);
                Assert.Equal("msg_789", result.Message.Body.Mid);
                Assert.Equal(42, result.Message.Body.Seq);
                Assert.Equal("Important pinned message", result.Message.Body.Text);
                Assert.Equal(100, result.Message.Sender?.UserId);
                Assert.Equal(1612345678901, result.Message.Timestamp);
            }
            
            /// <summary>
            /// Проверяет, что если в чате нет закреплённого сообщения, возвращается объект с полем message = null.
            /// </summary>
            [Fact]
            public async Task GetPinnedMessageAsync_ShouldReturnNullMessage_WhenNoPinnedMessage()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                var expectedJson = @"{ ""message"": null }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetPinnedMessageAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.Null(result.Message);
            }
            
            /// <summary>
            /// Проверяет, что при запросе несуществующего чата выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task GetPinnedMessageAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetPinnedMessageAsync(chatId));
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetPinnedMessageAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetPinnedMessageAsync(chatId));
            }
        }
        
        /// <summary>
        /// Тесты для метода удаления закреплённого сообщения (DELETE /chats/{chatId}/pin).
        /// </summary>
        public class UnpinMessageAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешное удаление закреплённого сообщения.
            /// </summary>
            [Fact]
            public async Task UnpinMessageAsync_ShouldReturnSuccess_WhenRequestIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.UnpinMessageAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);

                mockHttp.VerifyNoOutstandingExpectation();
            }
            
            /// <summary>
            /// Проверяет, что при попытке удалить закреплённое сообщение в несуществующем чате возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task UnpinMessageAsync_ShouldReturnFailure_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Chat not found"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.UnpinMessageAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Equal("Chat not found", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task UnpinMessageAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.UnpinMessageAsync(chatId));
            }

            /// <summary>
            /// Проверяет, что при запросе несуществующего чата (и возврате 404) выбрасывается ApiException.
            /// </summary>
            [Fact]
            public async Task UnpinMessageAsync_ShouldThrowApiException_WhenChatNotFoundVia404()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/pin")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.UnpinMessageAsync(chatId));
            }
        }
        
        /// <summary>
        /// Тесты для метода получения информации о членстве бота (GET /chats/{chatId}/members/me).
        /// </summary>
        public class GetMyChatMembershipAsyncTests : ChatsMethods
        {
            
            
            /// <summary>
            /// Проверяет успешное получение информации о членстве бота (все поля присутствуют).
            /// </summary>
            [Fact]
            public async Task GetMyChatMembershipAsync_ShouldReturnChatMember_WhenBotIsMember()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                // JSON-ответ, соответствующий объекту ChatMember
                var expectedJson = @"{
                    ""user_id"": 98765,
                    ""first_name"": ""TestBot"",
                    ""username"": ""test_bot"",
                    ""is_bot"": true,
                    ""last_activity_time"": 1612345678901,
                    ""description"": ""Test bot description"",
                    ""avatar_url"": ""https://example.com/avatar.jpg"",
                    ""full_avatar_url"": ""https://example.com/avatar_full.jpg"",
                    ""last_access_time"": 1612345600000,
                    ""is_owner"": false,
                    ""is_admin"": true,
                    ""join_time"": 1612345600000,
                    ""permissions"": [""write"", ""pin_message""],
                    ""alias"": ""Bot""
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/me")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetMyChatMembershipAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(98765, result.UserId);
                Assert.Equal("TestBot", result.FirstName);
                Assert.Equal("test_bot", result.Username);
                Assert.True(result.IsBot);
                Assert.Equal(1612345678901, result.LastActivityTime);
                Assert.Equal("Test bot description", result.Description);
                Assert.Equal("https://example.com/avatar.jpg", result.AvatarUrl);
                Assert.Equal("https://example.com/avatar_full.jpg", result.FullAvatarUrl);
                Assert.Equal(1612345600000, result.LastAccessTime);
                Assert.False(result.IsOwner);
                Assert.True(result.IsAdmin);
                Assert.Equal(1612345600000, result.JoinTime);
                Assert.NotNull(result.Permissions);
                Assert.Contains(ChatAdminPermission.Write, result.Permissions);
                Assert.Contains(ChatAdminPermission.PinMessage, result.Permissions);
                Assert.Equal("Bot", result.Alias);
            }
            
            /// <summary>
            /// Проверяет, что при отсутствии бота в чате выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task GetMyChatMembershipAsync_ShouldThrowApiException_WhenBotNotMember()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/me")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetMyChatMembershipAsync(chatId));
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetMyChatMembershipAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/me")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetMyChatMembershipAsync(chatId));
            }
        }
        
        /// <summary>
        /// Тесты для метода выхода бота из группового чата (DELETE /chats/{chatId}/members/me).
        /// </summary>
        public class LeaveChatAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешный выход бота из чата.
            /// </summary>
            [Fact]
            public async Task LeaveChatAsync_ShouldReturnSuccess_WhenBotIsMember()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/me")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.LeaveChatAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);

                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при попытке покинуть несуществующий чат или чат, где бот не состоит, возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task LeaveChatAsync_ShouldReturnFailure_WhenBotNotInChat()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Bot is not a member of this chat"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/me")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.LeaveChatAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.False(result.Success);
                Assert.Equal("Bot is not a member of this chat", result.Message);
            }
            
            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task LeaveChatAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/me")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.LeaveChatAsync(chatId));
            }
            
            /// <summary>
            /// Проверяет, что при запросе несуществующего чата (если API возвращает 404) выбрасывается ApiException.
            /// </summary>
            [Fact]
            public async Task LeaveChatAsync_ShouldThrowApiException_WhenChatNotFoundVia404()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/me")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.LeaveChatAsync(chatId));
            }
        }
        
        
        /// <summary>
        /// Тесты для метода получения списка администраторов чата (GET /chats/{chatId}/members/admins).
        /// </summary>
        public class GetChatAdminsAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешное получение списка администраторов (все поля присутствуют).
            /// </summary>
            [Fact]
            public async Task GetChatAdminsAsync_ShouldReturnAdminsList_WhenBotIsAdmin()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var expectedJson = @"{
                    ""members"": [
                        {
                            ""user_id"": 100,
                            ""first_name"": ""Alice"",
                            ""is_bot"": false,
                            ""is_admin"": true,
                            ""is_owner"": false,
                            ""join_time"": 1612345600000,
                            ""permissions"": [""write"", ""pin_message""]
                        },
                        {
                            ""user_id"": 101,
                            ""first_name"": ""Bob"",
                            ""is_bot"": false,
                            ""is_admin"": true,
                            ""is_owner"": true,
                            ""join_time"": 1612345600001,
                            ""permissions"": [""read_all_messages"", ""add_remove_members""]
                        }
                    ],
                    ""marker"": 789012
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetChatAdminsAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Members);
                Assert.Equal(2, result.Members.Count);
                Assert.Equal(789012, result.Marker);

                // Проверка первого администратора
                var first = result.Members[0];
                Assert.Equal(100, first.UserId);
                Assert.Equal("Alice", first.FirstName);
                Assert.False(first.IsBot);
                Assert.True(first.IsAdmin);
                Assert.False(first.IsOwner);
                Assert.Contains(ChatAdminPermission.Write, first.Permissions);
                Assert.Contains(ChatAdminPermission.PinMessage, first.Permissions);

                // Проверка второго администратора
                var second = result.Members[1];
                Assert.Equal(101, second.UserId);
                Assert.Equal("Bob", second.FirstName);
                Assert.True(second.IsAdmin);
                Assert.True(second.IsOwner);
                Assert.Contains(ChatAdminPermission.ReadAllMessages, second.Permissions);
                Assert.Contains(ChatAdminPermission.AddRemoveMembers, second.Permissions);
            }

            /// <summary>
            /// Проверяет получение пустого списка администраторов.
            /// </summary>
            [Fact]
            public async Task GetChatAdminsAsync_ShouldReturnEmptyList_WhenNoAdmins()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var expectedJson = @"{
                    ""members"": [],
                    ""marker"": null
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetChatAdminsAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Members);
                Assert.Null(result.Marker);
            }
            
            /// <summary>
            /// Проверяет, что при отсутствии прав администратора выбрасывается ApiException с кодом 403.
            /// </summary>
            [Fact]
            public async Task GetChatAdminsAsync_ShouldThrowApiException_WhenBotIsNotAdmin()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .Respond(HttpStatusCode.Forbidden);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetChatAdminsAsync(chatId));
            }
            
            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetChatAdminsAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetChatAdminsAsync(chatId));
            }
            
            /// <summary>
            /// Проверяет, что при запросе несуществующего чата выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task GetChatAdminsAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetChatAdminsAsync(chatId));
            }
        }
        
        /// <summary>
        /// Тесты для метода назначения администраторов (POST /chats/{chatId}/members/admins).
        /// </summary>
        public class AddChatAdminsAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешное назначение одного администратора.
            /// </summary>
            [Fact]
            public async Task AddChatAdminsAsync_ShouldReturnSuccess_WhenRequestIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new AddAdminsRequest
                {
                    Admins = new List<ChatAdminRequest>
                    {
                        new()
                        {
                            UserId = 100,
                            Permissions = new List<ChatAdminPermission>
                            {
                                ChatAdminPermission.Write,
                                ChatAdminPermission.PinMessage
                            },
                            Alias = "Moderator"
                        }
                    }
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.AddChatAdminsAsync(chatId, request);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }
            
            /// <summary>
            /// Проверяет успешное назначение нескольких администраторов.
            /// </summary>
            [Fact]
            public async Task AddChatAdminsAsync_ShouldReturnSuccess_WithMultipleAdmins()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new AddAdminsRequest
                {
                    Admins = new List<ChatAdminRequest>
                    {
                        new() { UserId = 100, Permissions = new List<ChatAdminPermission> { ChatAdminPermission.Write } },
                        new() { UserId = 101, Permissions = new List<ChatAdminPermission> { ChatAdminPermission.ReadAllMessages, ChatAdminPermission.PinMessage } }
                    }
                };

                var expectedResponse = new ApiResultResponce { Success = true };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.AddChatAdminsAsync(chatId, request);

                // Assert
                Assert.True(result.Success);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет возврат failure с сообщением при ошибке (например, недостаточно прав).
            /// </summary>
            [Fact]
            public async Task AddChatAdminsAsync_ShouldReturnFailure_WhenBotLacksPermission()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new AddAdminsRequest
                {
                    Admins = new List<ChatAdminRequest> { new() { UserId = 100, Permissions = new List<ChatAdminPermission>() } }
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Bot does not have 'add_admins' permission"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.AddChatAdminsAsync(chatId, request);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Bot does not have 'add_admins' permission", result.Message);
            }
            
            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task AddChatAdminsAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                var request = new AddAdminsRequest();

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.AddChatAdminsAsync(chatId, request));
            }
            
            /// <summary>
            /// Проверяет, что при запросе несуществующего чата выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task AddChatAdminsAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;
                var request = new AddAdminsRequest();

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members/admins")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.AddChatAdminsAsync(chatId, request));
            }
        }
        
        /// <summary>
        /// Тесты для метода отмены прав администратора (DELETE /chats/{chatId}/members/admins/{userId}).
        /// </summary>
        public class RemoveChatAdminAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешную отмену прав администратора у пользователя.
            /// </summary>
            [Fact]
            public async Task RemoveChatAdminAsync_ShouldReturnSuccess_WhenRequestIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/admins/{userId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.RemoveChatAdminAsync(chatId, userId);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет возврат failure с сообщением при ошибке (например, пользователь не администратор).
            /// </summary>
            [Fact]
            public async Task RemoveChatAdminAsync_ShouldReturnFailure_WhenUserNotAdmin()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 999;

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "User is not an administrator"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/admins/{userId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.RemoveChatAdminAsync(chatId, userId);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("User is not an administrator", result.Message);
            }

            /// <summary>
            /// Проверяет возврат failure с сообщением при недостатке прав у бота.
            /// </summary>
            [Fact]
            public async Task RemoveChatAdminAsync_ShouldReturnFailure_WhenBotLacksPermission()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Bot does not have 'add_admins' permission"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/admins/{userId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.RemoveChatAdminAsync(chatId, userId);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Bot does not have 'add_admins' permission", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task RemoveChatAdminAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/admins/{userId}")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.RemoveChatAdminAsync(chatId, userId));
            }
            
            /// <summary>
            /// Проверяет, что при запросе несуществующего чата выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task RemoveChatAdminAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;
                long userId = 100;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members/admins/{userId}")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.RemoveChatAdminAsync(chatId, userId));
            }
        }
        
        /// <summary>
        /// Тесты для метода получения участников чата (GET /chats/{chatId}/members).
        /// </summary>
        public class GetChatMembersAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешное получение списка участников с пагинацией.
            /// </summary>
            [Fact]
            public async Task GetChatMembersAsync_ShouldReturnMembersList_WhenRequestIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var expectedJson = @"{
                    ""members"": [
                        {
                            ""user_id"": 100,
                            ""first_name"": ""Alice"",
                            ""is_bot"": false,
                            ""join_time"": 1612345600000
                        },
                        {
                            ""user_id"": 101,
                            ""first_name"": ""Bob"",
                            ""is_bot"": false,
                            ""join_time"": 1612345600001
                        }
                    ],
                    ""marker"": 789012
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members")
                        .WithQueryString("count=30")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetChatMembersAsync(chatId, marker: null, count: 30);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Members);
                Assert.Equal(2, result.Members.Count);
                Assert.Equal(100, result.Members[0].UserId);
                Assert.Equal("Alice", result.Members[0].FirstName);
                Assert.Equal(101, result.Members[1].UserId);
                Assert.Equal(789012, result.Marker);
            }

            /// <summary>
            /// Проверяет фильтрацию по списку ID пользователей.
            /// </summary>
            [Fact]
            public async Task GetChatMembersAsync_ShouldFilterByUserIds_WhenParameterProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                var userIds = new List<long> { 100, 102 };

                var expectedJson = @"{
                    ""members"": [
                        {
                            ""user_id"": 100,
                            ""first_name"": ""Alice""
                        },
                        {
                            ""user_id"": 102,
                            ""first_name"": ""Charlie""
                        }
                    ],
                    ""marker"": null
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members")
                        .WithQueryString("user_ids=100,102")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetChatMembersAsync(chatId, userIds: userIds);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Members.Count);
                Assert.Equal(100, result.Members[0].UserId);
                Assert.Equal(102, result.Members[1].UserId);
            }

            /// <summary>
            /// Проверяет получение пустого списка участников.
            /// </summary>
            [Fact]
            public async Task GetChatMembersAsync_ShouldReturnEmptyList_WhenNoMembers()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var expectedJson = @"{
                    ""members"": [],
                    ""marker"": null
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetChatMembersAsync(chatId);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Members);
                Assert.Null(result.Marker);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetChatMembersAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetChatMembersAsync(chatId));
            }

            /// <summary>
            /// Проверяет, что при запросе несуществующего чата выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task GetChatMembersAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetChatMembersAsync(chatId));
            }

            /// <summary>
            /// Проверяет корректную передачу query-параметров marker и count.
            /// </summary>
            [Fact]
            public async Task GetChatMembersAsync_ShouldPassPaginationParameters()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long? marker = 555;
                int? count = 50;

                string? capturedUrl = null;
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/chats/{chatId}/members*")
                        .Respond(req =>
                        {
                            capturedUrl = req.RequestUri?.ToString();
                            return new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""members"":[],""marker"":null}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.GetChatMembersAsync(chatId, marker: marker, count: count);

                // Assert
                Assert.NotNull(capturedUrl);
                Assert.Contains("marker=555", capturedUrl);
                Assert.Contains("count=50", capturedUrl);
            }
        }
        
        /// <summary>
        /// Тесты для метода добавления участников в чат (POST /chats/{chatId}/members).
        /// </summary>
        public class AddChatMembersAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешное добавление всех пользователей.
            /// </summary>
            [Fact]
            public async Task AddChatMembersAsync_ShouldReturnSuccess_WhenAllUsersAdded()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new AddMembersRequest
                {
                    UserIds = new List<long> { 100, 101, 102 }
                };

                var expectedResponse = new AddMembersResponse
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.AddChatMembersAsync(chatId, request);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет неуспешное добавление с сообщением об ошибке.
            /// </summary>
            [Fact]
            public async Task AddChatMembersAsync_ShouldReturnFailure_WhenAddFails()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;

                var request = new AddMembersRequest
                {
                    UserIds = new List<long> { 100, 101 }
                };

                var expectedResponse = new AddMembersResponse
                {
                    Success = false,
                    Message = "Bot does not have permission to add members"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.AddChatMembersAsync(chatId, request);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Bot does not have permission to add members", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task AddChatMembersAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                var request = new AddMembersRequest();

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.AddChatMembersAsync(chatId, request));
            }

            /// <summary>
            /// Проверяет, что при недостатке прав выбрасывается ApiException с кодом 403.
            /// </summary>
            [Fact]
            public async Task AddChatMembersAsync_ShouldThrowApiException_WhenBotNotAdmin()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                var request = new AddMembersRequest();

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.Forbidden);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.AddChatMembersAsync(chatId, request));
            }

            /// <summary>
            /// Проверяет, что при запросе несуществующего чата выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task AddChatMembersAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;
                var request = new AddMembersRequest();

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/chats/{chatId}/members")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.AddChatMembersAsync(chatId, request));
            }
        }
        
        /// <summary>
        /// Тесты для метода удаления участника из чата (DELETE /chats/{chatId}/members).
        /// </summary>
        public class RemoveChatMemberAsyncTests : ChatsMethods
        {
            /// <summary>
            /// Проверяет успешное удаление участника без блокировки.
            /// </summary>
            [Fact]
            public async Task RemoveChatMemberAsync_ShouldReturnSuccess_WhenUserRemoved()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members?user_id={userId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.RemoveChatMemberAsync(chatId, userId);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет успешное удаление участника с блокировкой.
            /// </summary>
            [Fact]
            public async Task RemoveChatMemberAsync_ShouldReturnSuccess_WhenUserRemovedAndBlocked()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;
                bool block = true;

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members?user_id={userId}&block=True")
                    .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.RemoveChatMemberAsync(chatId, userId, block: block);

                // Assert
                Assert.True(result.Success);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет возврат failure с сообщением при ошибке (например, недостаточно прав).
            /// </summary>
            [Fact]
            public async Task RemoveChatMemberAsync_ShouldReturnFailure_WhenBotLacksPermission()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Bot does not have permission to remove members"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members?user_id={userId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.RemoveChatMemberAsync(chatId, userId);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Bot does not have permission to remove members", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task RemoveChatMemberAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members?user_id={userId}")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.RemoveChatMemberAsync(chatId, userId));
            }

            /// <summary>
            /// Проверяет, что при недостатке прав выбрасывается ApiException с кодом 403.
            /// </summary>
            [Fact]
            public async Task RemoveChatMemberAsync_ShouldThrowApiException_WhenForbidden()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 123456;
                long userId = 100;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members?user_id={userId}")
                        .Respond(HttpStatusCode.Forbidden);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.RemoveChatMemberAsync(chatId, userId));
            }

            /// <summary>
            /// Проверяет, что при запросе несуществующего чата или пользователя выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task RemoveChatMemberAsync_ShouldThrowApiException_WhenNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 999999;
                long userId = 100;

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/chats/{chatId}/members?user_id={userId}")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.RemoveChatMemberAsync(chatId, userId));
            }
        }
    }

    public class SubscriptionsMethods : MaxApiTests
    {
        /// <summary>
        /// Тесты для методов работы с подписками.
        /// </summary>
        public class GetSubscriptions : SubscriptionsMethods
        {
            /// <summary>
            /// Проверяет успешное получение списка подписок.
            /// </summary>
            [Fact]
            public async Task GetSubscriptionsAsync_ShouldReturnSubscriptionsList_WhenResponseIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var expectedJson = @"{
                    ""subscriptions"": [
                        {
                            ""url"": ""https://example.com/webhook1"",
                            ""time"": 1612345678901,
                            ""update_types"": ""message_created,message_callback""
                        },
                        {
                            ""url"": ""https://example.com/webhook2"",
                            ""time"": 1612345678902,
                            ""update_types"": ""bot_added""
                        }
                    ]
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/subscriptions")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetSubscriptionsAsync();

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Subscriptions);
                Assert.Equal(2, result.Subscriptions.Count);

                var first = result.Subscriptions[0];
                Assert.Equal("https://example.com/webhook1", first.Url);
                Assert.Equal(1612345678901, first.Time);
                Assert.Equal("message_created,message_callback", first.UpdateTypes);

                var second = result.Subscriptions[1];
                Assert.Equal("https://example.com/webhook2", second.Url);
                Assert.Equal(1612345678902, second.Time);
                Assert.Equal("bot_added", second.UpdateTypes);
            }

            /// <summary>
            /// Проверяет получение пустого списка подписок.
            /// </summary>
            [Fact]
            public async Task GetSubscriptionsAsync_ShouldReturnEmptyList_WhenNoSubscriptions()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var expectedJson = @"{ ""subscriptions"": [] }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/subscriptions")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetSubscriptionsAsync();

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Subscriptions);
                Assert.Empty(result.Subscriptions);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetSubscriptionsAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/subscriptions")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetSubscriptionsAsync());
            }

            /// <summary>
            /// Проверяет, что при внутренней ошибке сервера выбрасывается ApiException с кодом 500.
            /// </summary>
            [Fact]
            public async Task GetSubscriptionsAsync_ShouldThrowApiException_WhenServerError()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/subscriptions")
                        .Respond(HttpStatusCode.InternalServerError);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetSubscriptionsAsync());
            }
        }
        
        /// <summary>
        /// Тесты для метода создания подписки (POST /subscriptions).
        /// </summary>
        public class CreateSubscriptionAsyncTests : SubscriptionsMethods
        {
            /// <summary>
            /// Проверяет успешное создание подписки с минимальными данными (только url).
            /// </summary>
            [Fact]
            public async Task CreateSubscriptionAsync_ShouldReturnSuccess_WhenOnlyUrlProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new CreateSubscriptionRequest
                {
                    Url = "https://example.com/webhook"
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/subscriptions")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.CreateSubscriptionAsync(request);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет успешное создание подписки со всеми полями.
            /// </summary>
            [Fact]
            public async Task CreateSubscriptionAsync_ShouldReturnSuccess_WithAllFields()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new CreateSubscriptionRequest
                {
                    Url = "https://example.com/webhook",
                    UpdateTypes = new List<string> { "message_created", "bot_started" },
                    Secret = "mySecret123"
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/subscriptions")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.CreateSubscriptionAsync(request);

                // Assert
                Assert.True(result.Success);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при невалидном URL возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task CreateSubscriptionAsync_ShouldReturnFailure_WhenInvalidUrl()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new CreateSubscriptionRequest
                {
                    Url = "invalid-url"
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Invalid URL format"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/subscriptions")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.CreateSubscriptionAsync(request);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Invalid URL format", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task CreateSubscriptionAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new CreateSubscriptionRequest { Url = "https://example.com/webhook" };

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/subscriptions")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.CreateSubscriptionAsync(request));
            }

            /// <summary>
            /// Проверяет, что при конфликте (подписка уже существует) возвращается соответствующий ответ.
            /// </summary>
            [Fact]
            public async Task CreateSubscriptionAsync_ShouldReturnFailure_WhenAlreadyExists()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new CreateSubscriptionRequest
                {
                    Url = "https://example.com/webhook"
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Subscription already exists for this URL"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/subscriptions")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.CreateSubscriptionAsync(request);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Subscription already exists for this URL", result.Message);
            }
        }
        
        /// <summary>
        /// Тесты для метода удаления подписки (DELETE /subscriptions).
        /// </summary>
        public class DeleteSubscriptionAsyncTests : SubscriptionsMethods
        {
            /// <summary>
            /// Проверяет успешное удаление подписки.
            /// </summary>
            [Fact]
            public async Task DeleteSubscriptionAsync_ShouldReturnSuccess_WhenUrlIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string testUrl = "https://example.com/webhook";
                string encodedUrl = Uri.EscapeDataString(testUrl);

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/subscriptions?url={encodedUrl}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.DeleteSubscriptionAsync(testUrl);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при попытке удалить несуществующую подписку возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task DeleteSubscriptionAsync_ShouldReturnFailure_WhenUrlNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string testUrl = "https://example.com/nonexistent";
                string encodedUrl = Uri.EscapeDataString(testUrl);

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Subscription not found"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/subscriptions?url={encodedUrl}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.DeleteSubscriptionAsync(testUrl);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Subscription not found", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task DeleteSubscriptionAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string testUrl = "https://example.com/webhook";
                string encodedUrl = Uri.EscapeDataString(testUrl);

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/subscriptions?url={encodedUrl}")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.DeleteSubscriptionAsync(testUrl));
            }

            /// <summary>
            /// Проверяет корректное кодирование специальных символов в URL.
            /// </summary>
            [Fact]
            public async Task DeleteSubscriptionAsync_ShouldEncodeUrlParameter()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string testUrl = "https://example.com/path?param=value&other=1";
                string encodedUrl = Uri.EscapeDataString(testUrl); // должно стать https%3A%2F%2Fexample.com%2Fpath%3Fparam%3Dvalue%26other%3D1

                var expectedResponse = new ApiResultResponce { Success = true };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                string? capturedUrl = null;
                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/subscriptions*")
                        .Respond(req =>
                        {
                            capturedUrl = req.RequestUri?.ToString();
                            return new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(expectedJson, Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.DeleteSubscriptionAsync(testUrl);

                // Assert
                Assert.NotNull(capturedUrl);
                Assert.Contains($"url={encodedUrl}", capturedUrl);
                // Проверим, что параметр действительно закодирован
                Assert.DoesNotContain("?param=value", capturedUrl); // эти символы не должны остаться незакодированными
            }
        }
        
        /// <summary>
        /// Тесты для методов работы с обновлениями (updates).
        /// </summary>
        public class UpdatesMethods : MaxApiTests
        {
            /// <summary>
            /// Проверяет успешное получение списка обновлений.
            /// </summary>
            [Fact]
            public async Task GetUpdatesAsync_ShouldReturnUpdatesList_WhenResponseIsValid()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var expectedJson = @"{
                    ""updates"": [
                        {
                            ""update_type"": ""message_created"",
                            ""timestamp"": 1612345678901,
                            ""message"": {
                                ""body"": { ""mid"": ""msg1"" }
                            }
                        },
                        {
                            ""update_type"": ""bot_added"",
                            ""timestamp"": 1612345678902,
                            ""chat_id"": 123,
                            ""user"": { ""user_id"": 456 },
                            ""is_channel"": false
                        }
                    ],
                    ""marker"": 42
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/updates")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetUpdatesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Updates);
                Assert.Equal(2, result.Updates.Count);
                Assert.Equal(42, result.Marker);

                // Первое обновление — message_created
                result.Updates[0].Match(
                    messageCreatedUpdateCase: (msg, locale) =>
                    {
                        Assert.Null(locale); // в тесте нет user_locale
                        Assert.NotNull(msg.Body);
                        Assert.Equal("msg1", msg.Body.Mid);
                    },
                    messageCallbackUpdateCase: (_, _, _) => Assert.Fail("Unexpected message_callback"),
                    messageEditedUpdateCase: _ => Assert.Fail("Unexpected message_edited"),
                    messageRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected message_removed"),
                    botAddedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot_added"),
                    botRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot_removed"),
                    dialogMutedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected dialog_muted"),
                    dialogUnmutedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog_unmuted"),
                    dialogClearedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog_cleared"),
                    dialogRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog_removed"),
                    userAddedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user_added"),
                    userRemovedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user_removed"),
                    botStartedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected bot_started"),
                    botStoppedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot_stopped"),
                    chatTitleChangedUpdateCase: (_, _, _) => Assert.Fail("Unexpected chat_title_changed")
                );

                // Второе обновление — bot_added
                result.Updates[1].Match(
                    botAddedUpdateCase: (chatId, user, isChannel) =>
                    {
                        Assert.Equal(123, chatId);
                        Assert.Equal(456, user.UserId);
                        Assert.False(isChannel);
                    },
                    messageCreatedUpdateCase: (_, _) => Assert.Fail("Unexpected message_created"),
                    messageCallbackUpdateCase: (_, _, _) => Assert.Fail("Unexpected message_callback"),
                    messageEditedUpdateCase: _ => Assert.Fail("Unexpected message_edited"),
                    messageRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected message_removed"),
                    botRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot_removed"),
                    dialogMutedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected dialog_muted"),
                    dialogUnmutedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog_unmuted"),
                    dialogClearedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog_cleared"),
                    dialogRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog_removed"),
                    userAddedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user_added"),
                    userRemovedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user_removed"),
                    botStartedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected bot_started"),
                    botStoppedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot_stopped"),
                    chatTitleChangedUpdateCase: (_, _, _) => Assert.Fail("Unexpected chat_title_changed")
                );
            }
            /// <summary>
            /// Проверяет получение пустого списка обновлений.
            /// </summary>
            [Fact]
            public async Task GetUpdatesAsync_ShouldReturnEmptyList_WhenNoUpdates()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var expectedJson = @"{ ""updates"": [], ""marker"": null }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/updates")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetUpdatesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Updates);
                Assert.Null(result.Marker);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetUpdatesAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/updates")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetUpdatesAsync());
            }

            /// <summary>
            /// Проверяет корректную передачу всех query-параметров.
            /// </summary>
            [Fact]
            public async Task GetUpdatesAsync_ShouldPassAllQueryParameters()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();

                mockHttp.Expect(HttpMethod.Get, $"{BaseUrl}/updates")
                    .WithQueryString("limit=50&timeout=45&marker=100&types=message_created,message_callback")
                    .Respond("application/json", @"{""updates"":[],""marker"":null}");

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.GetUpdatesAsync(limit: 50, timeout: 45, marker: 100, types: "message_created,message_callback");

                // Assert
                mockHttp.VerifyNoOutstandingExpectation();
            }
            
            
            /// <summary>
            /// Проверяет, что при внутренней ошибке сервера выбрасывается ApiException с кодом 500.
            /// </summary>
            [Fact]
            public async Task GetUpdatesAsync_ShouldThrowApiException_WhenServerError()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/updates")
                        .Respond(HttpStatusCode.InternalServerError);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetUpdatesAsync());
            }
        }
        
    }
    
    /// <summary>
    /// Тесты для методов работы с загрузками (uploads).
    /// </summary>
    public class UploadsMethods : MaxApiTests
    {
        /// <summary>
        /// Проверяет успешное получение URL для загрузки изображения (без токена).
        /// </summary>
        [Fact]
        public async Task GetUploadUrlAsync_ShouldReturnUrl_ForImageType()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var expectedJson = @"{
                ""url"": ""https://upload.example.com/upload?token=abc123""
            }";

            mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/uploads?type=image")
                    .Respond("application/json", expectedJson);

            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
            var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
            var api = RestService.For<IMaxApi>(httpClient, settings);

            // Act
            var result = await api.GetUploadUrlAsync(UploadType.Image);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://upload.example.com/upload?token=abc123", result.Url);
            Assert.Null(result.Token);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// Проверяет успешное получение URL для загрузки видео (с токеном).
        /// </summary>
        [Fact]
        public async Task GetUploadUrlAsync_ShouldReturnUrlAndToken_ForVideoType()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var expectedJson = @"{
                ""url"": ""https://vu.mycdn.me/upload.do?sig=signature&expires=123456789"",
                ""token"": ""video_token_123""
            }";

            mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/uploads?type=video")
                    .Respond("application/json", expectedJson);

            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
            var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
            var api = RestService.For<IMaxApi>(httpClient, settings);

            // Act
            var result = await api.GetUploadUrlAsync(UploadType.Video);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://vu.mycdn.me/upload.do?sig=signature&expires=123456789", result.Url);
            Assert.Equal("video_token_123", result.Token);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// Проверяет успешное получение URL для загрузки аудио (с токеном).
        /// </summary>
        [Fact]
        public async Task GetUploadUrlAsync_ShouldReturnUrlAndToken_ForAudioType()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var expectedJson = @"{
                ""url"": ""https://au.mycdn.me/upload.do?sig=signature&expires=123456789"",
                ""token"": ""audio_token_456""
            }";

            mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/uploads?type=audio")
                    .Respond("application/json", expectedJson);

            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
            var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
            var api = RestService.For<IMaxApi>(httpClient, settings);

            // Act
            var result = await api.GetUploadUrlAsync(UploadType.Audio);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://au.mycdn.me/upload.do?sig=signature&expires=123456789", result.Url);
            Assert.Equal("audio_token_456", result.Token);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// Проверяет успешное получение URL для загрузки файла (без токена).
        /// </summary>
        [Fact]
        public async Task GetUploadUrlAsync_ShouldReturnUrl_ForFileType()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            var expectedJson = @"{
                ""url"": ""https://upload.example.com/upload?token=file_token_789""
            }";

            mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/uploads?type=file")
                    .Respond("application/json", expectedJson);

            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
            var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
            var api = RestService.For<IMaxApi>(httpClient, settings);

            // Act
            var result = await api.GetUploadUrlAsync(UploadType.File);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://upload.example.com/upload?token=file_token_789", result.Url);
            Assert.Null(result.Token);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        /// <summary>
        /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
        /// </summary>
        [Fact]
        public async Task GetUploadUrlAsync_ShouldThrowApiException_WhenUnauthorized()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, $"{BaseUrl}/uploads?type=image")
                    .Respond(HttpStatusCode.Unauthorized);

            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
            var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
            var api = RestService.For<IMaxApi>(httpClient, settings);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => api.GetUploadUrlAsync(UploadType.Image));
        }

        /// <summary>
        /// Проверяет, что при ошибке сервера (например, неверный тип файла) выбрасывается ApiException с кодом 400.
        /// </summary>
        [Fact]
        public async Task GetUploadUrlAsync_ShouldThrowApiException_WhenServerReturnsBadRequest()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, $"{BaseUrl}/uploads?type=image")
                .Respond(HttpStatusCode.BadRequest);

            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
            var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
            var api = RestService.For<IMaxApi>(httpClient, settings);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => api.GetUploadUrlAsync(UploadType.Image));
        }

        /// <summary>
        /// Проверяет корректную передачу query-параметра type.
        /// </summary>
        [Fact]
        public async Task GetUploadUrlAsync_ShouldPassTypeParameter()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();
            string? capturedUrl = null;

            mockHttp.When(HttpMethod.Post, $"{BaseUrl}/uploads*")
                    .Respond(req =>
                    {
                        capturedUrl = req.RequestUri?.ToString();
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(@"{""url"":""test""}", Encoding.UTF8, "application/json")
                        };
                    });

            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
            var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
            var api = RestService.For<IMaxApi>(httpClient, settings);

            // Act
            await api.GetUploadUrlAsync(UploadType.Video);

            // Assert
            Assert.NotNull(capturedUrl);
            Assert.Contains("type=video", capturedUrl);
        }
    }

    public class MessagesMethods : MaxApiTests
    {
        /// <summary>
        /// Тесты для методов работы с сообщениями.
        /// </summary>
        public class GetMessageMethods : MessagesMethods
        {
            /// <summary>
            /// Проверяет успешное получение сообщений по chat_id.
            /// </summary>
            [Fact]
            public async Task GetMessagesAsync_ShouldReturnMessages_WhenChatIdProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var expectedJson = @"{
                    ""messages"": [
                        {
                            ""body"": { ""mid"": ""msg1"", ""text"": ""Hello"" },
                            ""timestamp"": 1612345678901
                        },
                        {
                            ""body"": { ""mid"": ""msg2"", ""text"": ""World"" },
                            ""timestamp"": 1612345678900
                        }
                    ]
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages?chat_id=123")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetMessagesAsync(chatId: 123);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Messages);
                Assert.Equal(2, result.Messages.Count);

                var first = result.Messages[0];
                Assert.NotNull(first.Body);
                Assert.Equal("msg1", first.Body.Mid);
                Assert.Equal("Hello", first.Body.Text);
                Assert.Equal(1612345678901, first.Timestamp);

                var second = result.Messages[1];
                Assert.NotNull(second.Body);
                Assert.Equal("msg2", second.Body.Mid);
                Assert.Equal("World", second.Body.Text);
                Assert.Equal(1612345678900, second.Timestamp);
            }

            /// <summary>
            /// Проверяет успешное получение сообщений по message_ids.
            /// </summary>
            [Fact]
            public async Task GetMessagesAsync_ShouldReturnMessages_WhenMessageIdsProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var expectedJson = @"{
                    ""messages"": [
                        {
                            ""body"": { ""mid"": ""msg1"", ""text"": ""Hello"" }
                        },
                        {
                            ""body"": { ""mid"": ""msg3"", ""text"": ""Third"" }
                        }
                    ]
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages?message_ids=msg1,msg3")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetMessagesAsync(messageIds: "msg1,msg3");

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Messages.Count);

                var first = result.Messages[0];
                Assert.NotNull(first.Body);
                Assert.Equal("msg1", first.Body.Mid);
                Assert.Equal("Hello", first.Body.Text);

                var second = result.Messages[1];
                Assert.NotNull(second.Body);
                Assert.Equal("msg3", second.Body.Mid);
                Assert.Equal("Third", second.Body.Text);
            }

            /// <summary>
            /// Проверяет успешное получение пустого списка сообщений.
            /// </summary>
            [Fact]
            public async Task GetMessagesAsync_ShouldReturnEmptyList_WhenNoMessages()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var expectedJson = @"{ ""messages"": [] }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages?chat_id=123")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetMessagesAsync(chatId: 123);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result.Messages);
            }

            /// <summary>
            /// Проверяет передачу всех опциональных параметров.
            /// </summary>
            [Fact]
            public async Task GetMessagesAsync_ShouldPassAllParameters()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();

                mockHttp.Expect(HttpMethod.Get, $"{BaseUrl}/messages")
                        .WithQueryString("chat_id=123&from=1000&to=2000&count=30")
                        .Respond("application/json", @"{""messages"":[]}");

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.GetMessagesAsync(chatId: 123, from: 1000, to: 2000, count: 30);

                // Assert
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetMessagesAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages?chat_id=123")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetMessagesAsync(chatId: 123));
            }

            /// <summary>
            /// Проверяет, что при отсутствии чата выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task GetMessagesAsync_ShouldThrowApiException_WhenChatNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages?chat_id=999")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetMessagesAsync(chatId: 999));
            }
        }
        
        /// <summary>
        /// Тесты для метода отправки сообщения (POST /messages).
        /// </summary>
        public class SendMessageAsyncTests : MessagesMethods
        {
            /// <summary>
            /// Проверяет успешную отправку сообщения пользователю.
            /// </summary>
            [Fact]
            public async Task SendMessageAsync_ShouldReturnMessage_WhenUserIdProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long userId = 123;
                var request = new NewMessageBody
                {
                    Text = "Hello",
                    Notify = true
                };

                var expectedResponseJson = @"{
                    ""message"": {
                        ""body"": { ""mid"": ""msg123"", ""text"": ""Hello"" },
                        ""timestamp"": 1612345678901
                    }
                }";

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/messages?user_id={userId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedResponseJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendMessageAsync(request, userId: userId);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Message);
                Assert.NotNull(result.Message.Body);
                Assert.Equal("msg123", result.Message.Body.Mid);
                Assert.Equal("Hello", result.Message.Body.Text);
                Assert.Equal(1612345678901, result.Message.Timestamp);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет успешную отправку сообщения в чат.
            /// </summary>
            [Fact]
            public async Task SendMessageAsync_ShouldReturnMessage_WhenChatIdProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                long chatId = 456;
                var request = new NewMessageBody
                {
                    Text = "Group message"
                };

                var expectedResponseJson = @"{
                    ""message"": {
                        ""body"": { ""mid"": ""msg456"", ""text"": ""Group message"" }
                    }
                }";

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/messages?chat_id={chatId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedResponseJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendMessageAsync(request, chatId: chatId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("msg456", result.Message.Body?.Mid);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет передачу всех query-параметров (disable_link_preview).
            /// </summary>
            [Fact]
            public async Task SendMessageAsync_ShouldPassDisableLinkPreviewParameter()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new NewMessageBody { Text = "Check link preview" };

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/messages?user_id=123&disable_link_preview=True")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", @"{""message"":{""body"":{""mid"":""msg""}}}");

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendMessageAsync(request, userId: 123, disableLinkPreview: true);

                // Assert
                Assert.NotNull(result);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task SendMessageAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new NewMessageBody { Text = "Test" };

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/messages?user_id=123")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.SendMessageAsync(request, userId: 123));
            }

            /// <summary>
            /// Проверяет, что при неверном запросе (например, нет userId/chatId) выбрасывается ApiException с кодом 400.
            /// </summary>
            [Fact]
            public async Task SendMessageAsync_ShouldThrowApiException_WhenBadRequest()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                var request = new NewMessageBody { Text = "Test" };

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/messages")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.BadRequest);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.SendMessageAsync(request));
            }
        }
        
        /// <summary>
        /// Тесты для метода редактирования сообщения (PUT /messages).
        /// </summary>
        public class EditMessageAsyncTests : MessagesMethods
        {
            /// <summary>
            /// Проверяет успешное редактирование текста сообщения.
            /// </summary>
            [Fact]
            public async Task EditMessageAsync_ShouldReturnSuccess_WhenOnlyTextChanged()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";
                var request = new NewMessageBody
                {
                    Text = "Updated text"
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Put, $"{BaseUrl}/messages?message_id={messageId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.EditMessageAsync(messageId, request);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет успешное удаление вложений (передача пустого списка).
            /// </summary>
            [Fact]
            public async Task EditMessageAsync_ShouldReturnSuccess_WhenAttachmentsCleared()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";
                var request = new NewMessageBody
                {
                    Attachments = new List<AttachmentRequest>() // пустой список для удаления
                };

                var expectedResponse = new ApiResultResponce { Success = true };

                mockHttp.Expect(HttpMethod.Put, $"{BaseUrl}/messages?message_id={messageId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.EditMessageAsync(messageId, request);

                // Assert
                Assert.True(result.Success);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при редактировании слишком старого сообщения возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task EditMessageAsync_ShouldReturnFailure_WhenMessageTooOld()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "old_msg";
                var request = new NewMessageBody { Text = "New text" };

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Cannot edit message older than 24 hours"
                };

                mockHttp.Expect(HttpMethod.Put, $"{BaseUrl}/messages?message_id={messageId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.EditMessageAsync(messageId, request);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Cannot edit message older than 24 hours", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task EditMessageAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";
                var request = new NewMessageBody { Text = "Test" };

                mockHttp.When(HttpMethod.Put, $"{BaseUrl}/messages?message_id={messageId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.EditMessageAsync(messageId, request));
            }

            /// <summary>
            /// Проверяет, что при несуществующем сообщении выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task EditMessageAsync_ShouldThrowApiException_WhenMessageNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "invalid";
                var request = new NewMessageBody { Text = "Test" };

                mockHttp.When(HttpMethod.Put, $"{BaseUrl}/messages?message_id={messageId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.EditMessageAsync(messageId, request));
            }

            /// <summary>
            /// Проверяет передачу всех опциональных полей в теле запроса.
            /// </summary>
            [Fact]
            public async Task EditMessageAsync_ShouldSendAllOptionalFields()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";
                var request = new NewMessageBody
                {
                    Text = "Updated",
                    Attachments = new List<AttachmentRequest>(),
                    Link = new NewMessageLink { Type = MessageLinkType.Reply, Mid = "orig_msg" },
                    Notify = false,
                    Format = TextFormat.Markdown
                };

                mockHttp.Expect(HttpMethod.Put, $"{BaseUrl}/messages?message_id={messageId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", @"{""success"":true}");

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.EditMessageAsync(messageId, request);

                // Assert
                Assert.True(result.Success);
                mockHttp.VerifyNoOutstandingExpectation();
            }
        }
        
        /// <summary>
        /// Тесты для метода удаления сообщения (DELETE /messages).
        /// </summary>
        public class DeleteMessageAsyncTests : MessagesMethods
        {
            /// <summary>
            /// Проверяет успешное удаление сообщения.
            /// </summary>
            [Fact]
            public async Task DeleteMessageAsync_ShouldReturnSuccess_WhenMessageExists()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/messages?message_id={messageId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.DeleteMessageAsync(messageId);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при попытке удалить несуществующее сообщение возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task DeleteMessageAsync_ShouldReturnFailure_WhenMessageNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "invalid";

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Message not found"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/messages?message_id={messageId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.DeleteMessageAsync(messageId);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Message not found", result.Message);
            }

            /// <summary>
            /// Проверяет, что при попытке удалить слишком старое сообщение возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task DeleteMessageAsync_ShouldReturnFailure_WhenMessageTooOld()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "old_msg";

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Cannot delete message older than 24 hours"
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/messages?message_id={messageId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.DeleteMessageAsync(messageId);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Cannot delete message older than 24 hours", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task DeleteMessageAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/messages?message_id={messageId}")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.DeleteMessageAsync(messageId));
            }

            /// <summary>
            /// Проверяет, что при внутренней ошибке сервера выбрасывается ApiException с кодом 500.
            /// </summary>
            [Fact]
            public async Task DeleteMessageAsync_ShouldThrowApiException_WhenServerError()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";

                mockHttp.When(HttpMethod.Delete, $"{BaseUrl}/messages?message_id={messageId}")
                        .Respond(HttpStatusCode.InternalServerError);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.DeleteMessageAsync(messageId));
            }

            /// <summary>
            /// Проверяет корректное кодирование параметра message_id в URL.
            /// </summary>
            [Fact]
            public async Task DeleteMessageAsync_ShouldEncodeMessageId()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg 123"; // содержит пробел
                string expectedQuery = "?message_id=msg%20123";

                string? capturedQuery = null;
                mockHttp.Expect(HttpMethod.Delete, $"{BaseUrl}/messages*")
                    .Respond(req =>
                    {
                        capturedQuery = req.RequestUri?.Query;
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "application/json")
                        };
                    });

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.DeleteMessageAsync(messageId);

                // Assert
                Assert.NotNull(capturedQuery);
                Assert.Equal(expectedQuery, capturedQuery);
            }
        }
        
        /// <summary>
        /// Тесты для метода получения сообщения по ID (GET /messages/{messageId}).
        /// </summary>
        public class GetMessageByIdAsyncTests : MessagesMethods
        {
            /// <summary>
            /// Проверяет успешное получение сообщения.
            /// </summary>
            [Fact]
            public async Task GetMessageByIdAsync_ShouldReturnMessage_WhenMessageExists()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";

                var expectedJson = @"{
                    ""body"": { ""mid"": ""msg123"", ""text"": ""Hello world"" },
                    ""sender"": { ""user_id"": 456, ""first_name"": ""Alice"" },
                    ""timestamp"": 1612345678901,
                    ""recipient"": { ""chat_id"": 789, ""chat_type"": ""chat"" }
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages/{messageId}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetMessageByIdAsync(messageId);

                // Assert
                Assert.NotNull(result);
                Assert.NotNull(result.Body);
                Assert.Equal("msg123", result.Body.Mid);
                Assert.Equal("Hello world", result.Body.Text);
                Assert.Equal(456, result.Sender?.UserId);
                Assert.Equal("Alice", result.Sender?.FirstName);
                Assert.Equal(1612345678901, result.Timestamp);
                Assert.NotNull(result.Recipient);
                Assert.Equal(789, result.Recipient.ChatId);
            }

            /// <summary>
            /// Проверяет, что при отсутствии сообщения выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task GetMessageByIdAsync_ShouldThrowApiException_WhenMessageNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "invalid";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages/{messageId}")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetMessageByIdAsync(messageId));
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetMessageByIdAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages/{messageId}")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetMessageByIdAsync(messageId));
            }

            /// <summary>
            /// Проверяет, что при внутренней ошибке сервера выбрасывается ApiException с кодом 500.
            /// </summary>
            [Fact]
            public async Task GetMessageByIdAsync_ShouldThrowApiException_WhenServerError()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg123";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/messages/{messageId}")
                        .Respond(HttpStatusCode.InternalServerError);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetMessageByIdAsync(messageId));
            }

            /// <summary>
            /// Проверяет корректное кодирование параметра messageId в URL.
            /// </summary>
            [Fact]
            public async Task GetMessageByIdAsync_ShouldEncodeMessageId()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string messageId = "msg 123/?"; // символы, требующие кодирования

                string? capturedPath = null;
                mockHttp.Expect(HttpMethod.Get, $"{BaseUrl}/messages/{Uri.EscapeDataString(messageId)}")
                        .Respond(req =>
                        {
                            capturedPath = req.RequestUri?.PathAndQuery;
                            return new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""body"":{""mid"":""msg""}}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.GetMessageByIdAsync(messageId);

                // Assert
                Assert.NotNull(capturedPath);
                Assert.Contains(Uri.EscapeDataString(messageId), capturedPath);
                mockHttp.VerifyNoOutstandingExpectation();
            }
        }
        
        /// <summary>
        /// Тесты для методов работы с видео.
        /// </summary>
        public class VideosMethods : MaxApiTests
        {
            /// <summary>
            /// Проверяет успешное получение информации о видео.
            /// </summary>
            [Fact]
            public async Task GetVideoInfoAsync_ShouldReturnVideoInfo_WhenVideoExists()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string videoToken = "abc123";

                var expectedJson = @"{
                    ""token"": ""abc123"",
                    ""urls"": {
                        ""playback"": ""https://example.com/video.mp4"",
                        ""download"": ""https://example.com/video_download.mp4""
                    },
                    ""thumbnail"": {
                        ""photo_id"": 456,
                        ""token"": ""thumb_token"",
                        ""url"": ""https://example.com/thumb.jpg""
                    },
                    ""width"": 1920,
                    ""height"": 1080,
                    ""duration"": 125
                }";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/videos/{videoToken}")
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.GetVideoInfoAsync(videoToken);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("abc123", result.Token);
                Assert.NotNull(result.Urls);
                // Проверка, что дополнительные данные из urls сохранились
                Assert.NotNull(result.Thumbnail);
                Assert.Equal(456, result.Thumbnail.PhotoId);
                Assert.Equal("thumb_token", result.Thumbnail.Token);
                Assert.Equal("https://example.com/thumb.jpg", result.Thumbnail.Url);
                Assert.Equal(1920, result.Width);
                Assert.Equal(1080, result.Height);
                Assert.Equal(125, result.Duration);
            }

            /// <summary>
            /// Проверяет, что при отсутствии видео выбрасывается ApiException с кодом 404.
            /// </summary>
            [Fact]
            public async Task GetVideoInfoAsync_ShouldThrowApiException_WhenVideoNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string videoToken = "invalid";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/videos/{videoToken}")
                        .Respond(HttpStatusCode.NotFound);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetVideoInfoAsync(videoToken));
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task GetVideoInfoAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string videoToken = "abc123";

                mockHttp.When(HttpMethod.Get, $"{BaseUrl}/videos/{videoToken}")
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.GetVideoInfoAsync(videoToken));
            }

            /// <summary>
            /// Проверяет корректное кодирование параметра videoToken в URL.
            /// </summary>
            [Fact]
            public async Task GetVideoInfoAsync_ShouldEncodeVideoToken()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string videoToken = "abc 123/?"; // символы, требующие кодирования
                string expectedPath = $"/videos/{Uri.EscapeDataString(videoToken)}";

                string? capturedPath = null;
                mockHttp.Expect(HttpMethod.Get, $"{BaseUrl}{expectedPath}")
                        .Respond(req =>
                        {
                            capturedPath = req.RequestUri?.PathAndQuery;
                            return new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""token"":""abc"",""width"":0,""height"":0,""duration"":0}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.GetVideoInfoAsync(videoToken);

                // Assert
                Assert.NotNull(capturedPath);
                Assert.Equal(expectedPath, capturedPath);
                mockHttp.VerifyNoOutstandingExpectation();
            }
        }
        
        /// <summary>
        /// Тесты для метода ответа на callback (POST /answers).
        /// </summary>
        public class AnswersMethods : MaxApiTests
        {
            /// <summary>
            /// Проверяет успешную отправку ответа с обновлением сообщения.
            /// </summary>
            [Fact]
            public async Task SendAnswerAsync_ShouldReturnSuccess_WhenMessageProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string callbackId = "cb_123";

                var request = new AnswerRequest
                {
                    Message = new NewMessageBody
                    {       
                        Text = "Обновлённый текст",
                        Attachments = new List<AttachmentRequest>()
                    }
                };

                var expectedResponse = new ApiResultResponce
                {
                    Success = true,
                    Message = null
                };
                var expectedJson = JsonSerializer.Serialize(expectedResponse, JsonOptions);

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/answers?callback_id={callbackId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", expectedJson);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendAnswerAsync(callbackId, request);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Null(result.Message);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет успешную отправку ответа только с уведомлением.
            /// </summary>
            [Fact]
            public async Task SendAnswerAsync_ShouldReturnSuccess_WhenNotificationProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string callbackId = "cb_123";

                var request = new AnswerRequest
                {
                    Notification = "Действие выполнено!"
                };

                var expectedResponse = new ApiResultResponce { Success = true };

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/answers?callback_id={callbackId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendAnswerAsync(callbackId, request);

                // Assert
                Assert.True(result.Success);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет успешную отправку ответа с обоими полями.
            /// </summary>
            [Fact]
            public async Task SendAnswerAsync_ShouldReturnSuccess_WhenBothProvided()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string callbackId = "cb_123";

                var request = new AnswerRequest
                {
                    Message = new NewMessageBody { Text = "Обновление" },
                    Notification = "Уведомление"
                };

                var expectedResponse = new ApiResultResponce { Success = true };

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/answers?callback_id={callbackId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendAnswerAsync(callbackId, request);

                // Assert
                Assert.True(result.Success);
                mockHttp.VerifyNoOutstandingExpectation();
            }

            /// <summary>
            /// Проверяет, что при неверном callback_id возвращается failure с сообщением.
            /// </summary>
            [Fact]
            public async Task SendAnswerAsync_ShouldReturnFailure_WhenCallbackNotFound()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string callbackId = "invalid";

                var request = new AnswerRequest { Notification = "Test" };

                var expectedResponse = new ApiResultResponce
                {
                    Success = false,
                    Message = "Callback not found"
                };

                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/answers?callback_id={callbackId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                var result = await api.SendAnswerAsync(callbackId, request);

                // Assert
                Assert.False(result.Success);
                Assert.Equal("Callback not found", result.Message);
            }

            /// <summary>
            /// Проверяет, что при отсутствии авторизации выбрасывается ApiException с кодом 401.
            /// </summary>
            [Fact]
            public async Task SendAnswerAsync_ShouldThrowApiException_WhenUnauthorized()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string callbackId = "cb_123";
                var request = new AnswerRequest { Notification = "Test" };

                mockHttp.When(HttpMethod.Post, $"{BaseUrl}/answers?callback_id={callbackId}")
                        .WithContent(JsonSerializer.Serialize(request, JsonOptions))
                        .Respond(HttpStatusCode.Unauthorized);

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act & Assert
                await Assert.ThrowsAsync<ApiException>(() => api.SendAnswerAsync(callbackId, request));
            }

            /// <summary>
            /// Проверяет корректное кодирование параметра callback_id в URL.
            /// </summary>
            [Fact]
            public async Task SendAnswerAsync_ShouldEncodeCallbackId()
            {
                // Arrange
                var mockHttp = new MockHttpMessageHandler();
                string callbackId = "cb 123/?"; // символы, требующие кодирования
                string encodedId = Uri.EscapeDataString(callbackId);

                string? capturedQuery = null;
                mockHttp.Expect(HttpMethod.Post, $"{BaseUrl}/answers*")
                        .Respond(req =>
                        {
                            capturedQuery = req.RequestUri?.Query;
                            return new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(@"{""success"":true}", Encoding.UTF8, "application/json")
                            };
                        });

                var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseUrl) };
                var settings = new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions) };
                var api = RestService.For<IMaxApi>(httpClient, settings);

                // Act
                await api.SendAnswerAsync(callbackId, new AnswerRequest { Notification = "Test" });

                // Assert
                Assert.NotNull(capturedQuery);
                Assert.Contains($"callback_id={encodedId}", capturedQuery);
                mockHttp.VerifyNoOutstandingExpectation();
            }
        }
    }
}