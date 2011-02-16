﻿using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace OGDotNet.Tests.Integration.Xunit.Extensions
{
    public class TheoryAttribute : global::Xunit.Extensions.TheoryAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var enumerateTestCommands = base.EnumerateTestCommands(method);
            return enumerateTestCommands.Select(cmd => new CustomizingCommand(cmd));
        }
    }
} 