﻿using System;
using System.Collections.Generic;
using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Data.External.FCS.Interface;
using ESFA.DC.ILR.ValidationService.Data.External.FCS.Model;
using ESFA.DC.ILR.ValidationService.Data.File.FileData.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.UKPRN;
using ESFA.DC.ILR.ValidationService.Rules.Tests.Abstract;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.UKPRN
{
    public class UKPRN_21RuleTests : AbstractRuleTests<UKPRN_21Rule>
    {
        [Fact]
        public void RuleName()
        {
            NewRule().RuleName.Should().Be("UKPRN_21");
        }

        [Fact]
        public void ContractAllocationsForUkprnAndFundingStreamPeriodCodes_NullFcsContractAllocations()
        {
            // Arrange
            var fileDataServiceMock = new Mock<IFileDataService>();
            fileDataServiceMock.Setup(s => s.UKPRN()).Returns(42);

            List<IFcsContractAllocation> contractAllocationsForUkprn = null;

            var fcsDataServiceMock = new Mock<IFCSDataService>();
            fcsDataServiceMock.Setup(s => s.GetContractAllocationsFor(42)).Returns(contractAllocationsForUkprn);

            var learner = new TestLearner
            {
                LearningDeliveries = new TestLearningDelivery[]
                {
                    new TestLearningDelivery()
                    {
                        ConRefNumber = "ConRef1",
                        FundModel = FundModels.ApprenticeshipsFrom1May2017,
                        LearnStartDate = new DateTime(2019, 01, 01)
                    }
                }
            };

            // Act & Assert
            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(
                    fileDataServiceMock.Object,
                    fcsDataServiceMock.Object,
                    validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void ContractAllocationsForUkprnAndFundingStreamPeriodCodes_FcsContractAllocationsContainsNulls()
        {
            // Arrange
            var fileDataServiceMock = new Mock<IFileDataService>();
            fileDataServiceMock.Setup(s => s.UKPRN()).Returns(42);

            var contractAllocationsForUkprn = new List<IFcsContractAllocation> { null };

            var fcsDataServiceMock = new Mock<IFCSDataService>();
            fcsDataServiceMock.Setup(s => s.GetContractAllocationsFor(42)).Returns(contractAllocationsForUkprn);

            var learner = new TestLearner
            {
                LearningDeliveries = new TestLearningDelivery[]
                {
                    new TestLearningDelivery()
                    {
                        ConRefNumber = "ConRef1",
                        FundModel = FundModels.ApprenticeshipsFrom1May2017,
                        LearnStartDate = new DateTime(2019, 01, 01),
                        LearningDeliveryFAMs = new TestLearningDeliveryFAM[]
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ACT,
                                LearnDelFAMCode = LearningDeliveryFAMCodeConstants.ACT_ContractESFA
                            }
                        }
                    }
                }
            };

            // Act & Assert
            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(
                    fileDataServiceMock.Object,
                    fcsDataServiceMock.Object,
                    validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void ContractAllocationsForUkprnAndFundingStreamPeriodCodes_LearningDeliveryWithNullConRef()
        {
            // Arrange
            var fileDataServiceMock = new Mock<IFileDataService>();
            fileDataServiceMock.Setup(s => s.UKPRN()).Returns(42);

            var contractAllocationsForUkprn = new List<IFcsContractAllocation>
            {
                new FcsContractAllocation { DeliveryUKPRN = 42, FundingStreamPeriodCode = FundingStreamPeriodCodeConstants.LEVY1799 },
            };

            var fcsDataServiceMock = new Mock<IFCSDataService>();
            fcsDataServiceMock.Setup(s => s.GetContractAllocationsFor(42)).Returns(contractAllocationsForUkprn);

            var learner = new TestLearner
            {
                LearningDeliveries = new TestLearningDelivery[]
                {
                    new TestLearningDelivery()
                    {
                        ConRefNumber = null,
                        FundModel = FundModels.ApprenticeshipsFrom1May2017,
                        LearnStartDate = new DateTime(2019, 01, 01),
                        LearningDeliveryFAMs = new TestLearningDeliveryFAM[]
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ACT,
                                LearnDelFAMCode = LearningDeliveryFAMCodeConstants.ACT_ContractESFA
                            }
                        }
                    }
                }
            };

            // Act & Assert
            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(
                    fileDataServiceMock.Object,
                    fcsDataServiceMock.Object,
                    validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void ContractAllocationsForUkprnAndFundingStreamPeriodCodes_NullLearningDeliveries()
        {
            // Arrange
            var fileDataServiceMock = new Mock<IFileDataService>();
            fileDataServiceMock.Setup(s => s.UKPRN()).Returns(42);

            var contractAllocationsForUkprn = new List<IFcsContractAllocation>
            {
                new FcsContractAllocation { DeliveryUKPRN = 42, FundingStreamPeriodCode = FundingStreamPeriodCodeConstants.LEVY1799 },
            };

            var fcsDataServiceMock = new Mock<IFCSDataService>();
            fcsDataServiceMock.Setup(s => s.GetContractAllocationsFor(42)).Returns(contractAllocationsForUkprn);

            var learner = new TestLearner
            {
                LearningDeliveries = null
            };

            // Act & Assert
            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(
                    fileDataServiceMock.Object,
                    fcsDataServiceMock.Object,
                    validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void ContractAllocationsForUkprnAndFundingStreamPeriodCodes_FiltersOutNonMatchingPeriodCodes()
        {
            // Arrange
            var fileDataServiceMock = new Mock<IFileDataService>();
            fileDataServiceMock.Setup(s => s.UKPRN()).Returns(42);

            var contractAllocationsForUkprn = new List<IFcsContractAllocation>
            {
                new FcsContractAllocation { DeliveryUKPRN = 42, FundingStreamPeriodCode = "AAAAA" },
                new FcsContractAllocation { DeliveryUKPRN = 42, FundingStreamPeriodCode = FundingStreamPeriodCodeConstants.LEVY1799 },
                new FcsContractAllocation { DeliveryUKPRN = 42, FundingStreamPeriodCode = FundingStreamPeriodCodeConstants.NONLEVY2019 },
                new FcsContractAllocation { DeliveryUKPRN = 42, FundingStreamPeriodCode = "ZZZZZ" },
            };

            var fcsDataServiceMock = new Mock<IFCSDataService>();
            fcsDataServiceMock.Setup(s => s.GetContractAllocationsFor(42)).Returns(contractAllocationsForUkprn);

            var rule = NewRule(fileDataServiceMock.Object, fcsDataServiceMock.Object, null);

            // Act
            var filtered = rule.ContractAllocationsForUkprnAndFundingStreamPeriodCodes(42);

            // Assert
            filtered.Should().NotBeNull();
            filtered.Should().NotBeEmpty();
            filtered.Should().HaveCount(2);
            filtered.Should().Contain(ca => ca.FundingStreamPeriodCode == FundingStreamPeriodCodeConstants.LEVY1799);
            filtered.Should().Contain(ca => ca.FundingStreamPeriodCode == FundingStreamPeriodCodeConstants.NONLEVY2019);
        }

        public static IEnumerable<object[]> ConditionMet_TestData()
        {
            yield return new object[] { "TestValue2", new DateTime(2018, 12, 31), true };
            yield return new object[] { "TestValue2", null, false };
            yield return new object[] { "TestValue1", null, false };
            yield return new object[] { "TestValue1", new DateTime(2019, 12, 31), false };
            yield return new object[] { "TestValue1", new DateTime(2019, 01, 01), true };
            yield return new object[] { "TestValue1", new DateTime(2018, 12, 31), true };
        }

        [Theory]
        [MemberData(nameof(ConditionMet_TestData))]
        public void ConditionMet(string contractAllocationNumber, DateTime? stopNewStartsFromDate, bool expectedResult)
        {
            // Arrange
            var contractAllocations = new List<IFcsContractAllocation>
            {
                new FcsContractAllocation { ContractAllocationNumber = contractAllocationNumber, StopNewStartsFromDate = stopNewStartsFromDate }
            };
            var learningDelivery = new TestLearningDelivery
            {
                ConRefNumber = "TestValue1",
                LearnStartDate = new DateTime(2019, 01, 01)
            };

            // Act
            var result = NewRule().ConditionMet(learningDelivery, contractAllocations);

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public void ConditionMet_Fail_Uses_Latest_Contract_Allocation()
        {
            // Arrange
            var contractAllocations = new List<IFcsContractAllocation>
            {
                new FcsContractAllocation { ContractAllocationNumber = "TestValue1", StartDate = new DateTime(2019, 01, 01), StopNewStartsFromDate = new DateTime(2019, 12, 31) },
                new FcsContractAllocation { ContractAllocationNumber = "TestValue1", StartDate = new DateTime(2019, 02, 01), StopNewStartsFromDate = null }
            };
            var learningDelivery = new TestLearningDelivery
            {
                ConRefNumber = "TestValue1",
                LearnStartDate = new DateTime(2019, 01, 01)
            };

            // Act
            var result = NewRule().ConditionMet(learningDelivery, contractAllocations);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ConditionMet_Pass_Uses_Latest_Contract_Allocation()
        {
            // Arrange
            var contractAllocations = new List<IFcsContractAllocation>
            {
                new FcsContractAllocation { ContractAllocationNumber = "TestValue1", StartDate = new DateTime(2019, 02, 01), StopNewStartsFromDate = new DateTime(2019, 12, 31) },
                new FcsContractAllocation { ContractAllocationNumber = "TestValue1", StartDate = new DateTime(2019, 01, 01), StopNewStartsFromDate = null }
            };
            var learningDelivery = new TestLearningDelivery
            {
                ConRefNumber = "TestValue1",
                LearnStartDate = new DateTime(2019, 01, 01)
            };

            // Act
            var result = NewRule().ConditionMet(learningDelivery, contractAllocations);

            // Assert
            result.Should().BeFalse();
        }

        public static IEnumerable<object[]> Validate_TestData()
        {
            yield return new object[] { "ConRef2", FundingStreamPeriodCodeConstants.LEVY1799, new DateTime(2018, 12, 31), 1, true };
            yield return new object[] { "ConRef1", FundingStreamPeriodCodeConstants.AEBTO_TOL1920, new DateTime(2018, 12, 31), 1, false };
            yield return new object[] { "ConRef1", FundingStreamPeriodCodeConstants.LEVY1799, new DateTime(2019, 12, 31), 1, false };
            yield return new object[] { "ConRef1", FundingStreamPeriodCodeConstants.LEVY1799, new DateTime(2019, 01, 02), 1, false };
            yield return new object[] { "ConRef1", FundingStreamPeriodCodeConstants.LEVY1799, new DateTime(2019, 01, 01), 1, true };
            yield return new object[] { "ConRef1", FundingStreamPeriodCodeConstants.LEVY1799, new DateTime(2018, 12, 31), 1, true };
        }

        [Theory]
        [MemberData(nameof(Validate_TestData))]
        public void Validate(string contractAllocationNumber, string fundingStreamPeriodCode, DateTime? stopNewStartsFromDate, int aimType, bool expectViolation)
        {
            // Arrange
            var fileDataServiceMock = new Mock<IFileDataService>();
            fileDataServiceMock.Setup(s => s.UKPRN()).Returns(42);

            var contractAllocationsForUkprn = new List<IFcsContractAllocation>
            {
                new FcsContractAllocation
                {
                    ContractAllocationNumber = contractAllocationNumber,
                    DeliveryUKPRN = 42,
                    FundingStreamPeriodCode = fundingStreamPeriodCode,
                    StopNewStartsFromDate = stopNewStartsFromDate
                }
            };

            var fcsDataServiceMock = new Mock<IFCSDataService>();
            fcsDataServiceMock.Setup(s => s.GetContractAllocationsFor(42)).Returns(contractAllocationsForUkprn);

            var learner = new TestLearner
            {
                LearningDeliveries = new TestLearningDelivery[]
                {
                    new TestLearningDelivery()
                    {
                        ConRefNumber = "ConRef1",
                        AimType = aimType,
                        FundModel = FundModels.ApprenticeshipsFrom1May2017,
                        LearnStartDate = new DateTime(2019, 01, 01),
                        LearningDeliveryFAMs = new TestLearningDeliveryFAM[]
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ACT,
                                LearnDelFAMCode = LearningDeliveryFAMCodeConstants.ACT_ContractEmployer
                            }
                        }
                    }
                }
            };

            // Act & Assert
            using (var validationErrorHandlerMock = expectViolation ? BuildValidationErrorHandlerMockForError() : BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(
                    fileDataServiceMock.Object,
                    fcsDataServiceMock.Object,
                    validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void Validate_NoContractAllocations_NoViolation()
        {
            // Arrange
            var fileDataServiceMock = new Mock<IFileDataService>();
            fileDataServiceMock.Setup(s => s.UKPRN()).Returns(42);

            var contractAllocationsForUkprn = new List<IFcsContractAllocation> { };

            var fcsDataServiceMock = new Mock<IFCSDataService>();
            fcsDataServiceMock.Setup(s => s.GetContractAllocationsFor(42)).Returns(contractAllocationsForUkprn);

            var learner = new TestLearner
            {
                LearningDeliveries = new TestLearningDelivery[]
                {
                    new TestLearningDelivery()
                    {
                        AimType = 1,
                        FundModel = FundModels.ApprenticeshipsFrom1May2017,
                        LearnStartDate = new DateTime(2019, 01, 01),
                        LearningDeliveryFAMs = new TestLearningDeliveryFAM[]
                        {
                            new TestLearningDeliveryFAM()
                            {
                                LearnDelFAMType = LearningDeliveryFAMTypeConstants.ACT,
                                LearnDelFAMCode = LearningDeliveryFAMCodeConstants.ACT_ContractESFA
                            }
                        }
                    }
                }
            };

            // Act
            using (var validationErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(
                    fileDataServiceMock.Object,
                    fcsDataServiceMock.Object,
                    validationErrorHandlerMock.Object).Validate(learner);
            }
        }

        private UKPRN_21Rule NewRule(
            IFileDataService fileDataService = null,
            IFCSDataService fcsDataService = null,
            IValidationErrorHandler validationErrorHandler = null)
        {
            return new UKPRN_21Rule(fileDataService, fcsDataService, validationErrorHandler);
        }
    }
}
