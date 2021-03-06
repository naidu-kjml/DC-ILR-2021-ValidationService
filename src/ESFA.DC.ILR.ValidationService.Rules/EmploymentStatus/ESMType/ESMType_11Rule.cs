﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.EmploymentStatus.ESMType
{
    public class ESMType_11Rule :
        IRule<ILearner>
    {
        public const string Name = RuleNameConstants.ESMType_11;

        private readonly IValidationErrorHandler _messageHandler;

        private readonly IProvideLookupDetails _lookups;

        public ESMType_11Rule(
            IValidationErrorHandler validationErrorHandler,
            IProvideLookupDetails lookups)
        {
            _messageHandler = validationErrorHandler;
            _lookups = lookups;
        }

        public string RuleName => Name;

        public bool InQualifyingPeriod(IEmploymentStatusMonitoring monitor, DateTime candidate) =>
            _lookups.IsCurrent(TypeOfLimitedLifeLookup.ESMType, $"{monitor.ESMType}{monitor.ESMCode}", candidate);

        public bool IsNotValid(ILearnerEmploymentStatus employmentStatus) =>
            employmentStatus.EmploymentStatusMonitorings.NullSafeAny(x => !InQualifyingPeriod(x, employmentStatus.DateEmpStatApp));

        public void Validate(ILearner objectToValidate)
        {
            var learnRefNumber = objectToValidate.LearnRefNumber;

            objectToValidate.LearnerEmploymentStatuses
                .NullSafeWhere(IsNotValid)
                .ForEach(x => RaiseValidationMessage(learnRefNumber, x));
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearnerEmploymentStatus thisEmployment)
        {
            var parameters = new List<IErrorMessageParameter>
            {
                _messageHandler.BuildErrorMessageParameter(nameof(thisEmployment.DateEmpStatApp), thisEmployment.DateEmpStatApp)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, null, parameters);
        }
    }
}