﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.External.LARS.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.StdCode
{
    public class StdCode_02Rule :
        IRule<ILearner>
    {
        public const string MessagePropertyName = PropertyNameConstants.StdCode;

        public const string Name = RuleNameConstants.StdCode_02;

        private readonly IValidationErrorHandler _messageHandler;

        private readonly ILARSDataService _larsData;

        public StdCode_02Rule(
            IValidationErrorHandler validationErrorHandler,
            ILARSDataService larsData)
        {
            _messageHandler = validationErrorHandler;
            _larsData = larsData;
        }

        public string RuleName => Name;

        public bool IsValidStandardCode(ILearningDelivery delivery) =>
            _larsData.ContainsStandardFor(delivery.StdCodeNullable.Value);

        public bool HasStandardCode(ILearningDelivery delivery) =>
            delivery.StdCodeNullable.HasValue;

        public bool IsNotValid(ILearningDelivery delivery) =>
            HasStandardCode(delivery) && !IsValidStandardCode(delivery);

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
                _messageHandler.BuildErrorMessageParameter(MessagePropertyName, thisDelivery.StdCodeNullable)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, thisDelivery.AimSeqNumber, parameters);
        }
    }
}