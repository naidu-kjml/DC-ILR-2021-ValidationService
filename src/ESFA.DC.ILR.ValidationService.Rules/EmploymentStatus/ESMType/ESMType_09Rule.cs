﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;

namespace ESFA.DC.ILR.ValidationService.Rules.EmploymentStatus.ESMType
{
    public class ESMType_09Rule : IRule<ILearner>
    {
        public const string MessagePropertyName = "ESMType";
        public const string Name = RuleNameConstants.ESMType_09;
        private readonly IValidationErrorHandler _messageHandler;
        private readonly IProvideRuleCommonOperations _check;
        private readonly IDateTimeQueryService _dateTimeQueryService;

        public ESMType_09Rule(
            IValidationErrorHandler validationErrorHandler,
            IProvideRuleCommonOperations check,
            IDateTimeQueryService dateTimeQueryService)
        {
            _messageHandler = validationErrorHandler;
            _check = check;
            _dateTimeQueryService = dateTimeQueryService;
        }

        public static DateTime FirstViableDate => new DateTime(2013, 08, 01);

        public string RuleName => Name;

        public DateTime? GetLastestQualifyingDate(IReadOnlyCollection<ILearningDelivery> deliveries) =>
            deliveries
                .NullSafeWhere(IsACandidate)
                .OrderByDescending(x => x.LearnStartDate)
                .FirstOrDefault()?
                .LearnStartDate;

        public bool IsACandidate(ILearningDelivery delivery) =>
            _check.InApprenticeship(delivery)
                && delivery.AimType == TypeOfAim.ProgrammeAim
                && _check.HasQualifyingStart(delivery, FirstViableDate);

        public bool IsQualifyingEmployment(ILearnerEmploymentStatus employmentStatus) =>
            employmentStatus.EmpStat == TypeOfEmploymentStatus.InPaidEmployment;

        public bool HasQualifyingIndicator(IEmploymentStatusMonitoring monitor) =>
            monitor.ESMType.CaseInsensitiveEquals(Monitoring.EmploymentStatus.Types.LengthOfEmployment);

        public bool HasQualifyingIndicator(ILearnerEmploymentStatus employmentStatus) =>
            employmentStatus.EmploymentStatusMonitorings.NullSafeAny(HasQualifyingIndicator);

        public bool IsNotValid(ILearnerEmploymentStatus employmentStatus, DateTime? lastViabledate) =>
            lastViabledate.HasValue
            && employmentStatus != null
            && _dateTimeQueryService.IsDateBetween(employmentStatus.DateEmpStatApp, FirstViableDate, lastViabledate ?? DateTime.MaxValue)
            && IsQualifyingEmployment(employmentStatus)
            && !HasQualifyingIndicator(employmentStatus);

        public void Validate(ILearner objectToValidate)
        {
            var learnRefNumber = objectToValidate.LearnRefNumber;
            var qualifyingDate = GetLastestQualifyingDate(objectToValidate.LearningDeliveries);

            objectToValidate.LearnerEmploymentStatuses
                .NullSafeWhere(x => IsNotValid(x, qualifyingDate))
                .ForEach(x => RaiseValidationMessage(learnRefNumber, x));
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearnerEmploymentStatus thisEmployment)
        {
            var parameters = new List<IErrorMessageParameter>
            {
                _messageHandler.BuildErrorMessageParameter(MessagePropertyName, Monitoring.EmploymentStatus.Types.LengthOfEmployment),
                _messageHandler.BuildErrorMessageParameter(nameof(thisEmployment.EmpStat), thisEmployment.EmpStat),
                _messageHandler.BuildErrorMessageParameter(nameof(thisEmployment.DateEmpStatApp), thisEmployment.DateEmpStatApp)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, null, parameters);
        }
    }
}