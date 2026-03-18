using Microsoft.EntityFrameworkCore;
using SaasAsaasApp.Data.Entities;
using SaasAsaasApp.Data.Enums;

namespace SaasAsaasApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if database is already seeded
        if (await context.SubscriptionPlans.AnyAsync())
            return;

        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                InternalName = "plan_basic_monthly",
                DisplayName = "Basic Plan",
                Price = 19.90m,
                Currency = "BRL",
                BillingCycle = BillingCycle.Monthly,
                MaxUsers = 3,
                MaxProjects = 5,
                FeaturesJson = "{\"api_access\": false, \"support\": \"email\"}",
                IsActive = true
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                InternalName = "plan_pro_monthly",
                DisplayName = "Professional Plan",
                Price = 49.90m,
                Currency = "BRL",
                BillingCycle = BillingCycle.Monthly,
                MaxUsers = 10,
                MaxProjects = 25,
                FeaturesJson = "{\"api_access\": true, \"support\": \"priority\"}",
                IsActive = true
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                InternalName = "plan_enterprise_monthly",
                DisplayName = "Enterprise Plan",
                Price = 99.90m,
                Currency = "BRL",
                BillingCycle = BillingCycle.Monthly,
                MaxUsers = 100,
                MaxProjects = 1000,
                FeaturesJson = "{\"api_access\": true, \"support\": \"dedicated\"}",
                IsActive = true
            }
        };

        await context.SubscriptionPlans.AddRangeAsync(plans);
        await context.SaveChangesAsync();
    }
}
