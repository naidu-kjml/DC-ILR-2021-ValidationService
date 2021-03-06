﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Data.External.LARS.Interface;
using ESFA.DC.ILR.ValidationService.Data.External.LARS.Model;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.LearnStartDate;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Tests.Abstract;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.LearnStartDate
{
    public class LearnStartDate_18RuleTests : AbstractRuleTests<LearnStartDate_18Rule>
    {
        [Fact]
        public void RuleName()
        {
            var sut = NewRule();

            var result = sut.RuleName;

            Assert.Equal("LearnStartDate_18", result);
        }

        [Theory]
        [InlineData(1, LearningDeliveryFAMTypeConstants.ACT, "2018-07-31", false)] // LearnStartDate < LARS.LastDateStarts and !RES
        [InlineData(1, LearningDeliveryFAMTypeConstants.ACT, "2018-08-01", false)] // LearnStartDate == LARS.LastDateStarts and !RES
        [InlineData(1, LearningDeliveryFAMTypeConstants.RES, "2018-08-02", true)] // LearnStartDate > LATS.LastDateStarts and == RES
        public void Validate_NoError(int? stdCode, string famType, string startDate, bool returns)
        {
            var larsStandard = new LARSStandard()
            {
                StandardCode = stdCode.Value,
                LastDateStarts = new DateTime(2018, 08, 01)
            };

            var learner = new TestLearner()
            {
                LearningDeliveries = new List<TestLearningDelivery>()
                {
                    new TestLearningDelivery()
                    {
                        StdCodeNullable = stdCode,
                        AimType = 1,
                        ProgTypeNullable = 25,
                        LearnStartDate = DateTime.Parse(startDate),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>()
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = famType
                            }
                        }
                    }
                }
            };

            var larsDataServiceMock = new Mock<ILARSDataService>();
            larsDataServiceMock.Setup(l => l.GetStandardFor(stdCode.Value)).Returns(larsStandard);

            var learningDeliveryFAMQueryServiceMock = new Mock<ILearningDeliveryFAMQueryService>();
            learningDeliveryFAMQueryServiceMock
                .Setup(qs => qs.HasLearningDeliveryFAMType(It.IsAny<IEnumerable<ILearningDeliveryFAM>>(), "RES"))
                .Returns(returns);

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(larsDataServiceMock.Object, learningDeliveryFAMQueryServiceMock.Object, validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void Validate_NoError_StdCode()
        {
            var learner = new TestLearner()
            {
                LearningDeliveries = new List<TestLearningDelivery>()
                {
                    new TestLearningDelivery()
                    {
                        StdCodeNullable = 0,
                        AimType = 1,
                        ProgTypeNullable = 25,
                        LearnStartDate = new DateTime(2020, 08, 01),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>()
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ACT
                            }
                        }
                    }
                }
            };

            var larsDataServiceMock = new Mock<ILARSDataService>();
            larsDataServiceMock.Setup(l => l.GetStandardFor(0)).Returns(new LARSStandard());

            var learningDeliveryFAMQueryServiceMock = new Mock<ILearningDeliveryFAMQueryService>();
            learningDeliveryFAMQueryServiceMock
                .Setup(qs => qs.HasLearningDeliveryFAMType(It.IsAny<IEnumerable<ILearningDeliveryFAM>>(), "RES"))
                .Returns(false);

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(larsDataServiceMock.Object, learningDeliveryFAMQueryServiceMock.Object, validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void Validate_Error()
        {
            int stdCode = 1;
            var larsStandard = new LARSStandard()
            {
                StandardCode = stdCode,
                LastDateStarts = new DateTime(2018, 08, 01)
            };

            var learner = new TestLearner()
            {
                LearningDeliveries = new List<TestLearningDelivery>()
                {
                    new TestLearningDelivery()
                    {
                        StdCodeNullable = stdCode,
                        ProgTypeNullable = 25,
                        AimType = 1,
                        LearnStartDate = new DateTime(2020, 08, 02),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>()
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ACT
                            }
                        }
                    }
                }
            };

            var larsDataServiceMock = new Mock<ILARSDataService>();
            larsDataServiceMock.Setup(l => l.GetStandardFor(stdCode)).Returns(larsStandard);

            var learningDeliveryFAMQueryServiceMock = new Mock<ILearningDeliveryFAMQueryService>();
            learningDeliveryFAMQueryServiceMock
                .Setup(qs => qs.HasLearningDeliveryFAMType(It.IsAny<IEnumerable<ILearningDeliveryFAM>>(), "RES"))
                .Returns(false);

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForError())
            {
                NewRule(larsDataServiceMock.Object, learningDeliveryFAMQueryServiceMock.Object, validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void Validate_NoError_InvalidStart()
        {
            int stdCode = 1;
            var larsStandard = new LARSStandard()
            {
                StandardCode = stdCode,
                LastDateStarts = new DateTime(2018, 08, 01)
            };

            var learner = new TestLearner()
            {
                LearningDeliveries = new List<TestLearningDelivery>()
                {
                    new TestLearningDelivery()
                    {
                        StdCodeNullable = stdCode,
                        ProgTypeNullable = 25,
                        AimType = 1,
                        LearnStartDate = new DateTime(2020, 07, 02),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>()
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ACT
                            }
                        }
                    }
                }
            };

            var larsDataServiceMock = new Mock<ILARSDataService>();
            larsDataServiceMock.Setup(l => l.GetStandardFor(stdCode)).Returns(larsStandard);

            var learningDeliveryFAMQueryServiceMock = new Mock<ILearningDeliveryFAMQueryService>();
            learningDeliveryFAMQueryServiceMock
                .Setup(qs => qs.HasLearningDeliveryFAMType(It.IsAny<IEnumerable<ILearningDeliveryFAM>>(), "RES"))
                .Returns(false);

            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(larsDataServiceMock.Object, learningDeliveryFAMQueryServiceMock.Object, validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Theory]
        [InlineData(8)]
        [InlineData(9)]
        public void LearnStartDateConditionMet_True(int month)
        {
            NewRule().LearnStartDateConditionMet(new DateTime(2020, month, 1)).Should().BeTrue();
        }

        [Fact]
        public void LearnStartDateConditionMet_False()
        {
            NewRule().LearnStartDateConditionMet(new DateTime(2020, 7, 1)).Should().BeFalse();
        }

        [Fact]
        public void ProgTypeConditionMet_True()
        {
            NewRule().ProgTypeConditionMet(25).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public void ProgTypeConditionMet_False(int? progType)
        {
            NewRule().ProgTypeConditionMet(progType).Should().BeFalse();
        }

        [Fact]
        public void AimTypeConditionMet_True()
        {
            NewRule().AimTypeConditionMet(1).Should().BeTrue();
        }

        [Fact]
        public void AimTypeConditionMet_False()
        {
            NewRule().AimTypeConditionMet(0).Should().BeFalse();
        }

        [Fact]
        public void StdCodeExists_False()
        {
            NewRule().StdCodeExists(null).Should().BeFalse();
        }

        [Fact]
        public void StdCodeExists_True()
        {
            NewRule().StdCodeExists(25).Should().BeTrue();
        }

        private LearnStartDate_18Rule NewRule(
          ILARSDataService larsDataService = null,
          ILearningDeliveryFAMQueryService learningDeliveryFamQueryService = null,
          IValidationErrorHandler validationErrorHandler = null)
        {
            return new LearnStartDate_18Rule(larsDataService, learningDeliveryFamQueryService, validationErrorHandler);
        }
    }
}
