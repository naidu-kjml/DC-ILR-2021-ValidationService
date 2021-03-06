﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.EmploymentStatus.ESMType
{
    public class ESMType_08Rule :
        IRule<ILearner>
    {
        public const string Name = RuleNameConstants.ESMType_08;

        private readonly IValidationErrorHandler _messageHandler;

        public ESMType_08Rule(
            IValidationErrorHandler validationErrorHandler)
        {
            _messageHandler = validationErrorHandler;
        }

        public string RuleName => Name;

        public DateTime LastInviableDate => new DateTime(2012, 07, 31);

        public bool IsQualifyingPeriod(ILearnerEmploymentStatus employmentStatus) =>
            employmentStatus.DateEmpStatApp > LastInviableDate;

        public bool IsQualifyingEmployment(ILearnerEmploymentStatus employmentStatus) =>
            employmentStatus.EmpStat == EmploymentStatusEmpStats.NotEmployedSeekingAndAvailable;

        public bool HasQualifyingIndicator(IEmploymentStatusMonitoring monitor) =>
            monitor.ESMType.CaseInsensitiveEquals(Monitoring.EmploymentStatus.Types.LengthOfUnemployment);

        public bool HasQualifyingIndicator(ILearnerEmploymentStatus employmentStatus) =>
            employmentStatus.EmploymentStatusMonitorings.NullSafeAny(HasQualifyingIndicator);

        public bool IsNotValid(ILearnerEmploymentStatus employmentStatus) =>
            IsQualifyingPeriod(employmentStatus) && IsQualifyingEmployment(employmentStatus) && !HasQualifyingIndicator(employmentStatus);

        public bool IsExcluded(ILearningDelivery delivery) =>
           delivery.FundModel == FundModels.Age16To19ExcludingApprenticeships
            || delivery.FundModel == FundModels.Other16To19;

        public bool IsExcluded(ILearner candidate) =>
            candidate.LearningDeliveries.NullSafeAny(IsExcluded);

        public void Validate(ILearner objectToValidate)
        {
            if (IsExcluded(objectToValidate))
            {
                return;
            }

            ValidateEmploymentRecords(objectToValidate);
        }

        public void ValidateEmploymentRecords(ILearner objectToValidate)
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
                _messageHandler.BuildErrorMessageParameter(nameof(thisEmployment.DateEmpStatApp), thisEmployment.DateEmpStatApp),
                _messageHandler.BuildErrorMessageParameter(nameof(thisEmployment.EmpStat), thisEmployment.EmpStat)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, null, parameters);
        }
    }
}