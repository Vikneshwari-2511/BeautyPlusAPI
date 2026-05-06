namespace BeautyPlusParlour.Helpers;

public static class UserAgentHelper
{
    public static string ParseBrowser(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "Unknown";

        if (userAgent.Contains("Edg/")) return "Edge";
        if (userAgent.Contains("OPR/") ||
            userAgent.Contains("Opera")) return "Opera";
        if (userAgent.Contains("Chrome")) return "Chrome";
        if (userAgent.Contains("Firefox")) return "Firefox";
        if (userAgent.Contains("Safari")) return "Safari";
        if (userAgent.Contains("curl")) return "curl";
        if (userAgent.Contains("Postman")) return "Postman";

        return "Other";
    }

    public static string ParseDevice(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "Unknown";

        if (userAgent.Contains("iPhone")) return "iPhone";
        if (userAgent.Contains("iPad")) return "iPad";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("Windows")) return "Windows PC";
        if (userAgent.Contains("Macintosh")) return "Mac";
        if (userAgent.Contains("Linux")) return "Linux";

        return "Unknown Device";
    }
}