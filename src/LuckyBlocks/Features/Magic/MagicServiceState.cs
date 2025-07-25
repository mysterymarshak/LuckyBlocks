using System;
using System.Collections.Generic;

namespace LuckyBlocks.Features.Magic;

internal record MagicServiceState(List<IMagic> Magics, bool IsMagicAllowed, TimeSpan MagicProhibitionTime);