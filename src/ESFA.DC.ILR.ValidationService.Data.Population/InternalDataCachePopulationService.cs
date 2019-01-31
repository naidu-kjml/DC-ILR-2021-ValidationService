﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.Interface;
using ESFA.DC.ILR.ValidationService.Data.Internal;
using ESFA.DC.ILR.ValidationService.Data.Internal.AcademicYear.Model;
using ESFA.DC.ILR.ValidationService.Data.Internal.Model;
using ESFA.DC.ILR.ValidationService.Data.Population.Interface;
using ESFA.DC.ILR.ValidationService.Utility;

namespace ESFA.DC.ILR.ValidationService.Data.Population
{
    /// <summary>
    /// the internal data cache factory class
    /// this has had to be registered under two contracts as the <seealso cref="IInternalDataCachePopulationService"/>
    /// is not available for injection into consuming services.
    /// once the pattern is fixed the <seealso cref="IInternalDataCachePopulationService"/>
    /// contract should be removed.
    /// </summary>
    /// <seealso cref="IInternalDataCachePopulationService" />
    /// <seealso cref="ICreateInternalDataCache" />
    public class InternalDataCachePopulationService :
        IInternalDataCachePopulationService,
        ICreateInternalDataCache
    {
        private readonly string resourceName = "ESFA.DC.ILR.ValidationService.Data.Population.Files.Lookups.xml";
        private readonly IInternalDataCache _internalDataCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalDataCachePopulationService"/> class.
        /// you don't inject a model item into a factory for composition, and then cast it into something in order to 'mutate' it...
        /// </summary>
        /// <param name="internalDataCache">The internal data cache.</param>
        public InternalDataCachePopulationService(IInternalDataCache internalDataCache)
        {
            _internalDataCache = internalDataCache;
        }

        /// <summary>
        /// Populates this instance.
        /// and this routine needs to go... the pattern is wrong.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task</returns>
        public async Task PopulateAsync(CancellationToken cancellationToken)
        {
            var internalDataCache = (InternalDataCache)_internalDataCache;
            Build(internalDataCache);
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns>an internal data cache</returns>
        public IInternalDataCache Create()
        {
            var internalDataCache = new InternalDataCache();
            Build(internalDataCache);

            return internalDataCache;
        }

        /// <summary>
        /// Builds the cache.
        /// </summary>
        /// <param name="cache">The cache.</param>
        public void Build(InternalDataCache cache)
        {
            var assembly = Assembly.GetAssembly(typeof(InternalDataCachePopulationService));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                var lookups = XElement.Load(stream);

                cache.AcademicYear = BuildAcademicYear();

                Enum.GetValues(typeof(LookupSimpleKey))
                    .OfType<LookupSimpleKey>()
                    .ToList()
                    .ForEach(x => AddLookups(x, lookups, cache));
                Enum.GetValues(typeof(LookupCodedKey))
                   .OfType<LookupCodedKey>()
                   .ToList()
                   .ForEach(x => AddLookups(x, lookups, cache));
                Enum.GetValues(typeof(LookupCodedKeyDictionary))
                    .OfType<LookupCodedKeyDictionary>()
                    .ToList()
                    .ForEach(x => AddLookups(x, lookups, cache));
                Enum.GetValues(typeof(LookupTimeRestrictedKey))
                    .OfType<LookupTimeRestrictedKey>()
                    .ToList()
                    .ForEach(x => AddLookups(x, lookups, cache));
                Enum.GetValues(typeof(LookupComplexKey))
                    .OfType<LookupComplexKey>()
                    .ToList()
                    .ForEach(x => AddLookups(x, lookups, cache));
                Enum.GetValues(typeof(LookupItemKey))
                    .OfType<LookupItemKey>()
                    .ToList()
                    .ForEach(x => AddLookups(x, lookups, cache));
            }
        }

        /// <summary>
        /// Adds lookups.
        /// </summary>
        /// <param name="forThisKey">For this key.</param>
        /// <param name="usingSource">using source.</param>
        /// <param name="addToCache">add to cache.</param>
        public void AddLookups(LookupSimpleKey forThisKey, XElement usingSource, InternalDataCache addToCache)
        {
            var lookups = BuildSimpleLookupEnumerable<int>(usingSource, forThisKey.ToString());

            addToCache.SimpleLookups.Add(forThisKey, lookups.ToList());
        }

        /// <summary>
        /// Adds lookups.
        /// </summary>
        /// <param name="forThisKey">For this key.</param>
        /// <param name="usingSource">using source.</param>
        /// <param name="addToCache">add to cache.</param>
        public void AddLookups(LookupCodedKey forThisKey, XElement usingSource, InternalDataCache addToCache)
        {
            var lookups = BuildSimpleLookupEnumerable<string>(usingSource, forThisKey.ToString());

            addToCache.CodedLookups.Add(forThisKey, lookups.ToList());
        }

        /// <summary>
        /// Adds lookups.
        /// </summary>
        /// <param name="forThisKey">For this key.</param>
        /// <param name="usingSource">using source.</param>
        /// <param name="addToCache">add to cache.</param>
        public void AddLookups(LookupCodedKeyDictionary forThisKey, XElement usingSource, InternalDataCache addToCache)
        {
            var lookups = BuildComplexLookupEnumerable(usingSource, forThisKey.ToString());

            addToCache.CodedDictionaryLookups.Add(forThisKey, lookups);
        }

        public void AddLookups(LookupComplexKey forThisKey, XElement usingSource, InternalDataCache addToCache)
        {
            var lookups = BuildComplexLookupWithValidityPeriods(usingSource, forThisKey.ToString());

            addToCache.CodedComplexLookups.Add(forThisKey, lookups);
        }

        public void AddLookups(LookupItemKey forThisKey, XElement usingSource, InternalDataCache addToCache)
        {
            var lookups = BuildItemLookupEnumerable(usingSource, forThisKey.ToString());

            addToCache.ItemLookups.Add(forThisKey, lookups);
        }

        /// <summary>
        /// Adds lookups.
        /// </summary>
        /// <param name="forThisKey">For this key.</param>
        /// <param name="usingSource">using source.</param>
        /// <param name="addToCache">add to cache.</param>
        public void AddLookups(LookupTimeRestrictedKey forThisKey, XElement usingSource, InternalDataCache addToCache)
        {
            var lookups = BuildLookupWithValidityPeriods(usingSource, forThisKey.ToString());

            addToCache.LimitedLifeLookups.Add(forThisKey, lookups);
        }

        /// <summary>
        /// Builds the academic year.
        /// this doesn't belong in here...
        /// there is a service for this
        /// </summary>
        /// <returns>the academic year</returns>
        private AcademicYear BuildAcademicYear()
        {
            return new AcademicYear()
            {
                AugustThirtyFirst = new DateTime(2018, 8, 31),
                End = new DateTime(2019, 7, 31),
                JanuaryFirst = new DateTime(2019, 1, 1),
                JulyThirtyFirst = new DateTime(2019, 7, 31),
                Start = new DateTime(2018, 8, 1),
            };
        }

        /// <summary>
        /// Builds the simple lookup enumerable.
        /// </summary>
        /// <typeparam name="T">the domain type for the lookup list</typeparam>
        /// <param name="lookups">The lookups.</param>
        /// <param name="type">The type.</param>
        /// <returns>a list of simple lookups</returns>
        private IEnumerable<T> BuildSimpleLookupEnumerable<T>(XElement lookups, string type)
        {
            return lookups
                .Descendants(type)
                .Descendants("option")
                .Attributes("code")
                .Select(c => (T)Convert.ChangeType(c.Value, typeof(T)));
        }

        private IDictionary<string, IReadOnlyCollection<string>> BuildItemLookupEnumerable(XElement lookups, string type)
        {
            return lookups
                .Descendants(type)
                .Descendants("option")
                .ToCaseInsensitiveDictionary(
                    n => GetAttributeValue(n.Attribute("code")),
                    v => v.Descendants("item")
                        .Select(i => GetAttributeValue(i.Attribute("value")))
                        .AsSafeReadOnlyList());
        }

        /// <summary>
        /// Builds the complex lookup enumerable.
        /// </summary>
        /// <param name="lookups">The lookups.</param>
        /// <param name="type">The type.</param>
        /// <returns>a list of simple lookups</returns>
        private IDictionary<string, IReadOnlyCollection<string>> BuildComplexLookupEnumerable(XElement lookups, string type)
        {
            return lookups
                .Descendants(type)
                .Elements()
                .ToCaseInsensitiveDictionary(
                    n => $"{n.Name}",
                    v => v.Descendants("option")
                        .Select(d => GetAttributeValue(d.Attribute("code")))
                        .AsSafeReadOnlyList());
        }

        private IDictionary<string, IDictionary<string, ValidityPeriods>> BuildComplexLookupWithValidityPeriods(
            XElement lookups, string type)
        {
            return lookups
                .Descendants(type)
                .Elements()
                .ToCaseInsensitiveDictionary(
                    n => $"{n.Name}",
                    v => v.Descendants("option")
                            .ToCaseInsensitiveDictionary(
                                c => c.Attribute("code")?.Value,
                                vp => new ValidityPeriods(
                                    DateTime.Parse(GetAttributeValue(vp.Attribute("validFrom")) ?? $"{DateTime.MinValue}"),
                                    DateTime.Parse(GetAttributeValue(vp.Attribute("validTo")) ?? $"{DateTime.MaxValue}")))
                        as IDictionary<string, ValidityPeriods>);
        }

        /// <summary>
        /// Builds the lookup with validity periods.
        /// </summary>
        /// <param name="lookups">The lookups.</param>
        /// <param name="type">The type.</param>
        /// <returns>string domain lists of validity periods</returns>
        private IDictionary<string, ValidityPeriods> BuildLookupWithValidityPeriods(XElement lookups, string type)
        {
            return lookups
                 .Descendants(type)
                 .Descendants("option")
                 .ToCaseInsensitiveDictionary(c => c.Attribute("code").Value, v => new ValidityPeriods(
                     validFrom: DateTime.Parse(GetAttributeValue(v.Attribute("validFrom")) ?? $"{DateTime.MinValue}"),
                     validTo: DateTime.Parse(GetAttributeValue(v.Attribute("validTo")) ?? $"{DateTime.MaxValue}")));
        }

        private string GetAttributeValue(XAttribute thisAttribute) =>
            thisAttribute?.Value;

        /// <summary>
        /// Builds the lookup with validity periods (using int keys)
        /// </summary>
        /// <param name="lookups">The lookups.</param>
        /// <param name="type">The type.</param>
        /// <returns>integer domain lists of validity periods</returns>
        private IDictionary<int, ValidityPeriods> BuildLookupAsIntWithValidityPeriods(XElement lookups, string type)
        {
            return lookups
                 .Descendants(type)
                 .Descendants("option")
                 .ToDictionary(c => int.Parse(c.Attribute("code").Value), v => new ValidityPeriods(
                     validFrom: DateTime.Parse(GetAttributeValue(v.Attribute("validFrom")) ?? $"{DateTime.MinValue}"),
                     validTo: DateTime.Parse(GetAttributeValue(v.Attribute("validTo")) ?? $"{DateTime.MaxValue}")));
        }
    }
}
