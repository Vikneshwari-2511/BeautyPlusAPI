using BeautyPlusParlour.Constants;
using BeautyPlusParlour.Models.Enums;

namespace BeautyPlusParlour.Helpers;

public static class LoyaltyPointsHelper
{
    public static int Calculate(decimal basePrice, ServiceType serviceType)
    {
        var basePoints = (int)Math.Floor(basePrice / ServiceConstants.LoyaltyPointsDivisor);

        return serviceType == ServiceType.OnSite
            ? basePoints * ServiceConstants.OnSiteLoyaltyMultiplier
            : basePoints;
    }
}