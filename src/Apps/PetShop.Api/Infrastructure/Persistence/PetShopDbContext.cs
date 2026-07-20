using Microsoft.EntityFrameworkCore;

namespace PetShop.Api.Infrastructure.Persistence;

public sealed class PetShopDbContext(DbContextOptions<PetShopDbContext> options) : DbContext(options);
