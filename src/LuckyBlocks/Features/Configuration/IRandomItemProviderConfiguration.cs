using System.Collections.Generic;

namespace LuckyBlocks.Features.Configuration;

internal interface IRandomItemProviderConfiguration : IConfiguration
{
    IEnumerable<string> ExcludedItems { get; set; }
}