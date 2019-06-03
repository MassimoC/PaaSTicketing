﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Api.Extensions
{
    public static class CustomExtensions
    {

        private static Random random = new Random();
        public static string RandomString(this String str)
        {
            return new string(Enumerable.Repeat(str, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
