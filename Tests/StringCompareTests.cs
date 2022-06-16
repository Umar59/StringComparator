using Microsoft.VisualStudio.TestPlatform.Utilities;
using NUnit.Framework;
using Moq;

namespace Tests
{
    public class StringCompareTests
    {
        private StringCompare.StringComparatorFull _comparatorFull;

        [SetUp]
        public void Setup()
        {
            _comparatorFull = new StringCompare.StringComparatorFull();
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
        
        public float IsCalculatingWordsListSimilarity(float initialCountTest, float voiceRecCountTest, float similarPairsCountTest)
        {
            var similarity =
                _comparatorFull.CalculateSimilarity(initialCountTest, voiceRecCountTest, similarPairsCountTest);
            
            return similarity;
        }

        #region TestCasesStruct

        public struct RemovingList
        {
            public RemovingList(List<string> testCase, List<string> removedDuplicatesExpected, Dictionary<string, int> outPutExpected)
            {
                TestCase = testCase;
                RemovedDuplicatesExpected = removedDuplicatesExpected;
                OutPutExpected = outPutExpected;
            }

            public List<string> TestCase { get; }
            public List<string> RemovedDuplicatesExpected { get; }
            public Dictionary<string, int> OutPutExpected { get; }
        }
        #endregion testCasesStruct
        
        [Test]
        public void IsRemovingFirstAppearedDuplicates()
        {
            List<string> beforeRemovingDuplicates;
            List<string> afterRemovingDuplicates;
            Dictionary<int, int> outputDuplicatesExpected;
            int[][] d = new int[2][];
            outputDuplicatesExpected = d.ToDictionary(ints => {  })
            #region TestCases

            List<RemovingList> TestCases = new List<RemovingList>()
            {
                new RemovingList(new List<string>()
                {
                    "c", "x", "x", "y", "z", "x", "x", "y", "a", "f"
                }, new List<string>()
                {
                    "c", "x", "y", "z", "x", "x", "y", "a", "f"
                }, new Dictionary<string, int>()
                {
                    {"x", 2}
                }),
                // new RemovingList(new List<string>()
                // {
                //     "c", "c", "c", "c", "c", "c", "c", "c", "c", "c"
                // }, new List<string>()
                // {
                //     "c"
                // }, new Dictionary<string, int>()
                // {
                //     {"c", 10}
                // }),
                new RemovingList(new List<string>()
                {
                    "c", "c", "c", "y", "c", "y", "y", "y", "f", "f"
                }, new List<string>()
                {
                    "c", "y", "c", "y", "y", "y", "f"
                }, new Dictionary<string, int>()
                {
                    {"c", 3},
                    {"f", 2}
                }),
                // new RemovingList(new List<string>()
                // {
                //     "c", "y", "c", "y", "y", "y", "f"
                // }, new List<string>()
                // {
                //     "c", "y", "c", "y", "y", "y", "f"
                // }, new Dictionary<string, int>()
                // {
                // }),
                // new RemovingList(new List<string>()
                // {
                // }, new List<string>()
                // {
                // }, new Dictionary<string, int>()
                // {
                // }),
            };

            #endregion

            foreach (var removingList in TestCases)
            {
                List<string> input = new List<string>(removingList.TestCase);

                Dictionary<string, int> outputActual = _comparatorFull.RemoveFirstAppearedDuplicates(input);
            
                Assert.AreEqual(removingList.RemovedDuplicatesExpected, input);
               // Assert.AreEqual(removingList.OutPutExpected, outputActual);
            }
        }
    }
}