﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.LearnDelFAMDateTo;
using ESFA.DC.ILR.ValidationService.Rules.Tests.Abstract;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.LearnDelFAMDateTo
{
    public class LearnDelFAMDateTo_02RuleTests : AbstractRuleTests<LearnDelFAMDateTo_02Rule>
    {
        [Fact]
        public void RuleName()
        {
            NewRule().RuleName.Should().Be("LearnDelFAMDateTo_02");
        }

        [Fact]
        public void ConditionMet_True()
        {
            NewRule().ConditionMet(new DateTime(2017, 1, 1), new DateTime(2016, 1, 1)).Should().BeTrue();
        }

        [Fact]
        public void ConditionMet_False_DateToNull()
        {
            NewRule().ConditionMet(null, new DateTime(2017, 1, 1)).Should().BeFalse();
        }

        [Fact]
        public void ConditionMet_False()
        {
            NewRule().ConditionMet(new DateTime(2016, 1, 1), new DateTime(2017, 1, 1)).Should().BeFalse();
        }

        [Fact]
        public void Validate_Error()
        {
            var learner = new TestLearner()
            {
                LearningDeliveries = new List<TestLearningDelivery>()
                {
                    new TestLearningDelivery()
                    {
                        LearnPlanEndDate = new DateTime(2016, 1, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>()
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMDateToNullable = new DateTime(2017, 1, 1),
                            }
                        }
                    }
                }
            };

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForError())
            {
                NewRule(validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void Validate_NoErrors()
        {
            var learner = new TestLearner()
            {
                LearningDeliveries = new List<TestLearningDelivery>()
                {
                    new TestLearningDelivery()
                    {
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>()
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMDateToNullable = null
                            }
                        }
                    }
                }
            };

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void BuildErrorMessageParameters()
        {
            var validationErrorHandlerMock = new Mock<IValidationErrorHandler>();

            validationErrorHandlerMock.Setup(veh => veh.BuildErrorMessageParameter("LearnPlanEndDate", "01/01/2016")).Verifiable();
            validationErrorHandlerMock.Setup(veh => veh.BuildErrorMessageParameter("LearnDelFAMDateTo", "01/01/2017")).Verifiable();

            NewRule(validationErrorHandlerMock.Object).BuildErrorMessageParameters(new DateTime(2016, 1, 1), new DateTime(2017, 1, 1));

            validationErrorHandlerMock.Verify();
        }

        private LearnDelFAMDateTo_02Rule NewRule(IValidationErrorHandler validationErrorHandler = null)
        {
            return new LearnDelFAMDateTo_02Rule(validationErrorHandler);
        }
    }
}
