﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Derived;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.Derived
{
    public class DerivedData_21RuleTests
    {
        [Theory]
        [InlineData("LDM", "318", 11, "BSI", 3)]
        [InlineData("LDM", "318", 12, "BSI", 3)]
        [InlineData("LDM", "034", 11, "BSI", 4)]
        [InlineData("LDM", "034", 12, "BSI", 4)]
        [InlineData("ALD", "034", 11, "BSI", 4)]
        [InlineData("ALD", "034", 12, "BSI", 4)]
        [InlineData("ALD", "318", 11, "BSI", 4)]
        [InlineData("ALD", "318", 12, "BSI", 4)]
        public void IsAdultFundedUnemployedWithOtherStateBenefits_True(
            string learnDelFAMType,
            string learnDelFAMCode,
            int empStatus,
            string eSMType,
            int eSMCode)
        {
            DateTime learnStartDate = new DateTime(2016, 08, 01);

            var learninDelivery = new TestLearningDelivery()
            {
                AimSeqNumber = 1001,
                CompStatus = 1,
                FundModel = 35,
                LearnStartDate = learnStartDate,
                LearningDeliveryFAMs = new TestLearningDeliveryFAM[]
                {
                    new TestLearningDeliveryFAM()
                    {
                         LearnDelFAMType = learnDelFAMType,
                         LearnDelFAMCode = learnDelFAMCode
                    }
                }
            };

            var testLearnerEmploymentStatus = new TestLearnerEmploymentStatus()
            {
                EmpStat = empStatus,
                EmploymentStatusMonitorings = new TestEmploymentStatusMonitoring[]
                        {
                            new TestEmploymentStatusMonitoring()
                            {
                                ESMType = eSMType,
                                ESMCode = eSMCode
                            }
                        }
            };

            var testLearner = new TestLearner()
            {
                LearnRefNumber = "1A001",
                LearnerEmploymentStatuses = new TestLearnerEmploymentStatus[]
                {
                    testLearnerEmploymentStatus
                },
                LearningDeliveries = new TestLearningDelivery[]
                {
                    learninDelivery
                }
            };

            var lEmpQS = new Mock<ILearnerEmploymentStatusQueryService>();
            lEmpQS.Setup(x => x.LearnerEmploymentStatusForDate(testLearner.LearnerEmploymentStatuses, learnStartDate)).Returns(testLearnerEmploymentStatus);

            NewRule(lEmpQS.Object).IsAdultFundedUnemployedWithOtherStateBenefits(learninDelivery, testLearner).Should().BeTrue();
        }

        [Theory]
        [InlineData(35, true, "LDM", "318", 11, "BSI", 4)]
        [InlineData(35, true, "LDM", "318", 13, "BSI", 3)]
        [InlineData(35, true, "LDM", "318", 13, "LOE", 3)]
        [InlineData(35, true, "LDM", "034", 12, "BSI", 5)]
        [InlineData(70, false, "LDM", "318", 11, "BSI", 3)]
        [InlineData(25, false, "LDM", "318", 12, "BSI", 3)]
        [InlineData(10, false, "LDM", "034", 11, "BSI", 4)]
        [InlineData(36, false, "LDM", "034", 12, "BSI", 4)]
        public void IsAdultFundedUnemployedWithOtherStateBenefits_False(
            int fundModel,
            bool fundModelExpecedResult,
            string learnDelFAMType,
            string learnDelFAMCode,
            int empStatus,
            string eSMType,
            int eSMCode)
        {
            DateTime learnStartDate = new DateTime(2016, 08, 01);

            var learningDelivery = new TestLearningDelivery()
            {
                AimSeqNumber = 1001,
                CompStatus = 1,
                FundModel = fundModel,
                LearnStartDate = learnStartDate,
                LearningDeliveryFAMs = new TestLearningDeliveryFAM[]
                {
                    new TestLearningDeliveryFAM()
                    {
                         LearnDelFAMType = learnDelFAMType,
                         LearnDelFAMCode = learnDelFAMCode
                    }
                }
            };

            var testLearnerEmploymentStatus = new TestLearnerEmploymentStatus()
            {
                EmpStat = empStatus,
                EmploymentStatusMonitorings = new TestEmploymentStatusMonitoring[]
                {
                    new TestEmploymentStatusMonitoring()
                    {
                        ESMType = eSMType,
                        ESMCode = eSMCode
                    }
                }
            };

            var testLearner = new TestLearner()
            {
                LearnRefNumber = "1A001",
                LearnerEmploymentStatuses = new TestLearnerEmploymentStatus[]
                {
                    testLearnerEmploymentStatus
                },
                LearningDeliveries = new TestLearningDelivery[]
                {
                    learningDelivery
                }
            };

            var lEmpQS = new Mock<ILearnerEmploymentStatusQueryService>();
            lEmpQS.Setup(x => x.LearnerEmploymentStatusForDate(testLearner.LearnerEmploymentStatuses, learnStartDate)).Returns(testLearnerEmploymentStatus);

            NewRule(lEmpQS.Object).IsAdultFundedUnemployedWithOtherStateBenefits(learningDelivery, testLearner).Should().BeFalse();
        }

        [Theory]
        [InlineData(EmploymentStatusEmpStats.NotEmployedNotSeekingOrNotAvailable, true)]
        [InlineData(EmploymentStatusEmpStats.InPaidEmployment, false)]
        [InlineData(EmploymentStatusEmpStats.NotEmployedSeekingAndAvailable, true)]
        [InlineData(EmploymentStatusEmpStats.NotKnownProvided, false)]
        public void IsNotEmployedMeetsExpectation(int candidate, bool expectation)
        {
            var sut = NewRule();
            var mockDelivery = new Mock<ILearnerEmploymentStatus>();
            mockDelivery
                .SetupGet(y => y.EmpStat)
                .Returns(candidate);

            var result = sut.IsNotEmployed(mockDelivery.Object);

            Assert.Equal(expectation, result);
        }

        [Theory]
        [InlineData(Monitoring.EmploymentStatus.EmployedForLessThan16HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor16HoursOrMorePW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor16To19HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor20HoursOrMorePW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor0To10HourPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor11To20HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor21To30HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor31PlusHoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedForUpTo3M, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor4To6M, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor7To12M, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedForMoreThan12M, false)]
        [InlineData(Monitoring.EmploymentStatus.InFulltimeEducationOrTrainingPriorToEnrolment, false)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfAnotherStateBenefit, true)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfEmploymentAndSupportAllowance, false)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfJobSeekersAllowance, false)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfUniversalCredit, false)]
        [InlineData(Monitoring.EmploymentStatus.SelfEmployed, false)]
        [InlineData(Monitoring.EmploymentStatus.SmallEmployer, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedForLessThan6M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor6To11M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor12To23M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor24To35M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor36MPlus, false)]
        public void InReceiptOfAnotherBenefitMeetsExpectation(string candidate, bool expectation)
        {
            var sut = NewRule();
            var mockItem = new Mock<IEmploymentStatusMonitoring>(MockBehavior.Strict);
            mockItem
                .SetupGet(y => y.ESMType)
                .Returns(candidate.Substring(0, 3));
            mockItem
                .SetupGet(y => y.ESMCode)
                .Returns(int.Parse(candidate.Substring(3)));

            var result = sut.InReceiptOfAnotherBenefit(mockItem.Object);

            Assert.Equal(expectation, result);
            mockItem.VerifyAll();
        }

        [Theory]
        [InlineData(Monitoring.EmploymentStatus.EmployedForLessThan16HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor16HoursOrMorePW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor16To19HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor20HoursOrMorePW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor0To10HourPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor11To20HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor21To30HoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor31PlusHoursPW, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedForUpTo3M, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor4To6M, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedFor7To12M, false)]
        [InlineData(Monitoring.EmploymentStatus.EmployedForMoreThan12M, false)]
        [InlineData(Monitoring.EmploymentStatus.InFulltimeEducationOrTrainingPriorToEnrolment, false)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfAnotherStateBenefit, false)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfEmploymentAndSupportAllowance, false)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfJobSeekersAllowance, false)]
        [InlineData(Monitoring.EmploymentStatus.InReceiptOfUniversalCredit, true)]
        [InlineData(Monitoring.EmploymentStatus.SelfEmployed, false)]
        [InlineData(Monitoring.EmploymentStatus.SmallEmployer, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedForLessThan6M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor6To11M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor12To23M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor24To35M, false)]
        [InlineData(Monitoring.EmploymentStatus.UnemployedFor36MPlus, false)]
        public void InReceiptOfUniversalCreditMeetsExpectation(string candidate, bool expectation)
        {
            var sut = NewRule();
            var mockItem = new Mock<IEmploymentStatusMonitoring>(MockBehavior.Strict);
            mockItem
                .SetupGet(y => y.ESMType)
                .Returns(candidate.Substring(0, 3));
            mockItem
                .SetupGet(y => y.ESMCode)
                .Returns(int.Parse(candidate.Substring(3)));

            var result = sut.InReceiptOfUniversalCredit(mockItem.Object);

            Assert.Equal(expectation, result);
            mockItem.VerifyAll();
        }

        [Theory]
        [InlineData(Monitoring.Delivery.Types.AdvancedLearnerLoansBursaryFunding, true)]
        [InlineData(Monitoring.Delivery.Types.AdvancedLearnerLoan, true)]
        [InlineData(Monitoring.Delivery.Types.ApprenticeshipContract, true)]
        [InlineData(Monitoring.Delivery.Types.CommunityLearningProvision, true)]
        [InlineData(Monitoring.Delivery.Types.EligibilityForEnhancedApprenticeshipFunding, true)]
        [InlineData(Monitoring.Delivery.Types.FamilyEnglishMathsAndLanguage, true)]
        [InlineData(Monitoring.Delivery.Types.FullOrCoFunding, true)]
        [InlineData(Monitoring.Delivery.Types.HEMonitoring, true)]
        [InlineData(Monitoring.Delivery.Types.HouseholdSituation, true)]
        [InlineData(Monitoring.Delivery.Types.Learning, false)]
        [InlineData(Monitoring.Delivery.Types.LearningSupportFunding, true)]
        [InlineData(Monitoring.Delivery.Types.NationalSkillsAcademy, true)]
        [InlineData(Monitoring.Delivery.Types.PercentageOfOnlineDelivery, true)]
        [InlineData(Monitoring.Delivery.Types.Restart, true)]
        [InlineData(Monitoring.Delivery.Types.SourceOfFunding, true)]
        [InlineData(Monitoring.Delivery.Types.WorkProgrammeParticipation, true)]
        public void IsMonitoredMeetsExpectation(string candidate, bool expectation)
        {
            var sut = NewRule();
            var mockItem = new Mock<ILearningDeliveryFAM>(MockBehavior.Strict);
            mockItem
                .SetupGet(y => y.LearnDelFAMType)
                .Returns(candidate);

            var result = sut.NotIsMonitored(mockItem.Object);

            Assert.Equal(expectation, result);
            mockItem.VerifyAll();
        }

        [Theory]
        [InlineData(Monitoring.Delivery.InReceiptOfLowWages, false)]
        [InlineData(Monitoring.Delivery.MandationToSkillsTraining, true)]
        [InlineData(Monitoring.Delivery.SteelIndustriesRedundancyTraining, false)]
        public void MandatedToSkillsTrainingMeetsExpectation(string candidate, bool expectation)
        {
            var sut = NewRule();
            var mockItem = new Mock<ILearningDeliveryFAM>(MockBehavior.Strict);
            mockItem
                .SetupGet(y => y.LearnDelFAMType)
                .Returns(candidate.Substring(0, 3));
            mockItem
                .SetupGet(y => y.LearnDelFAMCode)
                .Returns(candidate.Substring(3));

            var result = sut.MandatedToSkillsTraining(mockItem.Object);

            Assert.Equal(expectation, result);
            mockItem.VerifyAll();
        }

        public DerivedData_21Rule NewRule(ILearnerEmploymentStatusQueryService lEmpQS = null)
        {
            return new DerivedData_21Rule(lEmpQS ?? Mock.Of<ILearnerEmploymentStatusQueryService>());
        }
    }
}
