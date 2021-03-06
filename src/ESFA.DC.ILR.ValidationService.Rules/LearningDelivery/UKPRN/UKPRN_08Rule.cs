﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.External.FCS.Interface;
using ESFA.DC.ILR.ValidationService.Data.File.FileData.Interface;
using ESFA.DC.ILR.ValidationService.Data.Internal.AcademicYear.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.UKPRN
{
    public class UKPRN_08Rule : AbstractRule, IRule<ILearner>
    {
        private readonly IFileDataService _fileDataService;
        private readonly IAcademicYearDataService _academicYearDataService;
        private readonly ILearningDeliveryFAMQueryService _learningDeliveryFAMQueryService;
        private readonly IFCSDataService _fcsDataService;

        public UKPRN_08Rule(
            IValidationErrorHandler validationErrorHandler,
            IFileDataService fileDataService,
            IAcademicYearDataService academicYearDataService,
            ILearningDeliveryFAMQueryService learningDeliveryFAMQueryService,
            IFCSDataService fcsDataService)
            : base(validationErrorHandler, RuleNameConstants.UKPRN_08)
        {
            _fileDataService = fileDataService;
            _academicYearDataService = academicYearDataService;
            _learningDeliveryFAMQueryService = learningDeliveryFAMQueryService;
            _fcsDataService = fcsDataService;
        }

        public bool IsExcluded(ILearningDelivery thisDelivery) =>
            IsNotPartOfThisYear(thisDelivery);

        public DateTime GetCurrentAcademicYearCommencementDate() =>
            _academicYearDataService.Start();

        public bool IsNotPartOfThisYear(ILearningDelivery thisDelivery) =>
            thisDelivery.LearnActEndDateNullable.HasValue
                && thisDelivery.LearnActEndDateNullable < GetCurrentAcademicYearCommencementDate();

        public bool HasQualifyingProviderID(IFcsContractAllocation allocation, int providerID) =>
            allocation.DeliveryUKPRN == providerID;

        public bool HasQualifyingFundingStream(IFcsContractAllocation allocation) =>
            allocation.FundingStreamPeriodCode.CaseInsensitiveEquals(FundingStreamPeriodCodeConstants.ALLB2021)
            || allocation.FundingStreamPeriodCode.CaseInsensitiveEquals(FundingStreamPeriodCodeConstants.ALLBC2021);

        public bool HasFundingRelationship(ILearningDelivery thisDelivery)
        {
            var allocations = _fcsDataService.GetContractAllocationsFor(_fileDataService.UKPRN());

            return allocations.NullSafeAny(HasQualifyingFundingStream);
        }

        public bool IsNotValid(ILearningDelivery thisDelivery)
        {
            return !IsExcluded(thisDelivery)
                && _learningDeliveryFAMQueryService.HasLearningDeliveryFAMType(thisDelivery?.LearningDeliveryFAMs, LearningDeliveryFAMTypeConstants.ALB)
                && !HasFundingRelationship(thisDelivery);
        }

        public void Validate(ILearner thisLearner)
        {
            var learnRefNumber = thisLearner.LearnRefNumber;

            thisLearner.LearningDeliveries
                .ForAny(IsNotValid, x => RaiseValidationMessage(learnRefNumber, x));
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearningDelivery thisDelivery)
        {
            HandleValidationError(learnRefNumber, thisDelivery.AimSeqNumber, BuildMessageParameters());
        }

        public IEnumerable<IErrorMessageParameter> BuildMessageParameters()
        {
            return new[]
            {
                BuildErrorMessageParameter(PropertyNameConstants.UKPRN, _fileDataService.UKPRN()),
                BuildErrorMessageParameter(PropertyNameConstants.LearnDelFAMType, Monitoring.Delivery.Types.AdvancedLearnerLoansBursaryFunding)
            };
        }
    }
}
