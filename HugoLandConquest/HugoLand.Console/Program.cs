using HugoLand.Core.Data;
using System;

namespace HugoLand;

internal class Program
{
    static void Main(string[] args)
    {
        using var context = new HugoLandContextFactory().CreateDbContext([]);

    }
}
