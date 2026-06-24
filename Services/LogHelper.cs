using System;
using System.Web;

namespace SmboPhdService.Services
{
    public static class LogHelper
    {
        public static string RequestIp => HttpContext.Current?.Request?.UserHostAddress ?? "unknown";

        public static string Requestid
        {
            get
            {
                var context = HttpContext.Current;

                if (context != null)
                {
                    if (!context.Items.Contains("Rid"))
                        context.Items["Rid"] = GenerateId();
                    return context.Items["Rid"].ToString();
                }

                return GenerateId();
            }
        }

        private static string GenerateId() => Guid.NewGuid().ToString().Substring(0, 5);

    }
}