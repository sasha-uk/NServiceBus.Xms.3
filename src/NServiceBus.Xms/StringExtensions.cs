﻿using System;
using System.Globalization;

namespace NServiceBus.Xms
{
    public static class StringExtensions
    {
        public static string FormatWith(this string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}