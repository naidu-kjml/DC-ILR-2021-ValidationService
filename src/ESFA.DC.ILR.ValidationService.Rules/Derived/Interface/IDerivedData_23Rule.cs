﻿using ESFA.DC.ILR.Model.Interface;

namespace ESFA.DC.ILR.ValidationService.Rules.Derived.Interface
{
    public interface IDerivedData_23Rule
    {
        int? GetLearnersAgeAtStartOfESFContract(
            ILearner learner,
            string conRefNumber);
    }
}
