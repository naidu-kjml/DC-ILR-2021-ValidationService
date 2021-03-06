﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.Internal.AcademicYear.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Derived.Interface;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.LearnStartDate
{
    public class LearnStartDate_12Rule :
        IRule<ILearner>
    {
        public const string MessagePropertyName = PropertyNameConstants.LearnStartDate;

        public const string Name = RuleNameConstants.LearnStartDate_12;

        private readonly IValidationErrorHandler _messageHandler;

        private readonly IAcademicYearDataService _yearData;

        private readonly IDerivedData_07Rule _derivedData07;

        public LearnStartDate_12Rule(
            IValidationErrorHandler validationErrorHandler,
            IAcademicYearDataService yearData,
            IDerivedData_07Rule derivedData07)
        {
            _messageHandler = validationErrorHandler;
            _yearData = yearData;
            _derivedData07 = derivedData07;
        }

        public string RuleName => Name;

        public bool IsApprenticeship(ILearningDelivery delivery) =>
            _derivedData07.IsApprenticeship(delivery.ProgTypeNullable);

        public bool HasQualifyingStartDate(ILearningDelivery delivery) =>
            delivery.LearnStartDate < _yearData.End().AddYears(1);

        public bool IsNotValid(ILearningDelivery delivery) =>
            IsApprenticeship(delivery) && !HasQualifyingStartDate(delivery);

        public void Validate(ILearner objectToValidate)
        {
            var learnRefNumber = objectToValidate.LearnRefNumber;

            objectToValidate.LearningDeliveries
                .NullSafeWhere(IsNotValid)
                .ForEach(x => RaiseValidationMessage(learnRefNumber, x));
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearningDelivery thisDelivery)
        {
            var parameters = new List<IErrorMessageParameter>
            {
                _messageHandler.BuildErrorMessageParameter(MessagePropertyName, thisDelivery.LearnStartDate)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, thisDelivery.AimSeqNumber, parameters);
        }
    }
}
