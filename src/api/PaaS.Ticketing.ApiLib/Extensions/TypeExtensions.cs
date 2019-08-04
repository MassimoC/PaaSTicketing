using PaaS.Ticketing.ApiLib.DTOs;
using PaaS.Ticketing.ApiLib.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PaaS.Ticketing.ApiLib.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNullOrEmpty<T>(this T value)
        {
            if (typeof(T) == typeof(string)) return string.IsNullOrEmpty(value as string);

            return value == null || value.Equals(default(T));
        }

        public static dynamic Cast(dynamic obj, Type castTo)
        {
            return Convert.ChangeType(obj, castTo);
        }

        public static string[] GetFilledProperties<T>(this T value)
        {
            if (value.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(value), $"{nameof(value)} cannot be null.");

            bool PropertyHasValue(PropertyInfo prop)
            {
                if (prop.GetValue(value) is IList list && list.Count == 0) return false;

                return prop.GetValue(value) == null
                    ? false
                    : !IsNullOrEmpty(Cast(prop.GetValue(value), prop.GetValue(value).GetType()));
            }

            return value.GetType()
                        .GetProperties()
                        .Where(PropertyHasValue)
                        .Select(p => p.Name)
                        .ToArray();
        }

        public static bool Between(this int num, int lower, int upper, bool inclusive = false)
        {
            return inclusive
                ? lower <= num && num <= upper
                : lower < num && num < upper;
        }
    }

    public static class Transformer
    {
        public static ConcertDto Transform<TDestination>(object source)
        {
            var src = (Concert)source; 
            var tickets = new List<TicketDto>();
            foreach (var item in src.ConcertUser)
            {
                if (src.ConcertUser != null) tickets.Add(new TicketDto { Email = item.User.Email, Token = item.Token });
            };

            return new ConcertDto
            {
                ConcertId = src.ConcertId,
                Name = src.Name,
                Price = src.Price,
                Location = src.Location,
                From = src.From,
                To = src.To,
                Tickets = tickets
            };
        }
    }
}
