﻿using System.Collections.Generic;
using System.Text;

namespace PaaS.Ticketing.ApiLib.Models
{
    internal class LoggerState : Dictionary<string, object>
    {
        const string seperator = " => ";
        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var item in this)
                builder.Append($"{item.Key}:{item.Value}{seperator}");

            return builder.ToString().Replace(seperator, string.Empty);
        }
    }
}
