﻿using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.ValidationActor.Interfaces.Models;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.ILR.ValidationService.ValidationActor.Context
{
    public class ValidationActorModelValidationContextFactory : IValidationContextFactory<ValidationActorModel>
    {
        private readonly IJsonSerializationService _jsonSerializationService;

        public ValidationActorModelValidationContextFactory(IJsonSerializationService jsonSerializationService)
        {
            _jsonSerializationService = jsonSerializationService;
        }

        public IValidationContext Build(ValidationActorModel context)
        {
            return new ValidationActorModelValidationContext();
        }
    }
}
