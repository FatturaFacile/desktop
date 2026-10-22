// <copyright file="Utils.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatturaFacile.Common.Utils;

public static class Logger {
    public static void Log(string message) {
#if !NET
        Xpf.DemoBase.Helpers.Logger.Log(message);
#endif
    }
}
