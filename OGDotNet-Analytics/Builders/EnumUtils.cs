//-----------------------------------------------------------------------
// <copyright file="EnumUtils.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace OGDotNet.Builders
{
    public static class EnumUtils<TA, TB>
        where TA : struct
        where TB : struct
    {
        private static readonly Dictionary<TA, TB> LookupTable;

        static EnumUtils()
        {
            var values = Enum.GetValues(typeof(TA)).Cast<TA>();
            LookupTable = new Dictionary<TA, TB>();
            foreach (var a in values)
            {
                TB b;
                if (Enum.TryParse(a.ToString(), out b))
                {
                    LookupTable.Add(a, b);
                }
            }
        }

        public static TB ConvertTo(TA a)
        {
            return LookupTable[a];
        }
    }
}