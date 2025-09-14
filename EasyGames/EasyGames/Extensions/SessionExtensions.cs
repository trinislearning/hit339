﻿using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace EasyGames.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value) =>
            session.SetString(key, JsonSerializer.Serialize(value));

        public static T? GetObject<T>(this ISession session, string key)
        {
            var s = session.GetString(key);
            return string.IsNullOrEmpty(s) ? default : JsonSerializer.Deserialize<T>(s);
        }
    }
}
