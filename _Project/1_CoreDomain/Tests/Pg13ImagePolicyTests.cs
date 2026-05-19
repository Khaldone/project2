// Assets/_Project/1_CoreDomain/Tests/Pg13ImagePolicyTests.cs
using Billiards.CoreDomain.Services;
using NUnit.Framework;

namespace Billiards.Core.Tests
{
    public class Pg13ImagePolicyTests
    {
        private static ImageCategoryScores AllZero =>
            new ImageCategoryScores(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        [Test]
        public void Evaluate_AllZeroScoresNotFlagged_ReturnsAllowed()
        {
            var result = Pg13ImagePolicy.Evaluate(AllZero, providerFlagged: false);

            Assert.IsTrue(result.IsAllowed);
            Assert.IsFalse(result.DidError);
            Assert.AreEqual(0f, result.HighestScore);
        }

        [Test]
        public void Evaluate_ProviderFlagged_ReturnsRejectedEvenWithZeroScores()
        {
            var result = Pg13ImagePolicy.Evaluate(AllZero, providerFlagged: true);

            Assert.IsFalse(result.IsAllowed);
            Assert.AreEqual("provider-flagged", result.OffendingCategory);
        }

        [Test]
        public void Evaluate_SexualMinorsAnyNonzeroScore_ReturnsRejected()
        {
            // Zero-tolerance category — even 0.001 must trip the gate.
            var scores = new ImageCategoryScores(0f, sexualMinors: 0.001f, 0f, 0f, 0f, 0f, 0f, 0f);

            var result = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);

            Assert.IsFalse(result.IsAllowed);
            Assert.AreEqual("sexual/minors", result.OffendingCategory);
        }

        [Test]
        public void Evaluate_SexualAtThreshold_ReturnsAllowed_StrictlyGreaterIsRejection()
        {
            var scores = new ImageCategoryScores(sexual: Pg13ImagePolicy.SexualThreshold, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

            var result = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);

            Assert.IsTrue(result.IsAllowed, "Score exactly at threshold should not reject.");
        }

        [Test]
        public void Evaluate_SexualJustAboveThreshold_ReturnsRejected()
        {
            var scores = new ImageCategoryScores(sexual: Pg13ImagePolicy.SexualThreshold + 0.001f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

            var result = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);

            Assert.IsFalse(result.IsAllowed);
            Assert.AreEqual("sexual", result.OffendingCategory);
        }

        [Test]
        public void Evaluate_SelfHarmAboveThreshold_ReturnsRejected()
        {
            var scores = new ImageCategoryScores(0f, 0f, 0f, 0f, 0f, 0f, 0f, selfHarm: 0.03f);

            var result = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);

            Assert.IsFalse(result.IsAllowed);
            Assert.AreEqual("self-harm", result.OffendingCategory);
        }

        [Test]
        public void Evaluate_MultipleCategoriesViolated_ReportsHighestSeverityCategoryFirst()
        {
            // sexual/minors is the most severe category — must win over violence.
            var scores = new ImageCategoryScores(
                sexual: 0.5f,
                sexualMinors: 0.5f,
                violence: 0.5f,
                violenceGraphic: 0.5f,
                harassment: 0f, hate: 0f, hateThreatening: 0f, selfHarm: 0f);

            var result = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);

            Assert.IsFalse(result.IsAllowed);
            Assert.AreEqual("sexual/minors", result.OffendingCategory);
        }

        [Test]
        public void Evaluate_BelowAllThresholds_ReportsHighestScoreInAllowedResult()
        {
            var scores = new ImageCategoryScores(
                sexual: 0.05f, sexualMinors: 0f,
                violence: 0.12f, violenceGraphic: 0.03f,
                harassment: 0.08f, hate: 0.02f, hateThreatening: 0.01f, selfHarm: 0.01f);

            var result = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);

            Assert.IsTrue(result.IsAllowed);
            Assert.AreEqual(0.12f, result.HighestScore, 0.0001f);
            Assert.AreEqual("violence", result.HighestCategory);
        }

        [Test]
        public void Evaluate_CustomThresholdsTighterThanDefault_RejectsImagesDefaultWouldAllow()
        {
            // sexual=0.065 — below default 0.09, so default allows.
            var scores = new ImageCategoryScores(
                sexual: 0.065f, sexualMinors: 0f,
                violence: 0f, violenceGraphic: 0f,
                harassment: 0f, hate: 0f, hateThreatening: 0f, selfHarm: 0f);

            var defaultResult = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);
            Assert.IsTrue(defaultResult.IsAllowed, "Default thresholds should allow sexual=0.065.");

            // Tighten the sexual threshold and the same image must now reject.
            var tighter = new Pg13Thresholds(
                sexual: 0.05f,
                sexualMinors: Pg13ImagePolicy.SexualMinorsThreshold,
                violence: Pg13ImagePolicy.ViolenceThreshold,
                violenceGraphic: Pg13ImagePolicy.ViolenceGraphicThreshold,
                harassment: Pg13ImagePolicy.HarassmentThreshold,
                hate: Pg13ImagePolicy.HateThreshold,
                hateThreatening: Pg13ImagePolicy.HateThreateningThreshold,
                selfHarm: Pg13ImagePolicy.SelfHarmThreshold);

            var tightResult = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false, tighter);

            Assert.IsFalse(tightResult.IsAllowed);
            Assert.AreEqual("sexual", tightResult.OffendingCategory);
        }

        [Test]
        public void Evaluate_DefaultStructEqualsConstants()
        {
            var d = Pg13Thresholds.Default;

            Assert.AreEqual(Pg13ImagePolicy.SexualThreshold,          d.Sexual);
            Assert.AreEqual(Pg13ImagePolicy.SexualMinorsThreshold,    d.SexualMinors);
            Assert.AreEqual(Pg13ImagePolicy.ViolenceThreshold,        d.Violence);
            Assert.AreEqual(Pg13ImagePolicy.ViolenceGraphicThreshold, d.ViolenceGraphic);
            Assert.AreEqual(Pg13ImagePolicy.HarassmentThreshold,      d.Harassment);
            Assert.AreEqual(Pg13ImagePolicy.HateThreshold,            d.Hate);
            Assert.AreEqual(Pg13ImagePolicy.HateThreateningThreshold, d.HateThreatening);
            Assert.AreEqual(Pg13ImagePolicy.SelfHarmThreshold,        d.SelfHarm);
        }

        [Test]
        public void Evaluate_AllowedResult_PopulatesHighestCategoryNotOffending()
        {
            var scores = new ImageCategoryScores(
                sexual: 0.05f, sexualMinors: 0f,
                violence: 0.08f, violenceGraphic: 0f,
                harassment: 0f, hate: 0f, hateThreatening: 0f, selfHarm: 0f);

            var result = Pg13ImagePolicy.Evaluate(scores, providerFlagged: false);

            Assert.IsTrue(result.IsAllowed);
            Assert.IsNull(result.OffendingCategory, "OffendingCategory is only set on rejection.");
            Assert.AreEqual("violence", result.HighestCategory);
            Assert.AreEqual(0.08f, result.HighestScore, 0.0001f);
        }
    }
}