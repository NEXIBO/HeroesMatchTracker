﻿using HeroesMatchTracker.Infrastructure.Database.Contexts;
using System;

namespace HeroesMatchTracker.Core.Database.HeroesReplays
{
    public interface IReplayMatchRepository
    {
        bool IsExists(HeroesReplaysDbContext context, string hash);

        bool IsExists(HeroesReplaysDbContext context, long replayId);

        DateTime? GetLastestReplayTimeStamp(HeroesReplaysDbContext context);
    }
}