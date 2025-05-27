using Microsoft.AspNetCore.Http;
using GeeYeangSore.Models;
using System;

namespace GeeYeangSore.APIControllers.Session
{
    public static class SessionManager
    {
        public static void SetLogin(HttpContext context, HTenant tenant)
        {
            context.Session.SetString(CDictionary.SK_LOGINED_USER, tenant.HEmail);
            context.Session.SetString(CDictionary.SK_LOGINED_ROLE, "User");
            context.Session.SetString(CDictionary.SK_LOGINED_TYPE, "Tenant");
            context.Session.SetString("LoginTime", DateTimeOffset.UtcNow.ToString("o"));
            context.Session.SetInt32("TenantId", tenant.HTenantId);
        }

        public static bool IsLoggedIn(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Session.GetString(CDictionary.SK_LOGINED_USER));
        }

        public static void Clear(HttpContext context)
        {
            context.Session.Clear();
        }

        public static void SetFrontLogin(HttpContext context, HTenant tenant)
        {
            context.Session.SetString(CDictionary.SK_FRONT_LOGINED_USER, tenant.HEmail);
            context.Session.SetString(CDictionary.SK_FRONT_LOGINED_ROLE, "User");
            context.Session.SetString(CDictionary.SK_FRONT_LOGINED_TYPE, "Tenant");
            context.Session.SetString("FrontLoginTime", DateTimeOffset.UtcNow.ToString("o"));
            context.Session.SetInt32("FrontTenantId", tenant.HTenantId);
        }

        public static bool IsFrontLoggedIn(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Session.GetString(CDictionary.SK_FRONT_LOGINED_USER));
        }

        public static void ClearFront(HttpContext context)
        {
            context.Session.Remove(CDictionary.SK_FRONT_LOGINED_USER);
            context.Session.Remove(CDictionary.SK_FRONT_LOGINED_ROLE);
            context.Session.Remove(CDictionary.SK_FRONT_LOGINED_TYPE);
            context.Session.Remove("FrontLoginTime");
            context.Session.Remove("FrontTenantId");
        }
    }
}