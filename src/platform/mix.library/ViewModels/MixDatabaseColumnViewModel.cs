﻿using Mix.Shared.Models;
using Newtonsoft.Json;

namespace Mix.Lib.ViewModels
{
    public class MixDatabaseColumnViewModel
        : ViewModelBase<MixCmsContext, MixDatabaseColumn, int, MixDatabaseColumnViewModel>
    {
        #region Properties
        public string SystemName { get; set; }
        public string DisplayName { get; set; }
        public string MixDatabaseName { get; set; }
        public MixDataType DataType { get; set; } = MixDataType.Text;
        public int? ReferenceId { get; set; }

        public string DefaultValue { get; set; }
        public int MixDatabaseId { get; set; }
        [JsonIgnore]
        public string Configurations { get; set; }
        public ColumnConfigurations ColumnConfigurations { get; set; }
        #endregion

        #region Contructors

        public MixDatabaseColumnViewModel()
        {
        }

        public MixDatabaseColumnViewModel(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
        }

        public MixDatabaseColumnViewModel(
            MixDatabaseColumn entity,

            UnitOfWorkInfo uowInfo = null) : base(entity, uowInfo)
        {
        }
        #endregion

        #region Overrides

        public override Task<MixDatabaseColumn> ParseEntity()
        {
            ColumnConfigurations ??= new();
            Configurations = JsonConvert.SerializeObject(
                    ColumnConfigurations, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
            return base.ParseEntity();
        }

        public override void ParseView<TSource>(TSource sourceObject)
        {
            base.ParseView(sourceObject);
            ColumnConfigurations = Configurations != null
                        ? JsonConvert.DeserializeObject<ColumnConfigurations>(Configurations)
                        : new();
        }

        #endregion
    }
}