using Auto_Garage.Data.AutoGarageAuthDb;
using Microsoft.EntityFrameworkCore;

namespace Auto_Garage.BackgroundServices
{
    public class TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Token Cleanup Service started.");

            // Track last time each cleanup ran
            var lastRefreshTokenCleanup = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AutoGarageAuthDbContext>();

                    var now = DateTime.UtcNow;

                    // Clean blacklisted JWTs every hour
                    var expiredBlacklistedTokens = await dbContext.BlacklistedTokens
                        .Where(t => t.ExpiresAt < now)
                        .ToListAsync(stoppingToken);

                    if (expiredBlacklistedTokens.Count != 0)
                    {
                        dbContext.BlacklistedTokens.RemoveRange(expiredBlacklistedTokens);
                        logger.LogInformation($"Cleaned up {expiredBlacklistedTokens.Count} expired blacklisted tokens.");
                    }

                    // Clean refresh tokens only once per day — they live 7 days anyway
                    if ((now - lastRefreshTokenCleanup).TotalHours >= 24)
                    {
                        var expiredRefreshTokens = await dbContext.RefreshTokens
                            .Where(t => t.ExpiresAt < now || t.IsRevoked)
                            .ToListAsync(stoppingToken);

                        if (expiredRefreshTokens.Count != 0)
                        {
                            dbContext.RefreshTokens.RemoveRange(expiredRefreshTokens);
                            logger.LogInformation($"Cleaned up {expiredRefreshTokens.Count} expired/revoked refresh tokens.");
                        }

                        lastRefreshTokenCleanup = now;
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred during token cleanup.");
                }

                // Runs every 1 hour — good balance for 15min JWT expiry
                
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
                logger.LogInformation("Token Cleanup Service loop completed.");

            }
        }

        // Optional: Log when the service is stopping

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Token Cleanup Service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}
