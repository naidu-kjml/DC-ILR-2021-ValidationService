﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.LearnDelFAMDateTo;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.LearnDelFAMDateTo
{
    public class LearnDelFAMDateTo_03RuleTests
    {
        [Fact]
        public void RuleName()
        {
            // arrange
            var sut = NewRule();

            // act
            var result = sut.RuleName;

            // assert
            Assert.Equal("LearnDelFAMDateTo_03", result);
        }

        [Theory]
        [InlineData(35, true)]
        [InlineData(36, true)]
        [InlineData(81, true)]
        [InlineData(99, true)]
        [InlineData(25, false)]
        public void HasQualifyingFundingMeetsExpectation(int fundModel, bool expectation)
        {
            // arrange
            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(y => y.FundModel)
                .Returns(fundModel);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            var sut = new LearnDelFAMDateTo_03Rule(handler.Object);

            // act
            var result = sut.HasQualifyingFunding(delivery.Object);

            // assert
            Assert.Equal(expectation, result);
            handler.VerifyAll();
        }

        [Theory]
        [InlineData("ADL", true)] // Monitoring.Delivery.Types.AdvancedLearnerLoan
        [InlineData("ALB", true)] // Monitoring.Delivery.Types.AdvancedLearnerLoansBursaryFunding
        [InlineData("ACT", false)] // Monitoring.Delivery.Types.ApprenticeshipContract
        [InlineData("ASL", true)] // Monitoring.Delivery.Types.CommunityLearningProvision
        [InlineData("EEF", true)] // Monitoring.Delivery.Types.EligibilityForEnhancedApprenticeshipFunding
        [InlineData("FLN", true)] // Monitoring.Delivery.Types.FamilyEnglishMathsAndLanguage
        [InlineData("FFI", true)] // Monitoring.Delivery.Types.FullOrCoFunding
        [InlineData("HEM", true)] // Monitoring.Delivery.Types.HEMonitoring
        [InlineData("HHS", true)] // Monitoring.Delivery.Types.HouseholdSituation
        [InlineData("LDM", true)] // Monitoring.Delivery.Types.Learning
        [InlineData("LSF", true)] // Monitoring.Delivery.Types.LearningSupportFunding
        [InlineData("NSA", true)] // Monitoring.Delivery.Types.NationalSkillsAcademy
        [InlineData("POD", true)] // Monitoring.Delivery.Types.PercentageOfOnlineDelivery
        [InlineData("RES", true)] // Monitoring.Delivery.Types.Restart
        [InlineData("SOF", true)] // Monitoring.Delivery.Types.SourceOfFunding
        [InlineData("WPP", true)] // Monitoring.Delivery.Types.WorkProgrammeParticipation
        public void IsQualifyingMonitorMeetsExpectation(string candidate, bool expectation)
        {
            // arrange
            var fam = new Mock<ILearningDeliveryFAM>();
            fam
                .SetupGet(x => x.LearnDelFAMType)
                .Returns(candidate);

            var sut = NewRule();

            // act
            var result = sut.IsQualifyingMonitor(fam.Object);

            // assert
            Assert.Equal(expectation, result);
        }

        [Theory]
        [InlineData("2017-12-30", null, false)]
        [InlineData(null, "2017-12-31", false)]
        [InlineData("2017-12-30", "2017-12-31", false)]
        [InlineData("2017-12-31", "2017-12-31", false)]
        [InlineData("2018-01-01", "2017-12-31", true)]
        public void HasDisqualifyingDatesMeetsExpectation(string dateTo, string actEnd, bool expectation)
        {
            // arrange
            var fam = new Mock<ILearningDeliveryFAM>();
            fam
                .SetupGet(x => x.LearnDelFAMDateToNullable)
                .Returns(GetNullableDate(dateTo));

            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(x => x.LearnActEndDateNullable)
                .Returns(GetNullableDate(actEnd));

            var sut = NewRule();

            // act
            var result = sut.HasDisqualifyingDates(delivery.Object, fam.Object);

            // assert
            Assert.Equal(expectation, result);
        }

        public DateTime? GetNullableDate(string candidate) =>
             DateTime.TryParse(candidate, out var result) ? result : (DateTime?)null;

        [Theory]
        [InlineData("ADL", 1)] // Monitoring.Delivery.Types.AdvancedLearnerLoan
        [InlineData("ALB", 1)] // Monitoring.Delivery.Types.AdvancedLearnerLoansBursaryFunding
        [InlineData("ASL", 1)] // Monitoring.Delivery.Types.CommunityLearningProvision
        [InlineData("EEF", 1)] // Monitoring.Delivery.Types.EligibilityForEnhancedApprenticeshipFunding
        [InlineData("FLN", 1)] // Monitoring.Delivery.Types.FamilyEnglishMathsAndLanguage
        [InlineData("FFI", 1)] // Monitoring.Delivery.Types.FullOrCoFunding
        [InlineData("HEM", 1)] // Monitoring.Delivery.Types.HEMonitoring
        [InlineData("HHS", 1)] // Monitoring.Delivery.Types.HouseholdSituation
        [InlineData("LDM", 1)] // Monitoring.Delivery.Types.Learning
        [InlineData("LSF", 1)] // Monitoring.Delivery.Types.LearningSupportFunding
        [InlineData("NSA", 1)] // Monitoring.Delivery.Types.NationalSkillsAcademy
        [InlineData("POD", 1)] // Monitoring.Delivery.Types.PercentageOfOnlineDelivery
        [InlineData("RES", 1)] // Monitoring.Delivery.Types.Restart
        [InlineData("SOF", 1)] // Monitoring.Delivery.Types.SourceOfFunding
        [InlineData("WPP", 1)] // Monitoring.Delivery.Types.WorkProgrammeParticipation
        public void InvalidItemRaisesValidationMessage(string famType, int dateOffset)
        {
            // arrange
            const string learnRefNumber = "123456789X";

            var testDate = DateTime.Parse("2017-12-30");

            var fam = new Mock<ILearningDeliveryFAM>();
            fam
                .SetupGet(x => x.LearnDelFAMType)
                .Returns(famType);
            fam
                .SetupGet(x => x.LearnDelFAMDateToNullable)
                .Returns(testDate.AddDays(dateOffset));

            var fams = new ILearningDeliveryFAM[] { fam.Object };

            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(x => x.LearnActEndDateNullable)
                .Returns(testDate);
            delivery
                .SetupGet(x => x.FundModel)
                .Returns(35); // TypeOfFunding.AdultSkills
            delivery
                .SetupGet(x => x.LearningDeliveryFAMs)
                .Returns(fams);

            var deliveries = new ILearningDelivery[] { delivery.Object };

            var learner = new Mock<ILearner>();
            learner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(learnRefNumber);
            learner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            handler
                .Setup(x => x.Handle(RuleNameConstants.LearnDelFAMDateTo_03, learnRefNumber, 0, It.IsAny<IEnumerable<IErrorMessageParameter>>()));
            handler
                .Setup(x => x.BuildErrorMessageParameter("LearnActEndDate", AbstractRule.AsRequiredCultureDate(testDate)))
                .Returns(new Mock<IErrorMessageParameter>().Object);
            handler
                .Setup(x => x.BuildErrorMessageParameter("LearnDelFAMDateTo", AbstractRule.AsRequiredCultureDate(testDate.AddDays(dateOffset))))
                .Returns(new Mock<IErrorMessageParameter>().Object);

            var sut = new LearnDelFAMDateTo_03Rule(handler.Object);

            // act
            sut.Validate(learner.Object);

            // assert
            handler.VerifyAll();
        }

        [Theory]
        [InlineData("ACT", 1)] // Monitoring.Delivery.Types.ApprenticeshipContract
        [InlineData("ADL", 0)] // Monitoring.Delivery.Types.AdvancedLearnerLoan
        [InlineData("ALB", 0)] // Monitoring.Delivery.Types.AdvancedLearnerLoansBursaryFunding
        [InlineData("ASL", 0)] // Monitoring.Delivery.Types.CommunityLearningProvision
        [InlineData("EEF", 0)] // Monitoring.Delivery.Types.EligibilityForEnhancedApprenticeshipFunding
        [InlineData("FLN", 0)] // Monitoring.Delivery.Types.FamilyEnglishMathsAndLanguage
        [InlineData("FFI", 0)] // Monitoring.Delivery.Types.FullOrCoFunding
        [InlineData("HEM", 0)] // Monitoring.Delivery.Types.HEMonitoring
        [InlineData("HHS", 0)] // Monitoring.Delivery.Types.HouseholdSituation
        [InlineData("LDM", 0)] // Monitoring.Delivery.Types.Learning
        [InlineData("LSF", 0)] // Monitoring.Delivery.Types.LearningSupportFunding
        [InlineData("NSA", 0)] // Monitoring.Delivery.Types.NationalSkillsAcademy
        [InlineData("POD", 0)] // Monitoring.Delivery.Types.PercentageOfOnlineDelivery
        [InlineData("RES", 0)] // Monitoring.Delivery.Types.Restart
        [InlineData("SOF", 0)] // Monitoring.Delivery.Types.SourceOfFunding
        [InlineData("WPP", 0)] // Monitoring.Delivery.Types.WorkProgrammeParticipation
        public void ValidItemDoesNotRaiseValidationMessage(string famType, int dateOffset)
        {
            // arrange
            const string learnRefNumber = "123456789X";

            var testDate = DateTime.Parse("2017-12-30");

            var fam = new Mock<ILearningDeliveryFAM>();
            fam
                .SetupGet(x => x.LearnDelFAMType)
                .Returns(famType);
            fam
                .SetupGet(x => x.LearnDelFAMDateToNullable)
                .Returns(testDate.AddDays(dateOffset));

            var fams = new ILearningDeliveryFAM[] { fam.Object };

            var delivery = new Mock<ILearningDelivery>();
            delivery
                .SetupGet(x => x.LearnActEndDateNullable)
                .Returns(testDate);
            delivery
                .SetupGet(x => x.FundModel)
                .Returns(35); // TypeOfFunding.AdultSkills
            delivery
                .SetupGet(x => x.LearningDeliveryFAMs)
                .Returns(fams);

            var deliveries = new ILearningDelivery[] { delivery.Object };

            var learner = new Mock<ILearner>();
            learner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(learnRefNumber);
            learner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            var sut = new LearnDelFAMDateTo_03Rule(handler.Object);

            // act
            sut.Validate(learner.Object);

            // assert
            handler.VerifyAll();
        }

        public LearnDelFAMDateTo_03Rule NewRule()
        {
            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            return new LearnDelFAMDateTo_03Rule(handler.Object);
        }
    }
}
