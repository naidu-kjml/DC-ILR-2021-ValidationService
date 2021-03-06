﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.HE.DOMICILE
{
    public class DOMICILE_02Rule :
        IRule<ILearner>
    {
        public const string MessagePropertyName = "DOMICILE";

        public const string Name = RuleNameConstants.Domicile_02;

        private readonly IValidationErrorHandler _messageHandler;

        private readonly IProvideLookupDetails _lookups;

        public DOMICILE_02Rule(
            IValidationErrorHandler validationErrorHandler,
            IProvideLookupDetails lookups)
        {
            _messageHandler = validationErrorHandler;
            _lookups = lookups;
        }

        public string RuleName => Name;

        public bool HasHigherEd(ILearningDelivery delivery) =>
            delivery.LearningDeliveryHEEntity != null;

        public bool HasValidDomicile(ILearningDeliveryHE he) =>
            he.DOMICILE == null || _lookups.Contains(TypeOfStringCodedLookup.Domicile, he.DOMICILE);

        public void Validate(ILearner objectToValidate)
        {
            var learnRefNumber = objectToValidate.LearnRefNumber;

            objectToValidate.LearningDeliveries
                .NullSafeWhere(x => HasHigherEd(x) && !HasValidDomicile(x.LearningDeliveryHEEntity))
                .ForEach(x =>
                {
                    RaiseValidationMessage(learnRefNumber, x);
                });
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearningDelivery thisDelivery)
        {
            var parameters = new List<IErrorMessageParameter>
            {
                _messageHandler.BuildErrorMessageParameter(PropertyNameConstants.DOMICILE, thisDelivery.LearningDeliveryHEEntity.DOMICILE)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, thisDelivery.AimSeqNumber, parameters);
        }
    }
}