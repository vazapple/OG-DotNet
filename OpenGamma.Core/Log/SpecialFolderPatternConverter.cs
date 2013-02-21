﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecialFolderPatternConverter.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace OpenGamma.Log
{
    public class SpecialFolderPatternConverter : log4net.Util.PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            var specialFolder = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), Option, true);

            writer.Write(Environment.GetFolderPath(specialFolder));
        }
    }
}