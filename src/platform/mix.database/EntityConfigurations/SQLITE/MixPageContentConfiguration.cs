﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mix.Database.Entities.Cms;
using Mix.Database.EntityConfigurations.SQLITE.Base;
using Mix.Shared.Enums;

namespace Mix.Database.EntityConfigurations.SQLITE
{
    public class MixPageContentConfiguration : SqliteMultilanguageSEOContentBaseConfiguration<MixPageContent, int>
    {
        public override void Configure(EntityTypeBuilder<MixPageContent> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.ClassName)
                .HasColumnType($"{Config.String}{Config.SmallLength}")
                .HasCharSet(Config.CharSet);

            builder.Property(e => e.Type)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<MixPageType>())
                .HasColumnType($"{Config.String}{Config.SmallLength}")
                .HasCharSet(Config.CharSet);
        }
    }
}
