﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Data.External.LARS.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Derived.Interface;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.LearnDelFAMType;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Tests.Abstract;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.LearnDelFAMType
{
    public class LearnDelFAMType_65RuleTests : AbstractRuleTests<LearnDelFAMType_65Rule>
    {
        [Fact]
        public void RuleName()
        {
            NewRule().RuleName.Should().Be("LearnDelFAMType_65");
        }

        [Fact]
        public void ValidationPasses()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));

            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsMatchForLearnAimRefAndStartDate(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>(),
                        It.IsAny<DateTime>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
                .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
                .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2017, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                                LearnDelFAMCode = "2"
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_NoLDs()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var testLearner = new TestLearner();

            NewRule(validationErrorHandlerMock.Object).Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_NoFAMs()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var testLearner = new TestLearner
            {
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 25
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object).Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_IrrelevantFundingModel()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var testLearner = new TestLearner
            {
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 4,
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ASL
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object).Validate(testLearner);
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public void ValidationPasses_DerivedDataExceptions(bool dd07, bool dd28, bool dd29)
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));

            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(dd07);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(dd28);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(dd29);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2017, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                                LearnDelFAMCode = "1"
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_NVQException()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));

            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("3");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2017, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                                LearnDelFAMCode = "1"
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_BasicSkillException()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var learningDelivery = new TestLearningDelivery
            {
                FundModel = 35,
                LearnAimRef = "00103212",
                LearnStartDate = new DateTime(2018, 8, 1),
                LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                {
                    new TestLearningDeliveryFAM
                    {
                        LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                        LearnDelFAMCode = "1"
                    }
                }
            };

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));

            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(true);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);
            dateTimeServiceMock.Setup(x => x.IsDateBetween(learningDelivery.LearnStartDate, mockLARSLearningDelivery.Object.EffectiveFrom, mockLARSLearningDelivery.Object.EffectiveTo ?? DateTime.MaxValue, true))
                .Returns(true);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    learningDelivery
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_StartDate()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));
            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2016, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                                LearnDelFAMCode = "1"
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_AgeException()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));
            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(17);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1999, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2017, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                                LearnDelFAMCode = "1"
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ValidationPasses_PriorAttainments()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));
            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 1,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2017, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                                LearnDelFAMCode = "1"
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Theory]
        [InlineData(LearningDeliveryFAMTypeConstants.FFI, "2")]
        [InlineData(LearningDeliveryFAMTypeConstants.ACT, "1")]
        public void ValidationPasses_IrrelevantFam(string famType, string famCode)
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));
            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2017, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = famType,
                                LearnDelFAMCode = famCode
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ValidationFails()
        {
            var validationErrorHandlerMock = BuildValidationErrorHandlerMockForError();

            var mockLARSLearningDelivery = new Mock<ILARSLearningDelivery>();
            mockLARSLearningDelivery
                .SetupGet(x => x.LearnAimRef)
                .Returns("00103212");
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveFrom)
                .Returns(new DateTime(2018, 08, 01));
            mockLARSLearningDelivery
                .SetupGet(x => x.EffectiveTo)
                .Returns(new DateTime(2022, 08, 01));
            var larsService = new Mock<ILARSDataService>();
            larsService
                .Setup(m => m.BasicSkillsTypeMatchForLearnAimRef(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<string>()))
                .Returns(false);
            larsService
                .Setup(m => m.GetNotionalNVQLevelv2ForLearnAimRef(It.IsAny<string>()))
                .Returns("1");
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns(mockLARSLearningDelivery.Object);

            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);

            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);

            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);

            var dateTimeServiceMock = new Mock<IDateTimeQueryService>();
            dateTimeServiceMock
                .Setup(m => m.YearsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(20);

            var testLearner = new TestLearner
            {
                DateOfBirthNullable = new DateTime(1997, 8, 1),
                PriorAttainNullable = 2,
                LearningDeliveries = new List<TestLearningDelivery>
                {
                    new TestLearningDelivery
                    {
                        FundModel = 35,
                        LearnAimRef = "00103212",
                        LearnStartDate = new DateTime(2017, 8, 1),
                        LearningDeliveryFAMs = new List<TestLearningDeliveryFAM>
                        {
                            new TestLearningDeliveryFAM
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.FFI,
                                LearnDelFAMCode = "1"
                            }
                        }
                    }
                }
            };

            NewRule(validationErrorHandlerMock.Object, larsService.Object, dd07Mock.Object, dd28Mock.Object, dd29Mock.Object, dateTimeServiceMock.Object)
                .Validate(testLearner);
        }

        [Fact]
        public void ExclusionsApply_NullLARSLearningDelivery()
        {
            var dd07Mock = new Mock<IDerivedData_07Rule>();
            dd07Mock
                .Setup(m => m.IsApprenticeship(It.IsAny<int?>()))
                .Returns(false);
            var dd28Mock = new Mock<IDerivedData_28Rule>();
            dd28Mock
                .Setup(m => m.IsAdultFundedUnemployedWithBenefits(It.IsAny<ILearningDelivery>(), It.IsAny<ILearner>()))
                .Returns(false);
            var dd29Mock = new Mock<IDerivedData_29Rule>();
            dd29Mock
                .Setup(m => m.IsInflexibleElementOfTrainingAimLearningDelivery(It.IsAny<ILearningDelivery>()))
                .Returns(false);
            var learningDeliveryFAM = new Mock<ILearningDeliveryFAM>();
            var learningDelivery = new Mock<ILearningDelivery>();
            learningDelivery
                .Setup(x => x.LearningDeliveryFAMs).Returns(new List<ILearningDeliveryFAM> { learningDeliveryFAM.Object });
            var learner = new Mock<ILearner>();
            learner
                .Setup(x => x.LearningDeliveries).Returns(new List<ILearningDelivery> { learningDelivery.Object });
            var larsService = new Mock<ILARSDataService>();
            larsService
               .Setup(x => x.GetDeliveryFor(It.IsAny<string>()))
               .Returns((ILARSLearningDelivery)null);
            NewRule(larsDataService: larsService.Object, dd07: dd07Mock.Object, derivedData28Rule: dd28Mock.Object, derivedData29Rule: dd29Mock.Object)
                .ExclusionsApply(learner.Object, learningDelivery.Object).Should().BeTrue();
        }

        [Theory]
        [InlineData("2016-08-01", true)]
        [InlineData("2017-07-31", true)]
        [InlineData("2017-08-01", false)]
        [InlineData("2017-09-14", false)]
        [InlineData("2020-07-31", false)]
        [InlineData("2020-08-01", true)]
        [InlineData("2021-08-01", true)]
        public void LearnStartDateIsOutsideValidDateRange_MeetsExpectation(string candidate, bool expectation)
        {
            // arrange
            var sut = NewRule();
            var learnStartDate = DateTime.Parse(candidate);

            // act
            var result = sut.LearnStartDateIsOutsideValidDateRange(learnStartDate);

            // assert
            Assert.Equal(expectation, result);
        }

        private LearnDelFAMType_65Rule NewRule(
            IValidationErrorHandler validationErrorHandler = null,
            ILARSDataService larsDataService = null,
            IDerivedData_07Rule dd07 = null,
            IDerivedData_28Rule derivedData28Rule = null,
            IDerivedData_29Rule derivedData29Rule = null,
            IDateTimeQueryService dateTimeQueryService = null)
        {
            return new LearnDelFAMType_65Rule(validationErrorHandler, larsDataService, dd07, derivedData28Rule, derivedData29Rule, dateTimeQueryService);
        }
    }
}
