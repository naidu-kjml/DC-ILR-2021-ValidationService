﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.External.FCS.Interface;
using ESFA.DC.ILR.ValidationService.Data.File.FileData.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.UKPRN
{
    public class UKPRN_18Rule : AbstractRule, IRule<ILearner>
    {
        private readonly ILearningDeliveryFAMQueryService _learningDeliveryFAMQueryService;
        private readonly IFCSDataService _fcsData;

        private readonly HashSet<string> _fundingStreams = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            FundingStreamPeriodCodeConstants.AEBC_19TRN2021,
            FundingStreamPeriodCodeConstants.AEBC_ASCL2021
        };

        public UKPRN_18Rule(
            IValidationErrorHandler validationErrorHandler,
            IFileDataService fileDataService,
            ILearningDeliveryFAMQueryService learningDeliveryFAMQueryService,
            IFCSDataService fcsDataService)
            : base(validationErrorHandler, RuleNameConstants.UKPRN_18)
        {
            ProviderUKPRN = fileDataService.UKPRN();
            _learningDeliveryFAMQueryService = learningDeliveryFAMQueryService;
            _fcsData = fcsDataService;
        }

        public int ProviderUKPRN { get; }

        public void Validate(ILearner theLearner)
        {
            var learnRefNumber = theLearner.LearnRefNumber;

            theLearner.LearningDeliveries
                .ForAny(IsNotValid, x => RaiseValidationMessage(learnRefNumber, x));
        }

        public bool IsNotValid(ILearningDelivery theDelivery) =>
            !IsExcluded(theDelivery)
            && HasQualifyingModel(theDelivery)
            && IsESFAAdultFunding(theDelivery)
            && HasDisQualifyingFundingRelationship(x => HasStartedAfterStopDate(x, theDelivery));

        public bool IsExcluded(ILearningDelivery theDelivery)
        {
            return _learningDeliveryFAMQueryService.HasLearningDeliveryFAMCodeForType(
                       theDelivery.LearningDeliveryFAMs,
                       LearningDeliveryFAMTypeConstants.LDM,
                       LearningDeliveryFAMCodeConstants.LDM_ProcuredAdultEducationBudget)
                   || _learningDeliveryFAMQueryService.HasLearningDeliveryFAMType(
                       theDelivery.LearningDeliveryFAMs,
                       LearningDeliveryFAMTypeConstants.RES);
        }

        public bool HasQualifyingModel(ILearningDelivery theDelivery) =>
            theDelivery.FundModel == FundModels.AdultSkills;

        public bool IsESFAAdultFunding(ILearningDelivery theDelivery) =>
            _learningDeliveryFAMQueryService.HasLearningDeliveryFAMCodeForType(
                theDelivery.LearningDeliveryFAMs,
                LearningDeliveryFAMTypeConstants.SOF,
                LearningDeliveryFAMCodeConstants.SOF_ESFA_Adult);

        public bool HasDisQualifyingFundingRelationship(Func<IFcsContractAllocation, bool> hasStartedAfterStopDate)
        {
            var fcsRecord = _fcsData
                .GetContractAllocationsFor(ProviderUKPRN)
                .OrderBy(x => x.StartDate)
                .FirstOrDefault();

            if (fcsRecord == null)
            {
                return false;
            }

            return HasFundingRelationship(fcsRecord) && hasStartedAfterStopDate(fcsRecord);
        }

        public bool HasFundingRelationship(IFcsContractAllocation theAllocation) =>
            _fundingStreams.Contains(theAllocation.FundingStreamPeriodCode);

        public bool HasStartedAfterStopDate(IFcsContractAllocation theAllocation, ILearningDelivery theDelivery) =>
            theDelivery.LearnStartDate >= theAllocation.StopNewStartsFromDate;

        public void RaiseValidationMessage(string learnRefNumber, ILearningDelivery theDelivery) =>
            HandleValidationError(learnRefNumber, theDelivery.AimSeqNumber, BuildMessageParametersFor(theDelivery));

        public IEnumerable<IErrorMessageParameter> BuildMessageParametersFor(ILearningDelivery theDelivery) => new[]
        {
            BuildErrorMessageParameter(PropertyNameConstants.UKPRN, ProviderUKPRN),
            BuildErrorMessageParameter(PropertyNameConstants.FundModel, theDelivery.FundModel),
            BuildErrorMessageParameter(PropertyNameConstants.ProgType, theDelivery.ProgTypeNullable),
            BuildErrorMessageParameter(PropertyNameConstants.LearnStartDate, theDelivery.LearnStartDate),
        };
    }
}
