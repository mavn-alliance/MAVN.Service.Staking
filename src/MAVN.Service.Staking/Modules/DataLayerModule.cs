using Autofac;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using MAVN.Service.Staking.Domain.Repositories;
using MAVN.Service.Staking.MsSqlRepositories;
using MAVN.Service.Staking.MsSqlRepositories.Repositories;
using MAVN.Service.Staking.Settings;
using Lykke.SettingsReader;

namespace MAVN.Service.Staking.Modules
{
    [UsedImplicitly]
    public class DataLayerModule : Module
    {
        private readonly DbSettings _settings;

        public DataLayerModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.StakingService.Db;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _settings.DataConnString,
                connString => new StakingContext(connString, false),
                dbConn => new StakingContext(dbConn));

            builder.RegisterType<ReferralStakesRepository>()
                .As<IReferralStakesRepository>()
                .SingleInstance();

            builder.RegisterType<StakesBlockchainDataRepository>()
                .As<IStakesBlockchainDataRepository>()
                .SingleInstance();
        }
    }
}
