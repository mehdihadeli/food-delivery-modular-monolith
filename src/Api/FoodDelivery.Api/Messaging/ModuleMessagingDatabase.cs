using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Api.Messaging;

public sealed record ModuleMessagingDatabase(DbContext Context);
