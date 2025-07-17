using System;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Utils;
using OneOf.Types;
using Serilog;

namespace LuckyBlocks.Loot;

internal interface IRandomItemProvider
{
    Item GetRandomItem(LuckyBlockBrokenArgs args);
}

internal class RandomItemProvider : IRandomItemProvider
{
    private readonly IIdentityService _identityService;
    private readonly IAttributesChecker _attributesChecker;
    private readonly ILogger _logger;

    public RandomItemProvider(IIdentityService identityService, IAttributesChecker attributesChecker, ILogger logger)
        => (_identityService, _attributesChecker, _logger) = (identityService, attributesChecker, logger);

    public Item GetRandomItem(LuckyBlockBrokenArgs args)
    {
        var player = args.IsPlayer ? _identityService.GetPlayerById(args.PlayerId) : new Unknown();

        var luckyBlockItemValues = Enum
            .GetValues(typeof(Item))
            .Cast<Item>()
            .Where(x => _attributesChecker.Check(x, player))
            .ToList();

        // _logger.Information($"{string.Join(", ", luckyBlockItemValues)}");

        var alwaysItem = luckyBlockItemValues.SingleOrDefault(EnumUtils.AttributeExist<AlwaysAttribute, Item>);
        if (alwaysItem != Item.None)
            return alwaysItem;

        return luckyBlockItemValues
            .GetWeightedRandomElement(GetItemWeight);
    }

    private static double GetItemWeight(Item item) =>
        EnumUtils.AttributeExist<WeightAttribute, Item>(item, out var attribute)
            ? attribute.Weight
            : 1;
}