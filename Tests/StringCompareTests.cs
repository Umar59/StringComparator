using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NUnit.Framework;
using Moq;
using StringCompare;

namespace Tests
{
    public class StringCompareTests
    {
        private StringComparatorFull _comparatorFull;

        [SetUp]
        public void Setup()
        {
            _comparatorFull = new StringComparatorFull();
        }

        [Test]
        #region TestCases

        [TestCase(3, 4, 2, ExpectedResult = 0.5f)]
        [TestCase(4, 3, 2, ExpectedResult = 0.5f)]
        [TestCase(10, 10, 9, ExpectedResult = 0.9f)]
        [TestCase(10, 10, 10, ExpectedResult = 1f)]
        [TestCase(30, 20, 0, ExpectedResult = 0f)]
        [TestCase(float.MaxValue, float.MaxValue, float.MaxValue, ExpectedResult = 1f)]
        [TestCase(float.MinValue, float.MinValue, float.MinValue, ExpectedResult = -1f)]
        [TestCase(0, 0, 0, ExpectedResult = -1f)]
        [TestCase(3, 4, 5, ExpectedResult = -1f)]
        [TestCase(5, 4, 50, ExpectedResult = -1f)]
        [TestCase(-1, -1, 1, ExpectedResult = -1f)]
        [TestCase(1, -1, 1, ExpectedResult = -1f)]
        [TestCase(-1, 1, 1, ExpectedResult = -1f)]
        [TestCase(-1, -1, -1, ExpectedResult = -1f)]
        [TestCase(1, 1, -1, ExpectedResult = -1f)]
        [TestCase(1, -1, -1, ExpectedResult = -1f)]
        [TestCase(-1, 1, -1, ExpectedResult = -1f)]

        #endregion
        public float IsCalculatingWordsListSimilarity(float initialCountTest, float voiceRecCountTest,
            float similarPairsCountTest)
        {
            var similarity =
                _comparatorFull.CalculateSimilarity(initialCountTest, voiceRecCountTest, similarPairsCountTest);

            return similarity;
        }

        [Test]
        #region TestCases

        [TestCase(new string[] { "Hello", "World" }, new string[] { "hello", "world" })]
        [TestCase(new string[] { "HeLlo", "WoRLd" }, new string[] { "hello", "world" })]
        [TestCase(new string[] { "HELLO", "WORLD" }, new string[] { "hello", "world" })]
        [TestCase(new string[] { "HellO", "WorlD" }, new string[] { "hello", "world" })]
        [TestCase(new string[] { "hello", "world" }, new string[] { "hello", "world" })]
        [TestCase(new string[] { }, new string[] { })]

        #endregion TestCases
        public void IsMakingLowerCase(string[] upperCaseStrings, string[] expected)
        {
            List<string> actual = new List<string>(upperCaseStrings);

            _comparatorFull.MakeLowerCase(actual);

            Assert.AreEqual(expected, actual);
        }

        #region TestCases

        private static object[] _testCasesCalculatingPairs =
        {
            new object[]
            {
                new Dictionary<int, int>()
                {
                    { 1, 1 }
                },
                new Dictionary<int, string>()
                {
                    { 1, "a" },
                    { 2, "b" }
                },
                new Dictionary<int, string>()
                {
                    { 1, "a" },
                    { 2, "c" }
                }
            },
        };

        #endregion

        [Test]
        [TestCaseSource(nameof(_testCasesCalculatingPairs))]
        public void IsCalculatingPairsCorrect(Dictionary<int, int> expectedPairsIndices,
            Dictionary<int, string> indexedVoice, Dictionary<int, string> indexedInitial)
        {
            Dictionary<int, int> actualPairsIndices = _comparatorFull.CalculatePairs(indexedVoice, indexedInitial);

            Assert.AreEqual(expectedPairsIndices, actualPairsIndices);
        }

        #region TestCases

        private static object[] _testCasesIsRemovingFirstAppearedDuplicates =
        {
            new object[]
            {
                new List<string>() { "a", "b" },
                new List<string>() { "a", "b" },
                new Dictionary<string, int>() { }
            },
            new object[]
            {
                new List<string>() { "a", "b", "a" },
                new List<string>() { "a", "b", "a" },
                new Dictionary<string, int>() { }
            },
            new object[]
            {
                new List<string>() { "a", "a" },
                new List<string>() { "a" },
                new Dictionary<string, int>() { { "a", 2 } }
            },
            new object[]
            {
                new List<string>() { "a", "a", "a" },
                new List<string>() { "a" },
                new Dictionary<string, int>() { { "a", 3 } }
            },
            new object[]
            {
                new List<string>() { "a", "b", "b", "a", "x", "x" },
                new List<string>() { "a", "b", "a", "x" },
                new Dictionary<string, int>() { { "b", 2 }, { "x", 2 } }
            },
            new object[]
            {
                new List<string>() { "a", "b", "b", "b", "a", "x", "x" },
                new List<string>() { "a", "b", "a", "x" },
                new Dictionary<string, int>() { { "b", 3 }, { "x", 2 } }
            },
            new object[]
            {
                new List<string>() { },
                new List<string>() { },
                new Dictionary<string, int>() { }
            },
        };

        #endregion
        [Test]
        [TestCaseSource(nameof(_testCasesIsRemovingFirstAppearedDuplicates))]
        public void IsRemovingFirstAppearedDuplicates(List<string> beforeRemoving, List<string> afterRemoving,
            Dictionary<string, int> expectedDuplicatesCount)
        {
            List<string> afterRemovingOutput = new List<string>(beforeRemoving);
            Dictionary<string, int> actualDuplicatesCount =
                _comparatorFull.RemoveFirstAppearedDuplicates(afterRemovingOutput);

            Assert.AreEqual(afterRemoving, afterRemovingOutput);
            Assert.AreEqual(expectedDuplicatesCount, actualDuplicatesCount);
        }

        #region TestCases

        private static object[] _testCasesWrongRecognizedNotRecognized =
        {
            new object[]
            {
                new Dictionary<string, string>()
                {
                    { "b", "xxy" },
                    { "r", "f" },
                },
                new List<int>()
                {
                    7
                },
                new Dictionary<int, string>()
                {
                    { 0, "c" },
                    { 1, "x" },
                    { 2, "y" },
                    { 3, "z" },
                    { 4, "x" },
                    { 5, "x" },
                    { 6, "y" },
                    { 7, "a" },
                    { 8, "f" },
                },
                new Dictionary<int, string>()
                {
                    { 0, "c" },
                    { 1, "x" },
                    { 2, "y" },
                    { 3, "z" },
                    { 4, "b" },
                    { 5, "a" },
                    { 6, "r" },
                    { 7, "3" }
                },
                new Dictionary<int, int>()
                {
                    { 0, 0 },
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 5, 7 }
                }
            }
        };

        #endregion
        [Test]
        [TestCaseSource(nameof(_testCasesWrongRecognizedNotRecognized))]
        public void IsCalculatingWrongRecognizedNotRecognizedCorrect
        (
            Dictionary<string, string> expectedWrongRecognition,
            List<int> expectedNotRecognizedIndices,
            Dictionary<int, string> inputIndexedVoice,
            Dictionary<int, string> inputIndexedInitial,
            Dictionary<int, int> pairs)
        {
            List<int> notRecognizedIndices = new List<int>();
            Dictionary<string, string> wrongRecognition = new Dictionary<string, string>();

            _comparatorFull.CalculateNotRecognized(wrongRecognition, notRecognizedIndices,
                inputIndexedVoice, inputIndexedInitial, pairs);

            Assert.AreEqual(expectedWrongRecognition, wrongRecognition);
            Assert.AreEqual(expectedNotRecognizedIndices, notRecognizedIndices);
        }

        private static object[] _testCasesCreatingIndexedList =
        {
            new object[]
            {
                new List<string>()
                {
                    "hello",
                    "world"
                },
                new Dictionary<int, string>()
                {
                    {0, "hello"},
                    {1, "world"}
                }
            },
            new object[]
            {
                new List<string>()
                {
                    "ICVR"
                },
                new Dictionary<int, string>()
                {
                    {0, "ICVR"}
                }
            }
        };
        
        [Test]
        [TestCaseSource(nameof(_testCasesCreatingIndexedList))]
        public void IsCreatingIndexedList<T>(List<T> inputUnindexed, Dictionary<int, T> expected)
        {
            Dictionary<int, T> actual = new Dictionary<int, T>();
            
            actual = _comparatorFull.CreateIndexedList<T>(inputUnindexed);
            
            Assert.AreEqual(expected, actual);
        }

        private static object[] _testCasesIndicesToValues =
        {
            new object[]
            {
                new int[]
                {
                    1
                },
                new Dictionary<int, string>()
                {
                    {0, "hello"},
                    {1, "world"}
                },
                new List<string>()
                {
                    "world"
                },
            },
        };

        [Test]
        [TestCaseSource(nameof(_testCasesIndicesToValues))]
        public void IsConvertingIndicesToValues<T>(IEnumerable<int> indices, Dictionary<int, T> input, List<T> expected)
        {
            List<T> actual = new List<T>();

            actual = _comparatorFull.IndicesToValues(indices, input);
            
            Assert.AreEqual(expected, actual);
        }
        
    }
}