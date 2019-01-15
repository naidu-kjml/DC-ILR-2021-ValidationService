﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.External.LARS.Interface;
using ESFA.DC.ILR.ValidationService.Data.External.Organisation.Interface;
using ESFA.DC.ILR.ValidationService.Data.File.FileData.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Derived.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.LearnDelFAMType
{
    public class LearnDelFAMType_56Rule : AbstractRule, IRule<ILearner>
    {
        private const double DaysInYear = 365.242199;

        private const int MinAge = 19;
        private const int MaxAge = 23;
        private const int TradeUnionAimsCategoryRef = 19;
        private readonly DateTime MinimumStartDate = new DateTime(2016, 08, 01);

        private readonly HashSet<int> PriorAttainList1 = new HashSet<int>() { 2, 3, 4, 5, 10, 11, 12, 13, 97, 98 };
        private readonly HashSet<int> PriorAttainList2 = new HashSet<int>() { 3, 4, 5, 10, 11, 12, 13, 97, 98 };

        private readonly List<string> FamCodesForExclusion = new List<string>()
        {
            LearningDeliveryFAMCodeConstants.LDM_OLASS,
            LearningDeliveryFAMCodeConstants.LDM_RoTL,
            LearningDeliveryFAMCodeConstants.LDM_SteelRedundancy
        };

        private readonly HashSet<string> NvqLevelsList1 = new HashSet<string>(new List<string>() { "E", "1", "2" }).ToCaseInsensitiveHashSet();
        private readonly HashSet<string> NvqLevelsList2 = new HashSet<string>(new List<string>() { "3", "4" }).ToCaseInsensitiveHashSet();
        private readonly HashSet<int> BasicSkillTypes = new HashSet<int>() { 01, 11, 13, 20, 23, 24, 29, 31, 02, 12, 14, 19, 21, 25, 30, 32, 33, 34, 35 };

        private readonly ILARSDataService _larsDataService;
        private readonly IDD07 _dd07;
        private readonly IDerivedData_21Rule _derivedDataRule21;
        private readonly ILearningDeliveryFAMQueryService _famQueryService;
        private readonly IFileDataService _fileDataService;
        private readonly IOrganisationDataService _organisationDataService;

        public LearnDelFAMType_56Rule(
            IValidationErrorHandler validationErrorHandler,
            ILARSDataService larsDataService,
            IDD07 dd07,
            IDerivedData_21Rule derivedDataRule21,
            ILearningDeliveryFAMQueryService famQueryService,
            IFileDataService fileDataService,
            IOrganisationDataService organisationDataService)
            : base(validationErrorHandler, RuleNameConstants.LearnDelFAMType_56)
        {
            _larsDataService = larsDataService;
            _dd07 = dd07;
            _derivedDataRule21 = derivedDataRule21;
            _famQueryService = famQueryService;
            _fileDataService = fileDataService;
            _organisationDataService = organisationDataService;
        }

        public void Validate(ILearner learner)
        {
            if (learner?.LearningDeliveries == null)
            {
                return;
            }

            if (IsProviderExcluded())
            {
                return;
            }

            foreach (var learningDelivery in learner.LearningDeliveries)
            {
               if (ConditionMet(learningDelivery, learner.DateOfBirthNullable, learner.PriorAttainNullable) &&
                   !IsLearningDeliveryExcluded(learner, learningDelivery))
                {
                    HandleValidationError(
                        learningDelivery.LearnAimRef,
                        learningDelivery.AimSeqNumber,
                        BuildErrorMessageParameters(_fileDataService.UKPRN(), learner, learningDelivery));
                }
            }
        }

        public bool ConditionMet(ILearningDelivery learningDelivery, DateTime? dateofBirth, int? priorAttain)
        {
            return StartDateConditionMet(learningDelivery.LearnStartDate) &&
                   FundModelConditionMet(learningDelivery.FundModel) &&
                   AgeConditionMet(learningDelivery.LearnStartDate, dateofBirth) &&
                   FamConditionMet(learningDelivery.LearningDeliveryFAMs) &&
                   NvQLevelConditionMet(learningDelivery.LearnAimRef, priorAttain);
        }

        public bool StartDateConditionMet(DateTime learnStartDate)
        {
            return learnStartDate < MinimumStartDate;
        }

        public bool FundModelConditionMet(int fundModel)
        {
            return fundModel == TypeOfFunding.AdultSkills;
        }

        public bool AgeConditionMet(DateTime learnStartDate, DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
            {
                return false;
            }

            var ageAtCourseStart = Convert.ToInt32((learnStartDate - dateOfBirth.Value).TotalDays / DaysInYear);
            if (ageAtCourseStart >= MinAge || ageAtCourseStart <= MaxAge)
            {
                return true;
            }

            return false;
        }

        public bool FamConditionMet(IReadOnlyCollection<ILearningDeliveryFAM> fams)
        {
            return _famQueryService.HasLearningDeliveryFAMCodeForType(fams, LearningDeliveryFAMTypeConstants.FFI, LearningDeliveryFAMCodeConstants.FFI_Fully);
        }

        public bool NvQLevelConditionMet(string learnAimRef, int? priorAttain)
        {
            if (!priorAttain.HasValue)
            {
                return false;
            }

            if (!PriorAttainList1.Contains(priorAttain.Value) &&
                !PriorAttainList2.Contains(priorAttain.Value))
            {
                return false;
            }

            var nvqLevel = _larsDataService.GetNotionalNVQLevelv2ForLearnAimRef(learnAimRef);

            if ((PriorAttainList1.Contains(priorAttain.Value) && NvqLevelsList1.Contains(nvqLevel)) ||
                (PriorAttainList2.Contains(priorAttain.Value) && NvqLevelsList2.Contains(nvqLevel)))
            {
                return true;
            }

            return false;
        }

        public bool IsProviderExcluded()
        {
            return _organisationDataService.LegalOrgTypeMatchForUkprn(_fileDataService.UKPRN(), LegalOrgTypeConstants.USDC);
        }

        private bool IsLearningDeliveryExcluded(ILearner learner, ILearningDelivery learningDelivery)
        {
            if (_dd07.IsApprenticeship(learningDelivery.ProgTypeNullable))
            {
                return true;
            }

            if (learningDelivery.ProgTypeNullable.HasValue &&
                learningDelivery.ProgTypeNullable.Value == TypeOfLearningProgramme.Traineeship)
            {
                return true;
            }

            if (_famQueryService.HasAnyLearningDeliveryFAMCodesForType(
                learningDelivery.LearningDeliveryFAMs,
                LearningDeliveryFAMTypeConstants.LDM,
                FamCodesForExclusion))
            {
                return true;
            }

            //TODO: Add DD12

            if (_derivedDataRule21.IsAdultFundedUnemployedWithOtherStateBenefits(learner))
            {
                return true;
            }

            if (_famQueryService.HasLearningDeliveryFAMType(learningDelivery.LearningDeliveryFAMs, LearningDeliveryFAMTypeConstants.RES))
            {
                return true;
            }

            if (_larsDataService.BasicSkillsMatchForLearnAimRefAndStartDate(
                BasicSkillTypes,
                learningDelivery.LearnAimRef,
                learningDelivery.LearnStartDate))
            {
                return true;
            }

            if (_larsDataService.LearnAimRefExistsForLearningDeliveryCategoryRef(
                learningDelivery.LearnAimRef,
                TradeUnionAimsCategoryRef))
            {
                return true;
            }

            return false;
        }

        private IEnumerable<IErrorMessageParameter> BuildErrorMessageParameters(long ukprn, ILearner learner, ILearningDelivery learningDelivery)
        {
            return new List<IErrorMessageParameter>
            {
                BuildErrorMessageParameter(PropertyNameConstants.UKPRN, ukprn),
                BuildErrorMessageParameter(PropertyNameConstants.LearnAimRef, learningDelivery.LearnAimRef),
                BuildErrorMessageParameter(PropertyNameConstants.ProgType, learningDelivery.ProgTypeNullable),
                BuildErrorMessageParameter(PropertyNameConstants.FundModel, learningDelivery.FundModel),
                BuildErrorMessageParameter(PropertyNameConstants.LearnDelFAMType, LearningDeliveryFAMTypeConstants.FFI),
                BuildErrorMessageParameter(PropertyNameConstants.LearnDelFAMCode, LearningDeliveryFAMCodeConstants.FFI_Fully),
                BuildErrorMessageParameter(PropertyNameConstants.LearnStartDate, learningDelivery.LearnStartDate),
                BuildErrorMessageParameter(PropertyNameConstants.DateOfBirth, learner.DateOfBirthNullable),
                BuildErrorMessageParameter(PropertyNameConstants.PriorAttain, learner.PriorAttainNullable)
            };
        }
    }
}
