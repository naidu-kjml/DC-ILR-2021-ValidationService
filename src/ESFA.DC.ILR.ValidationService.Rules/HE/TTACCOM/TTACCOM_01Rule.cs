﻿using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.HE.TTACCOM
{
    public class TTACCOM_01Rule :
        IRule<ILearner>
    {
        public const string MessagePropertyName = "TTACCOM";

        public const string Name = RuleNameConstants.TTACCOM_01;

        private readonly IValidationErrorHandler _messageHandler;

        private readonly IProvideLookupDetails _lookupDetails;

        public TTACCOM_01Rule(IValidationErrorHandler validationErrorHandler, IProvideLookupDetails lookupDetails)
        {
            _messageHandler = validationErrorHandler;
            _lookupDetails = lookupDetails;
        }

        public string RuleName => Name;

        public void Validate(ILearner objectToValidate)
        {
            var learnRefNumber = objectToValidate.LearnRefNumber;
            var learnerHE = objectToValidate.LearnerHEEntity;
            var tTAccom = learnerHE?.TTACCOMNullable;

            var failedValidation = !ConditionMet(tTAccom);

            if (failedValidation)
            {
                RaiseValidationMessage(learnRefNumber, tTAccom.Value);
            }
        }

        public bool ConditionMet(int? tTAccom)
        {
            return tTAccom != null
                ? _lookupDetails.Contains(TypeOfLimitedLifeLookup.TTAccom, tTAccom.Value)
                : true;
        }

        public void RaiseValidationMessage(string learnRefNumber, int tTAccom)
        {
            var parameters = new List<IErrorMessageParameter>
            {
                _messageHandler.BuildErrorMessageParameter(MessagePropertyName, tTAccom)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, null, parameters);
        }
    }
}
