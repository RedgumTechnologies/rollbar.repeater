using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace redgum.rollbar.repeater.Services
{
    public static class RollbarConfigProvider
    {
        public static Rollbar.RollbarConfig GetRollbarConfig()
        {
            //this gets the rollbar config for posting to Real Rollbar
            //so the access token that is configured should be the one for Redgum.Rollbar.Repeater
            //not the token for whatever app(s) we might be repeating for
            //same can be said for environment
            return new Rollbar.RollbarConfig(AppSettingsProvider.GetSetting("Rollbar.AccessToken"))
            {
                Environment = AppSettingsProvider.GetSetting("Rollbar.Environment")
            };
        }
    }

    public static class RollbarRepeaterAccessTokenSettingsProvider
    {
        public static string GetFullName(string name)
        {
            return $"AccessToken.{name}";
        }
        public static string GetSetting(string name, string fallback)
        {
            var fullName = GetFullName(name);
            return RollbarRepeaterSettingsProvider.GetSetting(fullName, fallback);
        }
        public static string GetSetting(string name)
        {
            var fullName = GetFullName(name);
            return RollbarRepeaterSettingsProvider.GetSetting(fullName);
        }
    }
    public static class RollbarRepeaterSettingsProvider
    {
        public static string GetFullName(string name)
        {
            return $"RollbarRepeater.{name}";
        }
        public static string GetSetting(string name, string fallback)
        {
            var fullName = GetFullName(name);
            return AppSettingsProvider.GetSetting(fullName, fallback);
        }
        public static string GetSetting(string name)
        {
            var fullName = GetFullName(name);
            return AppSettingsProvider.GetSetting(fullName);
        }
    }
    public static class AppSettingsProvider
    {
        public static string GetSetting(string name, string fallback)
        {
            var setting = GetSetting(name);
            return string.IsNullOrEmpty(setting) ? fallback : setting;
        }
        public static string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}