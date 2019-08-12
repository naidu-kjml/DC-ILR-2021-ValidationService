﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.CrossEntity
{
    public class R104Rule : AbstractRule, IRule<ILearner>
    {
        private readonly string _famTypeACT = Monitoring.Delivery.Types.ApprenticeshipContract;

        public R104Rule( IValidationErrorHandler validationErrorHandler)
            : base(validationErrorHandler, RuleNameConstants.R104)
        {
        }

        public void Validate(ILearner objectToValidate)
        {
            if (objectToValidate.LearningDeliveries == null)
            {
                return;
            }

            foreach (var learningDelivery in objectToValidate.LearningDeliveries)
            {
                if (learningDelivery.LearningDeliveryFAMs == null || !HasValidFamsToCheck(learningDelivery.LearningDeliveryFAMs))
                {
                    continue;
                }

                var nonConsecutiveLearningDeliveryFAMs = GetNonConsecutiveLearningDeliveryFAMsForType(learningDelivery.LearningDeliveryFAMs);

                foreach (var learningDeliveryFAM in nonConsecutiveLearningDeliveryFAMs)
                {
                    HandleValidationError(
                        objectToValidate.LearnRefNumber,
                        learningDelivery.AimSeqNumber,
                        BuildErrorMessageParameters(
                            learningDelivery.LearnPlanEndDate,
                            learningDelivery.LearnActEndDateNullable,
                            _famTypeACT,
                            learningDeliveryFAM.LearnDelFAMDateFromNullable,
                            learningDeliveryFAM.LearnDelFAMDateToNullable));
                }

            }
        }

        public bool HasValidFamsToCheck(IEnumerable<ILearningDeliveryFAM> learningDeliveryFams)
        {
            var learnDelFAMsToCheck =
                learningDeliveryFams?
                    .Where(fam => fam.LearnDelFAMType.CaseInsensitiveEquals(_famTypeACT))
                    .OrderBy(ld => ld.LearnDelFAMDateFromNullable ?? DateTime.MaxValue);

            return learnDelFAMsToCheck.Any(ldf => ldf.LearnDelFAMDateFromNullable != null) &&
                   learnDelFAMsToCheck.Count() >= 2;
        }

        public IEnumerable<ILearningDeliveryFAM> GetNonConsecutiveLearningDeliveryFAMsForType(IEnumerable<ILearningDeliveryFAM> learningDeliveryFams)
        {
            var nonConsecutiveLearningDeliveryFAMs = new List<ILearningDeliveryFAM>();

            var learnDelFAMsArray = learningDeliveryFams.ToArray();
            var arraySize = learnDelFAMsArray.Length;

            for (var i = 0; i < arraySize - 1; i++)
            {
                var learnDelFAMSource = learnDelFAMsArray[i];
                var learnDelFAMToCompare = learnDelFAMsArray[i + 1];

                if (learnDelFAMSource.LearnDelFAMDateToNullable != learnDelFAMToCompare.LearnDelFAMDateFromNullable.Value.AddDays(-1))
                {
                    nonConsecutiveLearningDeliveryFAMs.Add(learnDelFAMToCompare);
                }
            }

            return nonConsecutiveLearningDeliveryFAMs;
        }

        public IEnumerable<IErrorMessageParameter> BuildErrorMessageParameters(DateTime learnPlanEndDate, DateTime? learnActEndDate, string famType, DateTime? learnDelFamDateFrom, DateTime? learnDelFamDateTo)
        {
            return new[]
            {
                BuildErrorMessageParameter(PropertyNameConstants.LearnPlanEndDate, learnPlanEndDate),
                BuildErrorMessageParameter(PropertyNameConstants.LearnActEndDate, learnActEndDate),
                BuildErrorMessageParameter(PropertyNameConstants.LearnDelFAMType, famType),
                BuildErrorMessageParameter(PropertyNameConstants.LearnDelFAMDateFrom, learnDelFamDateFrom),
                BuildErrorMessageParameter(PropertyNameConstants.LearnDelFAMDateTo, learnDelFamDateTo)
            };
        }
    }
}
