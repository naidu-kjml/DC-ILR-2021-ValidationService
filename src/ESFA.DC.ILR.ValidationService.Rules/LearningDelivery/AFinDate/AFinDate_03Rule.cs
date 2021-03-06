﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.File.FileData.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.AFinDate
{
    public class AFinDate_03Rule :
        AbstractRule,
        IRule<ILearner>
    {
        private readonly IFileDataService _fileData;

        public AFinDate_03Rule(IValidationErrorHandler validationErrorHandler, IFileDataService fileData)
            : base(validationErrorHandler, RuleNameConstants.AFinDate_03)
        {
            _fileData = fileData;
        }

        public void CheckDeliveryAFRs(ILearningDelivery delivery, Func<IAppFinRecord, bool> matchCondition, Action<IAppFinRecord> messageAction) =>
            delivery.AppFinRecords.ForAny(matchCondition, messageAction);

        public bool HasInvalidFinancialDate(IAppFinRecord record) =>
            record.AFinDate > _fileData.FilePreparationDate();

        public void Validate(ILearner objectToValidate)
        {
            var learnRefNumber = objectToValidate.LearnRefNumber;

            objectToValidate.LearningDeliveries
                .ForEach(x => CheckDeliveryAFRs(x, HasInvalidFinancialDate, y => RaiseValidationMessage(learnRefNumber, x, y)));
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearningDelivery thisDelivery, IAppFinRecord thisRecord)
        {
            HandleValidationError(learnRefNumber, thisDelivery.AimSeqNumber, BuildMessageParametersFor(thisRecord));
        }

        public IEnumerable<IErrorMessageParameter> BuildMessageParametersFor(IAppFinRecord thisRecord)
        {
            return new[]
            {
                BuildErrorMessageParameter(PropertyNameConstants.AFinDate, thisRecord.AFinDate),
                BuildErrorMessageParameter(PropertyNameConstants.FilePreparationDate, _fileData.FilePreparationDate())
            };
        }
    }
}
