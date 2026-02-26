using MaxBot.Models;
using MaxBot.Models.MaxRefitModels.Answers;
using MaxBot.Models.MaxRefitModels.Chat;
using MaxBot.Models.MaxRefitModels.Message;
using MaxBot.Models.MaxRefitModels.NewMessage;
using MaxBot.Models.MaxRefitModels.Requests;
using MaxBot.Models.MaxRefitModels.Responces;
using MaxBot.Models.MaxRefitModels.Users;
using Refit;

namespace MaxBot.Client;

/// <summary>
/// Интерфейс для взаимодействия с API MAX.
/// Все методы требуют авторизации через заголовок Authorization с токеном бота.
/// </summary>
public interface IMaxApi
{
    /// <summary>
    /// Получить информацию о текущем боте.
    /// </summary>
    /// <returns>Объект BotInfo с данными бота.</returns>
    [Get("/me")]
    Task<BotInfo> GetMeAsync();
    
    /// <summary>
    /// Получить список групповых чатов, в которых участвовал бот.
    /// </summary>
    /// <param name="count">Количество чатов (1–100, по умолчанию 50)</param>
    /// <param name="marker">Маркер следующей страницы (null для первой страницы)</param>
    /// <returns>Список чатов и маркер для следующей страницы</returns>
    [Get("/chats")]
    Task<GetChatsResponse> GetChatsAsync(
        [AliasAs("count")] int? count = null,
        [AliasAs("marker")] long? marker = null
    );
    
    
    /// <summary>
    /// Получить детальную информацию о групповом чате по его идентификатору.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <returns>Объект Chat с данными о групповом чате.</returns>
    [Get("/chats/{chatId}")]
    Task<Chat> GetGroupChatByIdAsync(long chatId);
    
    /// <summary>
    /// Изменить информацию о групповом чате (название, иконку, закреплённое сообщение).
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <param name="request">Данные для изменения (все поля опциональны).</param>
    /// <returns>Обновлённый объект Chat.</returns>
    /// <remarks>
    /// - Для установки иконки передаётся объект PhotoAttachmentRequestPayload (url, token или photos).
    /// - Чтобы закрепить сообщение, передайте его ID в поле pin.
    /// - Удаление закреплённого сообщения выполняется отдельным методом unpin.
    /// </remarks>
    [Patch("/chats/{chatId}")]
    Task<Chat> UpdateGroupChatAsync(long chatId, [Body] UpdateChatRequest request);
    
    /// <summary>
    /// Удалить групповой чат для всех участников.
    /// </summary>
    /// <param name="chatId">ID удаляемого чата.</param>
    /// <returns>Объект с полем success (true при успехе) и опциональным сообщением.</returns>
    [Delete("/chats/{chatId}")]
    Task<ApiResultResponce> DeleteGroupChatAsync(long chatId);
    
    
    /// <summary>
    /// Отправить действие бота в групповой чат (например, "набор текста").
    /// </summary>
    /// <param name="chatId">ID чата.</param>
    /// <param name="request">Объект с полем action.</param>
    /// <returns>Объект с полем success и опциональным сообщением.</returns>
    [Post("/chats/{chatId}/actions")]
    Task<ApiResultResponce> SendBotActionAsync(long chatId, [Body] SenderActionRequest request);
    
    
    /// <summary>
    /// Получить закреплённое сообщение в групповом чате.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <returns>Объект с полем message, содержащим закреплённое сообщение или null.</returns>
    [Get("/chats/{chatId}/pin")]
    Task<GetPinnedMessageResponse> GetPinnedMessageAsync(long chatId);
    
    /// <summary>
    /// Удалить закреплённое сообщение в групповом чате.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <returns>Объект с полем success (true при успехе) и опциональным сообщением об ошибке.</returns>
    [Delete("/chats/{chatId}/pin")]
    Task<ApiResultResponce> UnpinMessageAsync(long chatId);
    
    /// <summary>
    /// Получить информацию о членстве текущего бота в групповом чате.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <returns>Объект ChatMember с данными о боте как участнике чата (роль, права, время вступления и т.д.).</returns>
    /// <remarks>
    /// Если бот не является участником чата, API вернёт 404 Not Found.
    /// </remarks>
    [Get("/chats/{chatId}/members/me")]
    Task<ChatMember> GetMyChatMembershipAsync(long chatId);
    
    /// <summary>
    /// Удалить бота из участников группового чата (покинуть чат).
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата, который бот покидает.</param>
    /// <returns>Объект с полем success (true при успешном выходе) и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// Если бот не является участником чата, API вернёт success = false с соответствующим сообщением.
    /// </remarks>
    [Delete("/chats/{chatId}/members/me")]
    Task<ApiResultResponce> LeaveChatAsync(long chatId);
    
    /// <summary>
    /// Получить список всех администраторов группового чата.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <returns>Список администраторов и маркер для пагинации.</returns>
    /// <remarks>
    /// Бот должен быть администратором в запрашиваемом чате, иначе вернётся ошибка доступа.
    /// </remarks>
    [Get("/chats/{chatId}/members/admins")]
    Task<GetChatAdminsResponse> GetChatAdminsAsync(long chatId);
    
    /// <summary>
    /// Назначить администраторов в групповом чате.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <param name="request">Список пользователей с правами, которые становятся администраторами.</param>
    /// <returns>Объект с полем success (true при успешном назначении всех) и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// Бот должен сам быть администратором с правом "add_admins", чтобы выполнить этот запрос.
    /// </remarks>
    [Post("/chats/{chatId}/members/admins")]
    Task<ApiResultResponce> AddChatAdminsAsync(long chatId, [Body] AddAdminsRequest request);
    
    /// <summary>
    /// Отменить права администратора у пользователя в групповом чате.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <param name="userId">Идентификатор пользователя, у которого отменяются права администратора.</param>
    /// <returns>Объект с полем success (true при успешной отмене) и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// Бот должен быть администратором с правом "add_admins", чтобы выполнить этот запрос.
    /// </remarks>
    [Delete("/chats/{chatId}/members/admins/{userId}")]
    Task<ApiResultResponce> RemoveChatAdminAsync(long chatId, long userId);
    
    /// <summary>
    /// Получить список участников группового чата с поддержкой пагинации и фильтрации по ID пользователей.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <param name="userIds">Опциональный список ID пользователей для фильтрации (если указан, пагинация игнорируется).</param>
    /// <param name="marker">Маркер следующей страницы (для пагинации).</param>
    /// <param name="count">Количество участников на странице (по умолчанию 20, максимум 100).</param>
    /// <returns>Список участников и маркер следующей страницы.</returns>
    /// <remarks>
    /// Если параметр userIds передан, параметры marker и count игнорируются API.
    /// </remarks>
    [Get("/chats/{chatId}/members")]
    Task<GetChatMembersResponse> GetChatMembersAsync(
        long chatId,
        [AliasAs("user_ids")] List<long>? userIds = null,
        [AliasAs("marker")] long? marker = null,
        [AliasAs("count")] int? count = null);
    
    /// <summary>
    /// Добавить участников в групповой чат.
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <param name="request">Массив идентификаторов добавляемых пользователей.</param>
    /// <returns>Объект с полем success и опциональным сообщением об ошибке.</returns>
    [Post("/chats/{chatId}/members")]
    Task<AddMembersResponse> AddChatMembersAsync(long chatId, [Body] AddMembersRequest request);
    
    /// <summary>
    /// Удалить участника из группового чата (с возможностью блокировки).
    /// </summary>
    /// <param name="chatId">Идентификатор группового чата.</param>
    /// <param name="userId">Идентификатор удаляемого пользователя.</param>
    /// <param name="block">
    /// Если true, пользователь будет заблокирован в чате.
    /// Применяется только для чатов с публичной или приватной ссылкой.
    /// </param>
    /// <returns>Объект с полем success и опциональным сообщением об ошибке.</returns>
    [Delete("/chats/{chatId}/members")]
    Task<ApiResultResponce> RemoveChatMemberAsync(
        long chatId,
        [AliasAs("user_id")] long userId,
        [AliasAs("block")] bool? block = null);
    
    /// <summary>
    /// Получить список всех подписок бота.
    /// </summary>
    /// <returns>Список подписок с URL, временем и типами обновлений.</returns>
    [Get("/subscriptions")]
    Task<GetSubscriptionsResponse> GetSubscriptionsAsync();
    
    /// <summary>
    /// Создать подписку на получение обновлений через Webhook.
    /// </summary>
    /// <param name="request">Параметры подписки: URL, типы обновлений, секрет.</param>
    /// <returns>Объект с полем success и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// После успешного вызова бот будет получать уведомления о событиях на указанный URL.
    /// Сервер должен прослушивать один из портов: 80, 8080, 443, 8443, 16384-32383.
    /// </remarks>
    [Post("/subscriptions")]
    Task<ApiResultResponce> CreateSubscriptionAsync([Body] CreateSubscriptionRequest request);
    
    /// <summary>
    /// Удалить подписку на обновления для указанного URL.
    /// </summary>
    /// <param name="url">URL вебхука, который нужно удалить из подписок.</param>
    /// <returns>Объект с полем success и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// После успешного вызова бот перестаёт получать уведомления через Webhook.
    /// Доставка уведомлений становится доступна через API с длительным опросом.
    /// </remarks>
    [Delete("/subscriptions")]
    Task<ApiResultResponce> DeleteSubscriptionAsync([AliasAs("url")] string url);
    
    /// <summary>
    /// Получить обновления (события) через долгий опрос (long polling).
    /// </summary>
    /// <param name="limit">Максимальное количество обновлений (1–1000, по умолчанию 100).</param>
    /// <param name="timeout">Тайм-аут в секундах для долгого опроса (0–90, по умолчанию 30).</param>
    /// <param name="marker">
    /// Указатель на следующую страницу. Если передан, бот получит обновления, которые ещё не были получены.
    /// Если не передан, получит все новые обновления после последнего подтверждения.
    /// </param>
    /// <param name="types">
    /// Список типов обновлений через запятую (например, "message_created,message_callback").
    /// </param>
    /// <returns>Объект со списком обновлений и маркером следующей страницы.</returns>
    /// <remarks>
    /// Метод предназначен для разработки и тестирования. Для production рекомендуется использовать Webhook.
    /// </remarks>
    [Get("/updates")]
    Task<GetUpdatesResponse> GetUpdatesAsync(
        [AliasAs("limit")] int? limit = null,
        [AliasAs("timeout")] int? timeout = null,
        [AliasAs("marker")] long? marker = null,
        [AliasAs("types")] string? types = null);
    
    /// <summary>
    /// Получить URL для последующей загрузки файла.
    /// </summary>
    /// <param name="type">Тип загружаемого файла (image, video, audio, file).</param>
    /// <returns>URL для загрузки и опциональный токен (для video/audio).</returns>
    /// <remarks>
    /// После получения URL необходимо выполнить POST-запрос на этот URL с файлом в формате multipart/form-data.
    /// Для видео и аудио после загрузки придёт токен, который нужно использовать в сообщении.
    /// </remarks>
    [Post("/uploads")]
    Task<GetUploadUrlResponse> GetUploadUrlAsync([AliasAs("type")] UploadType type);
    
    
    /// <summary>
    /// Получить сообщения из чата или по списку ID.
    /// </summary>
    /// <param name="chatId">ID чата для получения сообщений.</param>
    /// <param name="messageIds">Список ID сообщений через запятую (например, "msg1,msg2").</param>
    /// <param name="from">Начало временного диапазона (Unix timestamp).</param>
    /// <param name="to">Конец временного диапазона (Unix timestamp).</param>
    /// <param name="count">Максимальное количество сообщений (1–100, по умолчанию 50).</param>
    /// <returns>Массив сообщений.</returns>
    /// <remarks>
    /// Обязательно указать один из параметров: chatId или messageIds.
    /// Сообщения возвращаются в обратном хронологическом порядке (последние первыми).
    /// </remarks>
    [Get("/messages")]
    Task<GetMessagesResponse> GetMessagesAsync(
        [AliasAs("chat_id")] long? chatId = null,
        [AliasAs("message_ids")] string? messageIds = null,
        [AliasAs("from")] long? from = null,
        [AliasAs("to")] long? to = null,
        [AliasAs("count")] int? count = null);
    
    /// <summary>
    /// Отправить сообщение пользователю или в чат.
    /// </summary>
    /// <param name="request">Тело сообщения.</param>
    /// <param name="userId">ID пользователя (если сообщение личное).</param>
    /// <param name="chatId">ID чата (если сообщение в группу).</param>
    /// <param name="disableLinkPreview">Если true, превью ссылок не генерируется.</param>
    /// <returns>Отправленное сообщение.</returns>
    [Post("/messages")]
    Task<SendMessageResponse> SendMessageAsync(
        [Body] NewMessageBody request,
        [AliasAs("user_id")] long? userId = null,
        [AliasAs("chat_id")] long? chatId = null,
        [AliasAs("disable_link_preview")] bool? disableLinkPreview = null);
    
    /// <summary>
    /// Редактировать сообщение в чате (только для сообщений, отправленных менее 24 часов назад).
    /// </summary>
    /// <param name="messageId">ID редактируемого сообщения.</param>
    /// <param name="request">Новые данные сообщения (текст, вложения, ссылка, уведомление, формат).</param>
    /// <returns>Объект с полем success и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// - Если attachments = null, вложения не изменяются.
    /// - Если attachments = пустой список, все вложения удаляются.
    /// </remarks>
    [Put("/messages")]
    Task<ApiResultResponce> EditMessageAsync(
        [AliasAs("message_id")] string messageId,
        [Body] NewMessageBody request);
    
    /// <summary>
    /// Удалить сообщение в диалоге или чате.
    /// </summary>
    /// <param name="messageId">ID удаляемого сообщения.</param>
    /// <returns>Объект с полем success и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// Можно удалять только сообщения, отправленные менее 24 часов назад.
    /// </remarks>
    [Delete("/messages")]
    Task<ApiResultResponce> DeleteMessageAsync([AliasAs("message_id")] string messageId);
    
    /// <summary>
    /// Получить сообщение по его ID.
    /// </summary>
    /// <param name="messageId">ID сообщения (mid).</param>
    /// <returns>Объект Message.</returns>
    [Get("/messages/{messageId}")]
    Task<Message> GetMessageByIdAsync(string messageId);
    
    /// <summary>
    /// Получить подробную информацию о видео по его токену.
    /// </summary>
    /// <param name="videoToken">Токен видео-вложения.</param>
    /// <returns>Информация о видео: токен, URL-ы воспроизведения, метаданные, миниатюра.</returns>
    [Get("/videos/{videoToken}")]
    Task<VideoInfo> GetVideoInfoAsync(string videoToken);
    
    /// <summary>
    /// Отправить ответ на callback после нажатия кнопки.
    /// </summary>
    /// <param name="callbackId">Идентификатор кнопки (из Update с типом message_callback).</param>
    /// <param name="request">Данные ответа: новое сообщение и/или уведомление.</param>
    /// <returns>Объект с полем success и опциональным сообщением об ошибке.</returns>
    /// <remarks>
    /// Можно обновить сообщение (передать message) или показать одноразовое уведомление (передать notification), либо и то, и другое.
    /// </remarks>
    [Post("/answers")]
    Task<ApiResultResponce> SendAnswerAsync(
        [AliasAs("callback_id")] string callbackId,
        [Body] AnswerRequest request);
}