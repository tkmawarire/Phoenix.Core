﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mix.Database.Entities.Base;
using System;

namespace Mix.Database.EntityConfigurations.Base
{
    public abstract class AssociationBaseConfiguration<T, TPrimaryKey, TConfig> : EntityBaseConfiguration<T, TPrimaryKey, TConfig>
        where TPrimaryKey : IComparable
        where T : AssociationBase<TPrimaryKey>
        where TConfig : IDatabaseConstants
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);
        }

    }
}
