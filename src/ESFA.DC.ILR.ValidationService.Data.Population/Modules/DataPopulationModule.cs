﻿using Autofac;
using ESFA.DC.ILR.Model;
using ESFA.DC.ILR.ReferenceDataService.Model;
using ESFA.DC.ILR.ValidationService.Data.Population.FileProvider;
using ESFA.DC.ILR.ValidationService.Data.Population.Interface;
using ESFA.DC.ILR.ValidationService.Interface;

namespace ESFA.DC.ILR.ValidationService.Data.Population.Modules
{
    public class DataPopulationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExternalDataCachePopulationService>().As<IExternalDataCachePopulationService>().InstancePerLifetimeScope();
            builder.RegisterType<FileDataCachePopulationService>().As<IFileDataCachePopulationService>().InstancePerLifetimeScope();
            builder.RegisterType<InternalDataCachePopulationService>().As<IInternalDataCachePopulationService>().InstancePerLifetimeScope();
            builder.RegisterType<MessageCachePopulationService>().As<IMessageCachePopulationService>().InstancePerLifetimeScope();
            builder.RegisterType<LearnerReferenceDataCachePopulationService>().As<ILearnerReferenceDataCachePopulationService>().InstancePerLifetimeScope();
            builder.RegisterType<PreValidationPopulationService>().As<IPopulationService>().InstancePerLifetimeScope();

            builder.RegisterType<JsonFileProviderService<ReferenceDataRoot>>().As<IFileProvider<ReferenceDataRoot>>().InstancePerLifetimeScope();
            builder.RegisterType<XmlFileProviderService<Message>>().As<IFileProvider<Message>>().InstancePerLifetimeScope();
        }
    }
}
