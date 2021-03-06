﻿using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.PHours;
using ESFA.DC.ILR.ValidationService.Rules.Tests.Abstract;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.PHours
{
    public class PHours_01RuleTests : AbstractRuleTests<PHours_01Rule>
    {
        [Fact]
        public void RuleName()
        {
            NewRule().RuleName.Should().Be("PHours_01");
        }

        [Fact]
        public void StartDateCondition_Pass_AsStartDateIsEqual()
        {
            var startDate = new DateTime(2019, 8, 1);

            var rule = NewRule().StartDateConditionMet(startDate);
            rule.Should().BeTrue();
        }

        [Fact]
        public void StartDateCondition_Pass_AsStartDateIsGreater()
        {
            var startDate = new DateTime(2019, 12, 1);

            var rule = NewRule().StartDateConditionMet(startDate);
            rule.Should().BeTrue();
        }

        [Fact]
        public void StartDateCondition_Fails_AsStartDateIsLessThan()
        {
            var startDate = new DateTime(2019, 6, 1);

            var rule = NewRule().StartDateConditionMet(startDate);
            rule.Should().BeFalse();
        }

        [Fact]
        public void PlannedHoursConditionMet_Fails_AsPHrsNotNull()
        {
            int? pHours = 40;
            NewRule().PlannedHoursConditionMet(pHours).Should().BeFalse();
        }

        [Fact]
        public void PlannedHoursConditionMet_Pass_AsNull()
        {
            int? pHours = null;
            NewRule().PlannedHoursConditionMet(pHours).Should().BeTrue();
        }

        [Fact]
        public void PlannedHoursConditionMet_Pass_AsZeroValue()
        {
            int? pHours = 0;
            NewRule().PlannedHoursConditionMet(pHours).Should().BeFalse();
        }

        [Fact]
        public void FundModelConditionMet_Pass()
        {
            var fundModel = 36;
            NewRule().FundModelConditionMet(fundModel).Should().BeTrue();
        }

        [Fact]
        public void AimTypeConditionMet_Pass_AsOne()
        {
            var aimType = 1;
            NewRule().AimTypeConditionMet(aimType).Should().BeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(2)]
        public void AimTypeConditionMet_Fail_NonOne(int aimType)
        {
            NewRule().AimTypeConditionMet(aimType).Should().BeFalse();
        }

        [Fact]
        public void FundModelConditionMet_Fails()
        {
            var fundModel = 81;
            NewRule().FundModelConditionMet(fundModel).Should().BeFalse();
        }

        [Fact]
        public void ConditionMet_Pass()
        {
            var startDate = new DateTime(2019, 12, 1);
            int? pHours = null;
            int fundModel = 36;
            int aimType = 1;

            var rule = NewRule().ConditionMet(startDate, pHours, fundModel, aimType);
            rule.Should().BeTrue();
        }

        [Theory]
        [InlineData("01/06/2019", 200, 36, 1)]
        [InlineData("01/08/2019", 250, 36, 1)]
        [InlineData("01/12/2019", 300, 81, 1)]
        [InlineData("01/12/2019", null, 36, 2)]
        public void ConditionMet_Fails(string startingDate, int? pHours, int fundModel, int aimType)
        {
            var startDate = DateTime.Parse(startingDate);
            var rule = NewRule().ConditionMet(startDate, pHours, fundModel, aimType);
            rule.Should().BeFalse();
        }

        [Fact]
        public void Validate_Error()
        {
            var learnStartDate = new DateTime(2019, 12, 01);

            var learningDeliveries = new List<TestLearningDelivery>()
            {
                 new TestLearningDelivery
                 {
                     FundModel = 36,
                     AimType = 1,
                     PHoursNullable = null,
                     LearnStartDate = learnStartDate
                 }
            };

            var learner = new TestLearner()
            {
                LearnRefNumber = "LearnRefNumber",
                LearningDeliveries = learningDeliveries
            };

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForError())
            {
                NewRule(validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void Validate_NoError()
        {
            var learnStartDate = new DateTime(2019, 12, 01);
            var learningDeliveries = new List<TestLearningDelivery>()
            {
                 new TestLearningDelivery
                 {
                     FundModel = 81, // fundModel doesn't meet the condition, hence no Error!
                     AimType = 1,
                     PHoursNullable = 200,
                     LearnStartDate = learnStartDate
                 }
            };

            var learner = new TestLearner()
            {
                LearnRefNumber = "LearnRefNumber",
                LearningDeliveries = learningDeliveries
            };

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void BuildErrorMessageParameters()
        {
            var fundModel = 36;
            var pHours = 200;
            var aimType = 1;
            var validationErrorHandlerMock = new Mock<IValidationErrorHandler>();

            validationErrorHandlerMock.Setup(x => x.BuildErrorMessageParameter(PropertyNameConstants.FundModel, fundModel)).Verifiable();
            validationErrorHandlerMock.Setup(x => x.BuildErrorMessageParameter(PropertyNameConstants.PHours, pHours)).Verifiable();
            validationErrorHandlerMock.Setup(x => x.BuildErrorMessageParameter(PropertyNameConstants.AimType, aimType)).Verifiable();

            NewRule(validationErrorHandler: validationErrorHandlerMock.Object).BuildErrorMessageParameters(fundModel, pHours, aimType);

            validationErrorHandlerMock.Verify();
        }

        public PHours_01Rule NewRule(IValidationErrorHandler validationErrorHandler = null)
        {
            return new PHours_01Rule(validationErrorHandler);
        }
    }
}
