﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Data.External.FCS.Interface;
using ESFA.DC.ILR.ValidationService.Data.File.FileData.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.UKPRN;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.UKPRN
{
    public class UKPRN_17RuleTests
    {
        public const int TestProviderID = 123456789;

        [Fact]
        public void RuleName()
        {
            var sut = NewRule();

            var result = sut.RuleName;

            Assert.Equal("UKPRN_17", result);
        }

        [Theory]
        [InlineData("16-18TRN2021", FundingStreamPeriodCodeConstants.C16_18TRN2021)]
        public void FundingStreamPeriodCodeMeetsExpectation(string candidate, string expectation)
        {
            Assert.Equal(expectation, candidate);
        }

        [Fact]
        public void ProviderUKPRNMeetsExpectation()
        {
            var sut = NewRule();

            Assert.Equal(TestProviderID, sut.ProviderUKPRN);
        }

        [Theory]
        [InlineData(25, FundModels.Age16To19ExcludingApprenticeships)]
        public void TypeOfFundingMeetsExpectation(int expectation, int candidate)
        {
            Assert.Equal(expectation, candidate);
        }

        [Theory]
        [InlineData(25, true)]
        [InlineData(35, false)]
        public void HasQualifyingModelMeetsExpectation(int fundModel, bool expectation)
        {
            var mockItem = new Mock<ILearningDelivery>();
            mockItem.Setup(x => x.FundModel).Returns(fundModel);

            var sut = NewRule();

            var result = sut.HasQualifyingModel(mockItem.Object);

            Assert.Equal(expectation, result);
        }

        [Theory]
        [InlineData(24, ProgTypes.Traineeship)]
        public void TypeOfProgrammeMeetsExpectation(int expectation, int candidate)
        {
            Assert.Equal(expectation, candidate);
        }

        [Theory]
        [InlineData(25, false)]
        [InlineData(24, true)]
        public void IsTraineeshipMeetsExpectation(int? progType, bool expectation)
        {
            var delivery = new Mock<ILearningDelivery>();
            delivery
               .SetupGet(y => y.ProgTypeNullable)
               .Returns(progType);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            var learningDeliveryFAMQS = new Mock<ILearningDeliveryFAMQueryService>(MockBehavior.Strict);

            var fileData = new Mock<IFileDataService>(MockBehavior.Strict);
            fileData
                .Setup(x => x.UKPRN())
                .Returns(TestProviderID);

            var fcsData = new Mock<IFCSDataService>(MockBehavior.Strict);

            var sut = new UKPRN_17Rule(handler.Object, fileData.Object, learningDeliveryFAMQS.Object, fcsData.Object);

            var result = sut.IsTraineeship(delivery.Object);

            Assert.Equal(expectation, result);

            handler.VerifyAll();
            learningDeliveryFAMQS.VerifyAll();
            fileData.VerifyAll();
            fcsData.VerifyAll();
        }

        [Theory]
        [InlineData("SOF105", Monitoring.Delivery.ESFAAdultFunding)]
        public void MonitoringCodeMeetsExpectation(string expectation, string candidate)
        {
            Assert.Equal(expectation, candidate);
        }

        [Theory]
        [InlineData(FundingStreamPeriodCodeConstants.LEVY1799, false)]
        [InlineData(FundingStreamPeriodCodeConstants.NONLEVY2019, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEBC1819, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEBC1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEBC_19TRN1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEBC_ASCL1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEBTO_TOL1819, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEBTO_TOL1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_19TRLS1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_19TRN1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_AS1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_ASLS1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_LS1819, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_LS1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_TOL1819, false)]
        [InlineData(FundingStreamPeriodCodeConstants.AEB_TOL1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.ALLB1819, false)]
        [InlineData(FundingStreamPeriodCodeConstants.ALLB1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.C16_18TRN1920, false)]
        [InlineData(FundingStreamPeriodCodeConstants.C16_18TRN2021, true)]
        public void HasFundingRelationshipMeetsExpectation(string candidate, bool expectation)
        {
            var thresholdDate = DateTime.Parse("2017-05-01");

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            var learningDeliveryFAMQS = new Mock<ILearningDeliveryFAMQueryService>(MockBehavior.Strict);

            var fileData = new Mock<IFileDataService>(MockBehavior.Strict);
            fileData
                .Setup(x => x.UKPRN())
                .Returns(TestProviderID);

            var allocation = new Mock<IFcsContractAllocation>(MockBehavior.Strict);
            allocation
                .SetupGet(x => x.FundingStreamPeriodCode)
                .Returns(candidate);
            allocation
                .SetupGet(x => x.StartDate)
                .Returns(thresholdDate);

            var fcsData = new Mock<IFCSDataService>(MockBehavior.Strict);
            fcsData
                .Setup(x => x.GetContractAllocationsFor(TestProviderID))
                .Returns(new IFcsContractAllocation[] { allocation.Object });

            var sut = new UKPRN_17Rule(handler.Object, fileData.Object, learningDeliveryFAMQS.Object, fcsData.Object);

            var result = sut.HasDisQualifyingFundingRelationship(x => true);

            Assert.Equal(expectation, result);

            handler.VerifyAll();
            learningDeliveryFAMQS.VerifyAll();
            fileData.VerifyAll();
            fcsData.VerifyAll();
        }

        [Theory]
        [InlineData(FundingStreamPeriodCodeConstants.C16_18TRN2021)]
        public void InvalidItemRaisesValidationMessage(string candidate)
        {
            const string LearnRefNumber = "123456789X";
            var thresholdDate = DateTime.Parse("2017-05-01");

            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(y => y.LearnStartDate)
                .Returns(thresholdDate);
            delivery
                .SetupGet(y => y.FundModel)
                .Returns(25);
            delivery
                .SetupGet(y => y.ProgTypeNullable)
                .Returns(24);

            var deliveries = new ILearningDelivery[] { delivery.Object };

            var learner = new Mock<ILearner>();
            learner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);
            learner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            handler
                .Setup(x => x.Handle(RuleNameConstants.UKPRN_17, LearnRefNumber, 0, It.IsAny<IEnumerable<IErrorMessageParameter>>()));
            handler
                .Setup(x => x.BuildErrorMessageParameter("UKPRN", TestProviderID))
                .Returns(new Mock<IErrorMessageParameter>().Object);
            handler
                .Setup(x => x.BuildErrorMessageParameter("FundModel", 25))
                .Returns(new Mock<IErrorMessageParameter>().Object);
            handler
                .Setup(x => x.BuildErrorMessageParameter("ProgType", 24))
                .Returns(new Mock<IErrorMessageParameter>().Object);
            handler
                .Setup(x => x.BuildErrorMessageParameter("LearnStartDate", AbstractRule.AsRequiredCultureDate(thresholdDate)))
                .Returns(new Mock<IErrorMessageParameter>().Object);
            handler
                .Setup(x => x.BuildErrorMessageParameter("LearnDelFAMType", "SOF"))
                .Returns(new Mock<IErrorMessageParameter>().Object);
            handler
                .Setup(x => x.BuildErrorMessageParameter("LearnDelFAMCode", "105"))
                .Returns(new Mock<IErrorMessageParameter>().Object);

            var fileData = new Mock<IFileDataService>(MockBehavior.Strict);
            fileData
                .Setup(x => x.UKPRN())
                .Returns(TestProviderID);

            var allocation = new Mock<IFcsContractAllocation>(MockBehavior.Strict);
            allocation
                .SetupGet(x => x.FundingStreamPeriodCode)
                .Returns(candidate);
            allocation
                .SetupGet(x => x.StopNewStartsFromDate)
                .Returns(thresholdDate);
            allocation
                .SetupGet(x => x.StartDate)
                .Returns(thresholdDate);

            var fcsData = new Mock<IFCSDataService>(MockBehavior.Strict);
            fcsData
                .Setup(x => x.GetContractAllocationsFor(TestProviderID))
                .Returns(new IFcsContractAllocation[] { allocation.Object });

            var learningDeliveryFamqs = new Mock<ILearningDeliveryFAMQueryService>(MockBehavior.Strict);
            learningDeliveryFamqs
               .Setup(x => x.HasLearningDeliveryFAMCodeForType(
                   delivery.Object.LearningDeliveryFAMs,
                   "SOF",
                   "105"))
               .Returns(true);

            learningDeliveryFamqs
                .Setup(x => x.HasLearningDeliveryFAMType(
                    delivery.Object.LearningDeliveryFAMs,
                    "RES"))
                .Returns(false);

            var sut = new UKPRN_17Rule(handler.Object, fileData.Object, learningDeliveryFamqs.Object, fcsData.Object);

            sut.Validate(learner.Object);

            handler.VerifyAll();
            learningDeliveryFamqs.VerifyAll();
            fileData.VerifyAll();
            fcsData.VerifyAll();
        }

        [Fact]
        public void HasNoRestartFamType_Fails()
        {
            IEnumerable<ILearningDeliveryFAM> deliveryFams = new List<TestLearningDeliveryFAM>()
            {
                new TestLearningDeliveryFAM() { LearnDelFAMType = "RES" }
            };

            var mockFamQuerySrvc = new Mock<ILearningDeliveryFAMQueryService>();
            mockFamQuerySrvc.Setup(x => x.HasLearningDeliveryFAMType(deliveryFams, "RES")).Returns(true);

            var result = NewRule(null, null, mockFamQuerySrvc.Object, null).HasNoRestartFamType(deliveryFams);

            result.Should().BeFalse();
            mockFamQuerySrvc.Verify(x => x.HasLearningDeliveryFAMType(deliveryFams, "RES"), Times.Exactly(1));
        }

        [Fact]
        public void HasNoRestartFamType_Pass()
        {
            IEnumerable<ILearningDeliveryFAM> deliveryFams = new List<TestLearningDeliveryFAM>()
            {
                new TestLearningDeliveryFAM() { LearnDelFAMType = "ADL" }
            };

            var mockFamQuerySrvc = new Mock<ILearningDeliveryFAMQueryService>();
            mockFamQuerySrvc.Setup(x => x.HasLearningDeliveryFAMType(deliveryFams, "RES")).Returns(false);

            var result = NewRule(null, null, mockFamQuerySrvc.Object, null).HasNoRestartFamType(deliveryFams);

            result.Should().BeTrue();
            mockFamQuerySrvc.Verify(x => x.HasLearningDeliveryFAMType(deliveryFams, "RES"), Times.Exactly(1));
        }

        [Theory]
        [InlineData(FundingStreamPeriodCodeConstants.C16_18TRN2021)]
        public void ValidItemDoesNotRaiseValidationMessage(string candidate)
        {
            const string LearnRefNumber = "123456789X";
            var thresholdDate = DateTime.Parse("2017-05-01");

            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(y => y.LearnStartDate)
                .Returns(thresholdDate);
            delivery
                .SetupGet(y => y.FundModel)
                .Returns(25);
            delivery
                .SetupGet(y => y.ProgTypeNullable)
                .Returns(24);

            var deliveries = new ILearningDelivery[] { delivery.Object };

            var learner = new Mock<ILearner>();
            learner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);
            learner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            var fileData = new Mock<IFileDataService>(MockBehavior.Strict);
            fileData
                .Setup(x => x.UKPRN())
                .Returns(TestProviderID);

            var allocation = new Mock<IFcsContractAllocation>(MockBehavior.Strict);
            allocation
                .SetupGet(x => x.FundingStreamPeriodCode)
                .Returns(candidate);
            allocation
                .SetupGet(x => x.StopNewStartsFromDate)
                .Returns(thresholdDate.AddDays(1));

            allocation
                .SetupGet(x => x.StartDate)
                .Returns(thresholdDate);

            var fcsData = new Mock<IFCSDataService>(MockBehavior.Strict);
            fcsData
                .Setup(x => x.GetContractAllocationsFor(TestProviderID))
                .Returns(new IFcsContractAllocation[] { allocation.Object });

            var learningDeliveryFamqs = new Mock<ILearningDeliveryFAMQueryService>(MockBehavior.Strict);
            learningDeliveryFamqs
               .Setup(x => x.HasLearningDeliveryFAMCodeForType(
                   delivery.Object.LearningDeliveryFAMs,
                   "SOF",
                   "105"))
               .Returns(true);

            learningDeliveryFamqs
                .Setup(x => x.HasLearningDeliveryFAMType(
                    delivery.Object.LearningDeliveryFAMs,
                    "RES"))
                .Returns(false);

            var sut = new UKPRN_17Rule(handler.Object, fileData.Object, learningDeliveryFamqs.Object, fcsData.Object);

            sut.Validate(learner.Object);

            handler.VerifyAll();
            learningDeliveryFamqs.VerifyAll();
            fileData.VerifyAll();
            fcsData.VerifyAll();
        }

        [Theory]
        [InlineData(FundingStreamPeriodCodeConstants.C16_18TRN2021)]
        public void ValidItemNoPreviousContractAllocationDoesNotRaiseValidationMessage(string candidate)
        {
            const string LearnRefNumber = "123456789X";
            var thresholdDate = DateTime.Parse("2017-05-01");

            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(y => y.LearnStartDate)
                .Returns(thresholdDate);
            delivery
                .SetupGet(y => y.FundModel)
                .Returns(25);
            delivery
                .SetupGet(y => y.ProgTypeNullable)
                .Returns(24);

            var deliveries = new ILearningDelivery[] { delivery.Object };

            var learner = new Mock<ILearner>();
            learner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);
            learner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            var fileData = new Mock<IFileDataService>(MockBehavior.Strict);
            fileData
                .Setup(x => x.UKPRN())
                .Returns(TestProviderID);

            var fcsData = new Mock<IFCSDataService>(MockBehavior.Strict);
            fcsData
                .Setup(x => x.GetContractAllocationsFor(TestProviderID))
                .Returns(new IFcsContractAllocation[] { });

            var learningDeliveryFamqs = new Mock<ILearningDeliveryFAMQueryService>(MockBehavior.Strict);
            learningDeliveryFamqs
               .Setup(x => x.HasLearningDeliveryFAMCodeForType(
                   delivery.Object.LearningDeliveryFAMs,
                   "SOF",
                   "105"))
               .Returns(true);

            learningDeliveryFamqs
                .Setup(x => x.HasLearningDeliveryFAMType(
                    delivery.Object.LearningDeliveryFAMs,
                    "RES"))
                .Returns(false);

            var sut = new UKPRN_17Rule(handler.Object, fileData.Object, learningDeliveryFamqs.Object, fcsData.Object);

            sut.Validate(learner.Object);

            handler.VerifyAll();
            learningDeliveryFamqs.VerifyAll();
            fileData.VerifyAll();
            fcsData.VerifyAll();
        }

        [Theory]
        [InlineData(FundingStreamPeriodCodeConstants.C16_18TRN2021)]
        public void ValidItemMultipleContractsDoesNotRaiseValidationMessage(string candidate)
        {
            const string LearnRefNumber = "123456789X";
            var thresholdDate = DateTime.Parse("2017-05-01");

            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(y => y.LearnStartDate)
                .Returns(thresholdDate);
            delivery
                .SetupGet(y => y.FundModel)
                .Returns(25);
            delivery
                .SetupGet(y => y.ProgTypeNullable)
                .Returns(24);

            var deliveries = new ILearningDelivery[] { delivery.Object };

            var learner = new Mock<ILearner>();
            learner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);
            learner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            var fileData = new Mock<IFileDataService>(MockBehavior.Strict);
            fileData
                .Setup(x => x.UKPRN())
                .Returns(TestProviderID);

            var allocation = new Mock<IFcsContractAllocation>(MockBehavior.Strict);
            allocation
                .SetupGet(x => x.FundingStreamPeriodCode)
                .Returns(candidate);
            allocation
                .SetupGet(x => x.StopNewStartsFromDate)
                .Returns(thresholdDate.AddDays(1));
            allocation
                .SetupGet(x => x.StartDate)
                .Returns(thresholdDate);

            var allocation2 = new Mock<IFcsContractAllocation>(MockBehavior.Strict);
            allocation2
                .SetupGet(x => x.FundingStreamPeriodCode)
                .Returns(candidate);
            allocation2
                .SetupGet(x => x.StopNewStartsFromDate)
                .Returns((DateTime?)null);
            allocation2
                .SetupGet(x => x.StartDate)
                .Returns(thresholdDate.AddDays(1));

            var fcsData = new Mock<IFCSDataService>(MockBehavior.Strict);
            fcsData
                .Setup(x => x.GetContractAllocationsFor(TestProviderID))
                .Returns(new IFcsContractAllocation[] { allocation.Object, allocation2.Object });

            var learningDeliveryFamqs = new Mock<ILearningDeliveryFAMQueryService>(MockBehavior.Strict);
            learningDeliveryFamqs
               .Setup(x => x.HasLearningDeliveryFAMCodeForType(
                   delivery.Object.LearningDeliveryFAMs,
                   "SOF",
                   "105"))
               .Returns(true);

            learningDeliveryFamqs
                .Setup(x => x.HasLearningDeliveryFAMType(
                    delivery.Object.LearningDeliveryFAMs,
                    "RES"))
                .Returns(false);

            var sut = new UKPRN_17Rule(handler.Object, fileData.Object, learningDeliveryFamqs.Object, fcsData.Object);

            sut.Validate(learner.Object);

            handler.VerifyAll();
            learningDeliveryFamqs.VerifyAll();
            fileData.VerifyAll();
            fcsData.VerifyAll();
        }

        public UKPRN_17Rule NewRule(
            IValidationErrorHandler handler = null,
            IFileDataService fileDataService = null,
            ILearningDeliveryFAMQueryService learningDeliveryFamqs = null,
            IFCSDataService fcsDataService = null)
        {
            var fileData = new Mock<IFileDataService>(MockBehavior.Strict);
            fileData
                .Setup(x => x.UKPRN())
                .Returns(TestProviderID);

            return new UKPRN_17Rule(
                handler ?? Mock.Of<IValidationErrorHandler>(),
                fileDataService ?? fileData.Object,
                learningDeliveryFamqs ?? Mock.Of<ILearningDeliveryFAMQueryService>(),
                fcsDataService ?? Mock.Of<IFCSDataService>());
        }
    }
}
