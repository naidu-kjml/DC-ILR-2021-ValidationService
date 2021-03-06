﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Extensions;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.CrossEntity;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.CrossEntity
{
    public class R107RuleTests
    {
        [Fact]
        public void RuleName()
        {
            var sut = NewRule();

            var result = sut.RuleName;

            Assert.Equal("R107", result);
        }

        [Theory]
        [InlineData("2012-07-31", "2012-01-30", "2012-02-28", "2012-07-31", "2012-03-30", "2012-04-30")]
        [InlineData("2013-03-30", "2012-01-30", "2012-07-31", "2013-03-30", "2012-04-30")]
        [InlineData("2014-07-31", "2012-01-30", "2012-02-28", "2012-07-31", "2014-07-31", "2012-04-30")]
        [InlineData("2015-07-31", "2015-07-31", "2012-02-28", "2012-07-31", "2012-04-30")]
        [InlineData("2016-07-31", "2012-02-28", "2016-07-31", "2012-04-30")]
        public void GetLastDeliveryReturnsMeetsExpectation(string expectedDate, params string[] testDates)
        {
            var testDate = DateTime.Parse(expectedDate);
            var sut = NewRule();

            var deliveries = new List<ILearningDelivery>();
            testDates.ForEach(x =>
            {
                var mockDel = new Mock<ILearningDelivery>();
                mockDel
                    .SetupGet(y => y.LearnActEndDateNullable)
                    .Returns(DateTime.Parse(x));

                deliveries.Add(mockDel.Object);
            });

            var mockItem = new Mock<ILearner>();
            mockItem
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var result = sut.GetLastDelivery(mockItem.Object);

            Assert.Equal(testDate, result.LearnActEndDateNullable);
        }

        [Fact]
        public void GetLastDeliveryWithNullDeliveriesMeetsExpectation()
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearner>();

            var result = sut.GetLastDelivery(mockItem.Object);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("sldfkajwefo asjf", 3)]
        [InlineData("alwerkasvf as", 2)]
        [InlineData("zxc,vmnsdlih", 5)]
        [InlineData(",samvnasorgdhkn", 1)]
        public void GetDAndPMeetsExpectation(string learnRN, int candidateCount)
        {
            var outcomes = new List<IDPOutcome>();
            for (int i = 0; i < candidateCount; i++)
            {
                outcomes.Add(new Mock<IDPOutcome>().Object);
            }

            var mockItem = new Mock<ILearnerDestinationAndProgression>();
            mockItem
                .SetupGet(x => x.LearnRefNumber)
                .Returns(learnRN);
            mockItem
                .SetupGet(x => x.DPOutcomes)
                .Returns(outcomes);

            var collection = new List<ILearnerDestinationAndProgression>();
            collection.Add(mockItem.Object);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            var message = new Mock<IMessage>(MockBehavior.Strict);

            message
                .SetupGet(x => x.LearnerDestinationAndProgressions)
                .Returns(collection);

            var sut = NewRule();

            var result = sut.GetDAndP(learnRN, message.Object);

            message.VerifyAll();
            Assert.Equal(candidateCount, result.DPOutcomes.Count);
        }

        [Theory]
        [InlineData("sldfkajwefo asjf")]
        [InlineData("alwerkasvf as")]
        [InlineData("zxc,vmnsdlih")]
        [InlineData(",samvnasorgdhkn")]
        public void GetDAndPWithNullDAndPsMeetsExpectation(string learnRN)
        {
            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            var message = new Mock<IMessage>(MockBehavior.Strict);
            message
                .SetupGet(x => x.LearnerDestinationAndProgressions)
                .Returns((IReadOnlyCollection<ILearnerDestinationAndProgression>)null);

            var sut = NewRule();

            var result = sut.GetDAndP(learnRN, message.Object);

            message.VerifyAll();
            Assert.Null(result);
        }

        [Theory]
        [InlineData("2012-08-31", "2012-07-30", false)]
        [InlineData("2012-07-31", "2012-07-30", false)]
        [InlineData("2012-01-30", "2012-01-30", true)]
        [InlineData("2012-07-29", "2012-07-30", true)]
        public void HasQualifyingOutcomeMeetsExpectation(string aEndDate, string oStartDate, bool expectation)
        {
            var testDate = DateTime.Parse(aEndDate);
            var sut = NewRule();

            var mockItem = new Mock<IDPOutcome>();
            mockItem
                .SetupGet(x => x.OutStartDate)
                .Returns(DateTime.Parse(oStartDate));

            var result = sut.HasQualifyingOutcome(mockItem.Object, testDate);

            Assert.Equal(expectation, result);
        }

        [Theory]
        [InlineData(FundModels.AdultSkills, true)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, true)]
        [InlineData(FundModels.ApprenticeshipsFrom1May2017, false)]
        [InlineData(FundModels.CommunityLearning, false)]
        [InlineData(FundModels.EuropeanSocialFund, true)]
        [InlineData(FundModels.NotFundedByESFA, false)]
        [InlineData(FundModels.Other16To19, false)]
        [InlineData(FundModels.OtherAdult, true)]
        public void HasQualifyingFundModelMeetsExpectation(int candidate, bool expectation)
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearningDelivery>();
            mockItem
                .SetupGet(x => x.FundModel)
                .Returns(candidate);

            var result = sut.HasQualifyingFundModel(mockItem.Object);

            Assert.Equal(expectation, result);
        }

        [Fact]
        public void HasQualifyingFundModelWithNullDeliveriesReturnsFalse()
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearner>();

            var result = sut.HasQualifyingFundModel(mockItem.Object);

            Assert.False(result);
        }

        [Fact]
        public void HasQualifyingFundModelWithEmptyDeliveriesReturnsFalse()
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearner>();
            mockItem
                .SetupGet(x => x.LearningDeliveries)
                .Returns(new List<ILearningDelivery>());

            var result = sut.HasQualifyingFundModel(mockItem.Object);

            Assert.False(result);
        }

        [Theory]
        [InlineData(CompletionState.HasCompleted, false)]
        [InlineData(CompletionState.HasTemporarilyWithdrawn, true)]
        [InlineData(CompletionState.HasWithdrawn, false)]
        [InlineData(CompletionState.IsOngoing, false)]
        [InlineData(4, false)]
        [InlineData(5, false)]
        [InlineData(7, false)]
        public void HasTemporarilyWithdrawnMeetsExpectation(int candidate, bool expectation)
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearningDelivery>();
            mockItem
                .SetupGet(x => x.CompStatus)
                .Returns(candidate);

            var result = sut.HasTemporarilyWithdrawn(mockItem.Object);

            Assert.Equal(expectation, result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("2013-03-17", true)]
        public void HasCompletedCourseMeetsExpectation(string candidate, bool expectation)
        {
            var sut = NewRule();

            var testDate = !string.IsNullOrWhiteSpace(candidate)
                ? DateTime.Parse(candidate)
                : (DateTime?)null;

            var mockItem = new Mock<ILearningDelivery>();
            mockItem
                .SetupGet(x => x.LearnActEndDateNullable)
                .Returns(testDate);

            var result = sut.HasCompletedCourse(mockItem.Object);

            Assert.Equal(expectation, result);
        }

        [Fact]
        public void HasCompletedCourseWithNullDeliveriesReturnsFalse()
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearner>();

            var result = sut.HasCompletedCourse(mockItem.Object);

            Assert.False(result);
        }

        [Fact]
        public void HasCompletedCourseWithEmptyDeliveriesReturnsFalse()
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearner>();
            mockItem
                .SetupGet(x => x.LearningDeliveries)
                .Returns(new List<ILearningDelivery>());

            var result = sut.HasCompletedCourse(mockItem.Object);

            Assert.False(result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(ProgTypes.AdvancedLevelApprenticeship, false)]
        [InlineData(ProgTypes.ApprenticeshipStandard, true)]
        [InlineData(ProgTypes.HigherApprenticeshipLevel4, false)]
        [InlineData(ProgTypes.HigherApprenticeshipLevel5, false)]
        [InlineData(ProgTypes.HigherApprenticeshipLevel6, false)]
        [InlineData(ProgTypes.HigherApprenticeshipLevel7Plus, false)]
        [InlineData(ProgTypes.IntermediateLevelApprenticeship, false)]
        [InlineData(ProgTypes.Traineeship, true)]
        public void InTrainingMeetsExpectation(int? candidate, bool expectation)
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearningDelivery>();
            mockItem
                .SetupGet(x => x.ProgTypeNullable)
                .Returns(candidate);

            var result = sut.InTraining(mockItem.Object);

            Assert.Equal(expectation, result);
        }

        [Fact]
        public void InTrainingWithNullDeliveriesReturnsFalse()
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearner>();

            var result = sut.InTraining(mockItem.Object);

            Assert.False(result);
        }

        [Fact]
        public void InTrainingWithEmptyDeliveriesReturnsFalse()
        {
            var sut = NewRule();

            var mockItem = new Mock<ILearner>();
            mockItem
                .SetupGet(x => x.LearningDeliveries)
                .Returns(new List<ILearningDelivery>());

            var result = sut.InTraining(mockItem.Object);

            Assert.False(result);
        }

        [Theory]
        [InlineData(FundModels.AdultSkills, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        public void InvalidItemRaisesValidationMessage(int fundModel, int progType, int completionState)
        {
            const string LearnRefNumber = "123456789X";

            var mockDel = new Mock<ILearningDelivery>();
            mockDel
                .SetupGet(x => x.FundModel)
                .Returns(fundModel);
            mockDel
                .SetupGet(x => x.CompStatus)
                .Returns(completionState);
            mockDel
                .SetupGet(y => y.LearnActEndDateNullable)
                .Returns(DateTime.Parse("2013-04-01"));
            mockDel
                .SetupGet(x => x.ProgTypeNullable)
                .Returns(progType);

            var deliveries = new List<ILearningDelivery>();
            deliveries.Add(mockDel.Object);

            var mockItem = new Mock<ILearner>();
            mockItem
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);
            mockItem
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var learners = new List<ILearner>();
            learners.Add(mockItem.Object);

            var outcome = new Mock<IDPOutcome>();
            outcome
                .SetupGet(x => x.OutStartDate)
                .Returns(DateTime.Parse("2013-03-30"));

            var outcomes = new List<IDPOutcome>();
            outcomes.Add(outcome.Object);

            var mockDAndP = new Mock<ILearnerDestinationAndProgression>();
            mockDAndP
                .SetupGet(x => x.DPOutcomes)
                .Returns(outcomes);

            var destinationProgressions = new List<ILearnerDestinationAndProgression>();
            destinationProgressions.Add(mockDAndP.Object);

            var message = new Mock<IMessage>(MockBehavior.Strict);
            message
                .SetupGet(x => x.Learners)
                .Returns(learners);
            message
                .SetupGet(x => x.LearnerDestinationAndProgressions)
                .Returns(destinationProgressions);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            handler
                .Setup(x => x.Handle(
                    Moq.It.Is<string>(y => y == R107Rule.Name),
                    Moq.It.Is<string>(y => y == LearnRefNumber),
                    null,
                    null));

            var sut = new R107Rule(handler.Object);

            sut.Validate(message.Object);

            handler.VerifyAll();
            message.VerifyAll();
        }

        [Theory]
        [InlineData(FundModels.AdultSkills, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.OtherAdult, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasCompleted)]
        [InlineData(FundModels.AdultSkills, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.AdvancedLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel4, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel5, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel6, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.HigherApprenticeshipLevel7Plus, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.AdultSkills, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.Age16To19ExcludingApprenticeships, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.EuropeanSocialFund, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        [InlineData(FundModels.OtherAdult, ProgTypes.IntermediateLevelApprenticeship, CompletionState.HasWithdrawn)]
        public void ValidItemDoesNotRaiseValidationMessage(int fundModel, int progType, int completionState)
        {
            const string LearnRefNumber = "123456789X";

            var mockDel = new Mock<ILearningDelivery>();
            mockDel
                .SetupGet(x => x.FundModel)
                .Returns(fundModel);
            mockDel
                .SetupGet(x => x.CompStatus)
                .Returns(completionState);
            mockDel
                .SetupGet(y => y.LearnActEndDateNullable)
                .Returns(DateTime.Parse("2013-04-01"));
            mockDel
                .SetupGet(x => x.ProgTypeNullable)
                .Returns(progType);

            var deliveries = new List<ILearningDelivery>();
            deliveries.Add(mockDel.Object);

            var mockItem = new Mock<ILearner>();
            mockItem
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);
            mockItem
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries);

            var learners = new List<ILearner>();
            learners.Add(mockItem.Object);

            var outcome = new Mock<IDPOutcome>();
            outcome
                .SetupGet(x => x.OutStartDate)
                .Returns(DateTime.Parse("2013-04-02"));

            var outcomes = new List<IDPOutcome>();
            outcomes.Add(outcome.Object);

            var mockDAndP = new Mock<ILearnerDestinationAndProgression>();
            mockDAndP
                .SetupGet(x => x.DPOutcomes)
                .Returns(outcomes);

            var destinationProgressions = new List<ILearnerDestinationAndProgression>();
            destinationProgressions.Add(mockDAndP.Object);

            var message = new Mock<IMessage>(MockBehavior.Strict);
            message
                .SetupGet(x => x.Learners)
                .Returns(learners);
            message
                .SetupGet(x => x.LearnerDestinationAndProgressions)
                .Returns(destinationProgressions);

            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            handler
                .Setup(x => x.Handle(
                    Moq.It.Is<string>(y => y == R107Rule.Name),
                    Moq.It.Is<string>(y => y == LearnRefNumber),
                    null,
                    null));

            var sut = new R107Rule(handler.Object);

            sut.Validate(message.Object);

            handler.VerifyAll();
            message.VerifyAll();
        }

        public R107Rule NewRule()
        {
            var handler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            return new R107Rule(handler.Object);
        }
    }
}
