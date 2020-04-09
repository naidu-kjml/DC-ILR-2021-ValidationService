﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESFA.DC.ILR.ValidationService.Rules.EmploymentStatus.EmpStat
{
    public class EmpStat_11Rule :
        IRule<ILearner>
    {
        public const int PastoralHoursThreshold = 540;

        public const string MessagePropertyName = PropertyNameConstants.EmpStat;

        public const string Name = RuleNameConstants.EmpStat_11;

        private readonly IValidationErrorHandler _messageHandler;

        public EmpStat_11Rule(IValidationErrorHandler validationErrorHandler)
        {
            It.IsNull(validationErrorHandler)
                .AsGuard<ArgumentNullException>(nameof(validationErrorHandler));

            _messageHandler = validationErrorHandler;
        }

        public string RuleName => Name;

        public DateTime LastInviableDate => new DateTime(2014, 07, 31);

        public bool InTraining(ILearningDelivery delivery) =>
            It.IsInRange(delivery.ProgTypeNullable, TypeOfLearningProgramme.Traineeship);

        public bool IsViableStart(ILearningDelivery delivery) =>
            delivery.LearnStartDate > LastInviableDate;

        public bool HasQualifyingFunding(ILearningDelivery delivery) =>
            It.IsInRange(delivery.FundModel, TypeOfFunding.Age16To19ExcludingApprenticeships, TypeOfFunding.Other16To19);

        public bool HasAQualifyingEmploymentStatus(ILearningDelivery delivery, IReadOnlyCollection<ILearnerEmploymentStatus> usingEmployments) =>
            usingEmployments
                .SafeAny(x => x.DateEmpStatApp < delivery.LearnStartDate);

        public bool IsNotValid(ILearningDelivery delivery, IReadOnlyCollection<ILearnerEmploymentStatus> usingEmployments) =>
            !InTraining(delivery)
                && IsViableStart(delivery)
                && HasQualifyingFunding(delivery)
                && !HasAQualifyingEmploymentStatus(delivery, usingEmployments);

        public int GetQualifyingHours(ILearner learner) =>
            (learner.PlanEEPHoursNullable ?? 0) + (learner.PlanLearnHoursNullable ?? 0);

        public bool HasQualifyingHours(int candidate) =>
            candidate < PastoralHoursThreshold;

        public void Validate(ILearner objectToValidate)
        {
            It.IsNull(objectToValidate)
                .AsGuard<ArgumentNullException>(nameof(objectToValidate));

            if (HasQualifyingHours(GetQualifyingHours(objectToValidate)))
            {
                var learnRefNumber = objectToValidate.LearnRefNumber;
                var employments = objectToValidate.LearnerEmploymentStatuses.ToReadOnlyCollection();

                objectToValidate.LearningDeliveries
                    .Where(x => IsNotValid(x, employments))
                    .ForEach(x => RaiseValidationMessage(learnRefNumber, x));
            }
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearningDelivery thisDelivery)
        {
            var parameters = new List<IErrorMessageParameter>
            {
                _messageHandler.BuildErrorMessageParameter(MessagePropertyName, TypeOfEmploymentStatus.NotKnownProvided),
                _messageHandler.BuildErrorMessageParameter(PropertyNameConstants.LearnStartDate, thisDelivery.LearnStartDate)
            };

            _messageHandler.Handle(RuleName, learnRefNumber, thisDelivery.AimSeqNumber, parameters);
        }
    }
}
