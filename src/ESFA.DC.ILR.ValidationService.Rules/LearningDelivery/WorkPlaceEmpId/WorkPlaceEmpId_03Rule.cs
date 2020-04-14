﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Data.File.FileData.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Utility;
using System;
using System.Collections.Generic;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.WorkPlaceEmpId
{
    public class WorkPlaceEmpId_03Rule :
        AbstractRule,
        IRule<ILearner>
    {
        public const int TemporaryEmpID = 999999999;

        public const string Name = RuleNameConstants.WorkPlaceEmpId_03;

        private readonly IFileDataService _fileDataService;

        public WorkPlaceEmpId_03Rule(
            IValidationErrorHandler validationErrorHandler,
            IFileDataService fileDataService)
            : base(validationErrorHandler, Name)
        {
            It.IsNull(validationErrorHandler)
                .AsGuard<ArgumentNullException>(nameof(validationErrorHandler));
            It.IsNull(fileDataService)
                .AsGuard<ArgumentNullException>(nameof(fileDataService));

            _fileDataService = fileDataService;
        }

        public TimeSpan SixtyDays => new TimeSpan(60, 0, 0, 0);   

        public bool IsQualifyingProgramme(ILearningDelivery delivery) =>
            It.IsInRange(delivery.ProgTypeNullable, TypeOfLearningProgramme.Traineeship);

        public bool IsInsideTheRegistrationPeriod(ILearningDeliveryWorkPlacement placement) =>
            (_fileDataService.FilePreparationDate() - placement.WorkPlaceStartDate) <= SixtyDays;

        public bool RequiresEmployerRegistration(ILearningDeliveryWorkPlacement placement) =>
            It.IsInRange(placement.WorkPlaceEmpIdNullable, TemporaryEmpID);

        public bool IsNotValid(ILearningDeliveryWorkPlacement placement) =>
            RequiresEmployerRegistration(placement) && IsInsideTheRegistrationPeriod(placement);

        public bool IsNotValid(ILearningDelivery delivery) =>
            IsQualifyingProgramme(delivery)
                && delivery.LearningDeliveryWorkPlacements.NullSafeAny(IsNotValid);

        public void Validate(ILearner objectToValidate)
        {
            It.IsNull(objectToValidate)
                .AsGuard<ArgumentNullException>(nameof(objectToValidate));

            var learnRefNumber = objectToValidate.LearnRefNumber;

            objectToValidate.LearningDeliveries
                .NullSafeWhere(IsNotValid)
                .ForEach(x => RaiseValidationMessage(learnRefNumber, x));
        }

        public void RaiseValidationMessage(string learnRefNumber, ILearningDelivery thisDelivery)
        {
            HandleValidationError(learnRefNumber, thisDelivery.AimSeqNumber, BuildErrorMessageParameters());
        }

        public IEnumerable<IErrorMessageParameter> BuildErrorMessageParameters()
        {
            return new[]
            {
                BuildErrorMessageParameter(PropertyNameConstants.WorkPlaceEmpId, TemporaryEmpID)
            };
        }
    }
}
