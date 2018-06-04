﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.ServiceFabric;
using DC.JobContextManager;
using DC.JobContextManager.Interface;
using ESFA.DC.Auditing;
using ESFA.DC.Auditing.Dto;
using ESFA.DC.Auditing.Interface;
using ESFA.DC.ILR.ValidationService.Modules;
using ESFA.DC.ILR.ValidationService.Modules.PreValidation;
using ESFA.DC.ILR.ValidationService.Stateless.Configuration;
using ESFA.DC.ILR.ValidationService.Stateless.Handlers;
using ESFA.DC.ILR.ValidationService.Stateless.Mapper;
using ESFA.DC.ILR.ValidationService.Stateless.Models;
using ESFA.DC.IO.AzureCosmos;
using ESFA.DC.IO.AzureCosmos.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Mapping.Interface;
using ESFA.DC.Queueing;
using ESFA.DC.Queueing.Interface;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.ServiceFabric.Helpers;

namespace ESFA.DC.ILR.ValidationService.Stateless
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.
                var builder = BuildContainer();

                // Register the Autofac magic for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register the stateless service.
                builder.RegisterStatelessService<Stateless>("ESFA.DC.ILR.ValidationService.StatelessType");

                using (var container = builder.Build())
                {
                    var audi = container.Resolve<IAuditor>();
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Stateless).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static ContainerBuilder BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<PreValidationServiceModule>();

            // get ServiceBus, Azurestorage config values and register container
            var configHelper = new ConfigurationHelper();
            var serviceBusOptions =
                configHelper.GetSectionValues<ServiceBusOptions>("ServiceBusSettings");
            containerBuilder.RegisterInstance(serviceBusOptions).As<ServiceBusOptions>().SingleInstance();

            var azureStorageOptions =
                configHelper.GetSectionValues<AzureStorageModel>("AzureStorageSection");
            containerBuilder.RegisterInstance(azureStorageOptions).As<AzureStorageModel>().SingleInstance();

            // register logger
            var loggerOptions =
                configHelper.GetSectionValues<LoggerOptions>("LoggerSection");
            containerBuilder.RegisterInstance(loggerOptions).As<LoggerOptions>().SingleInstance();
            containerBuilder.RegisterModule<LoggerModule>();

            // register Cosmos config
            var azureCosmosOptions = configHelper.GetSectionValues<AzureCosmosOptions>("AzureCosmosSection");
            containerBuilder.Register(c => new AzureCosmosKeyValuePersistenceConfig(
                    azureCosmosOptions.CosmosEndpointUrl,
                    azureCosmosOptions.CosmosAuthKeyOrResourceToken))
                .As<IAzureCosmosKeyValuePersistenceServiceConfig>().SingleInstance();
            containerBuilder.RegisterType<AzureCosmosKeyValuePersistenceService>().As<IKeyValuePersistenceService>()
                .InstancePerLifetimeScope();

            // register reference data configs
            var referenceDataOptions =
                configHelper.GetSectionValues<ReferenceDataOptions>("ReferenceDataSection");
            containerBuilder.RegisterInstance(referenceDataOptions).As<ReferenceDataOptions>().SingleInstance();

            // service bus queue configuration
            var queueSubscriptionConfig = new ServiceBusQueueConfig(
                serviceBusOptions.ServiceBusConnectionString,
                serviceBusOptions.JobsQueueName,
                Environment.ProcessorCount,
                serviceBusOptions.TopicName);

            var topicPublishConfig = new ServiceBusTopicConfiguration(
                serviceBusOptions.ServiceBusConnectionString,
                serviceBusOptions.TopicName,
                serviceBusOptions.FundingCalcSubscriptionName,
                Environment.ProcessorCount);

            var auditPublishConfig = new ServiceBusQueueConfig(
                serviceBusOptions.ServiceBusConnectionString,
                serviceBusOptions.AuditQueueName,
                Environment.ProcessorCount);

            // register queue services
            containerBuilder.Register(c =>
            {
                var queueSubscriptionService =
                    new QueueSubscriptionService<JobContextDto>(
                        queueSubscriptionConfig,
                        c.Resolve<IJsonSerializationService>(),
                        c.Resolve<ILogger>());
                return queueSubscriptionService;
            }).As<IQueueSubscriptionService<JobContextDto>>();

            containerBuilder.Register(c =>
            {
                var topicPublishService =
                    new TopicPublishService<JobContextDto>(
                        topicPublishConfig,
                        c.Resolve<IJsonSerializationService>());
                return topicPublishService;
            }).As<ITopicPublishService<JobContextDto>>();

            containerBuilder.Register(c => new QueuePublishService<AuditingDto>(
                    auditPublishConfig,
                    c.Resolve<IJsonSerializationService>()))
                .As<IQueuePublishService<AuditingDto>>();

            // register job context manager
            containerBuilder.RegisterType<Auditor>().As<IAuditor>();
            containerBuilder.RegisterType<JobContextMessageMapper>()
                .As<IMapper<JobContextMessage, JobContextMessage>>();

            containerBuilder.RegisterType<MessageHandler>().As<IMessageHandler>();

            // register the  callback handle when a new message is received from ServiceBus
            containerBuilder.Register<Func<JobContextMessage, CancellationToken, Task<bool>>>(c =>
                c.Resolve<IMessageHandler>().Handle);

            containerBuilder.RegisterType<JobContextManager<JobContextMessage>>().As<IJobContextManager>();

            // register key generator
            containerBuilder.RegisterType<KeyGenerator.KeyGenerator>().As<IKeyGenerator>().SingleInstance();

            return containerBuilder;
        }
    }
}
