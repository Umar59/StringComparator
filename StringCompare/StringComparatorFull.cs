namespace StringCompare
{
    public struct ComparatorResults
    {
        public Dictionary<string, string> WrongRecognition { get; }
        public Dictionary<string, int> RecognizedMultipleTimes { get; }
        public List<string> NotRecognizedWords { get; }
        public float Similarity { get; }

        public Dictionary<string, string> Pairs { get; }

        public ComparatorResults(Dictionary<string, string> pairs, Dictionary<string, string> wrongRecognition,
            Dictionary<string, int> recognizedMultipleTimes, List<string> notRecognizedWords, float similarity)
        {
            Pairs = pairs;
            WrongRecognition = wrongRecognition;
            RecognizedMultipleTimes = recognizedMultipleTimes;
            NotRecognizedWords = notRecognizedWords;
            Similarity = similarity;
        }
    }

    public class StringComparatorFull
    {
        public  ComparatorResults Compare(List<string> initial, List<string> voiceRec)
        {
            int initialCount = initial.Count;
            int voiceRecCount = voiceRec.Count;

            MakeLowerCase(initial);
            MakeLowerCase(voiceRec);

            var recognizedMultipleTimes = RemoveFirstAppearedDuplicates(voiceRec);

            Dictionary<int, string> initialWithIndex = new Dictionary<int, string>();
            Dictionary<int, string> voiceWithIndex = new Dictionary<int, string>();

            CreateIndexedList(initial, initialWithIndex);
            CreateIndexedList(voiceRec, voiceWithIndex);

            Dictionary<int, string> initialWithIndexCopy = new Dictionary<int, string>(initialWithIndex);
            Dictionary<int, string> voiceWithIndexCopy = new Dictionary<int, string>(voiceWithIndex);

            Dictionary<int, int> pairsIndices = new Dictionary<int, int>();

            CalculatePairs(pairsIndices, voiceWithIndex, initialWithIndex);

            Dictionary<string, string> pairsValues = new Dictionary<string, string>();

            foreach (var index in pairsIndices)
            {
                pairsValues.Add(initialWithIndexCopy[index.Key], voiceWithIndexCopy[index.Value]);
            }

            List<int> notRecognizedIndices = new List<int>();
            Dictionary<string, string> wrongRecognition = new Dictionary<string, string>();

            CalculateWrongAndNotRecognized(wrongRecognition, notRecognizedIndices, voiceWithIndexCopy,
                initialWithIndexCopy,
                initialWithIndex, voiceWithIndex, pairsIndices);

            List<string> notRecognizedWords = IndicesToValues(notRecognizedIndices, initialWithIndexCopy);


            var similarity = CalculateSimilarity(initialCount, voiceRecCount, pairsIndices.Count);

            return new ComparatorResults(pairsValues, wrongRecognition, recognizedMultipleTimes, notRecognizedWords,
                similarity);
        }

        private  List<T> IndicesToValues<T>(IEnumerable<int> indices, Dictionary<int, T> indexedValues)
        {
            List<T> values = new List<T>();

            foreach (var index in indices)
            {
                if (indexedValues.ContainsKey(index))
                {
                    values.Add(indexedValues[index]);
                }
            }

            return values;
        }

        public int Test(int a)
        {
            return a + 1;
        }

        private  void CreateIndexedList<T>(List<T> convertInit, Dictionary<int, T> converted)
        {
            for (var i = 0; i < convertInit.Count; i++)
            {
                converted.Add(i, convertInit[i]);
            }
        }

        private  void CalculateWrongAndNotRecognized
        (
            Dictionary<string, string> wrongRecognition,
            List<int> notRecognizedIndices,
            Dictionary<int, string> voiceWithIndexCopy,
            Dictionary<int, string> initialWithIndexCopy,
            Dictionary<int, string> initialWithIndex,
            Dictionary<int, string> voiceWithIndex,
            Dictionary<int, int> pairs
        )
        {
            //тут мы обязательно смотрим из исходных слов в выходные

            Dictionary<List<int>, List<int>> groups = new Dictionary<List<int>, List<int>>();


            foreach (var keyValuePair in pairs)
            {
                int keyIndex = keyValuePair.Key + 1, valueIndex = keyValuePair.Value + 1;

                List<int> initGroupMember = new List<int>();
                List<int> voiceGroupMember = new List<int>();

                while (!pairs.ContainsValue(valueIndex) && voiceWithIndexCopy.ContainsKey(valueIndex))
                {
                    voiceGroupMember.Add(valueIndex);

                    valueIndex++;
                }
                //  0    1    2    3    4    5    6    7    8
                // "c", "x", "y", "z", "b", "a";
                // "c", "x", "y", "z", "x", "x", "y", "a", "f"

                //                      4    5    6    8
                //                     "b"       
                //                     "x", "x", "y", "f"     
                
                while (!pairs.ContainsKey(keyIndex) && initialWithIndexCopy.ContainsKey(keyIndex))
                {
                    initGroupMember.Add(keyIndex);

                    keyIndex++;
                }

                //потому что последующие слова могут быть неправильно обработанным словом, которому мы уже нашли пару.

                if (voiceGroupMember.Count > 0 && initGroupMember.Count == 0)
                {
                    initGroupMember.Add(keyValuePair.Key);
                }

                if (voiceGroupMember.Count > 0)
                {
                    groups.Add(initGroupMember, voiceGroupMember);
                }
                else if (initGroupMember.Count > 0)
                {
                    foreach (var s in initGroupMember)
                    {
                        notRecognizedIndices.Add(s);
                    }
                }
            }

            WrongRecognized(groups, voiceWithIndexCopy, initialWithIndexCopy, notRecognizedIndices, wrongRecognition);
        }

        private  void WrongRecognized
        (
            Dictionary<List<int>, List<int>> groups,
            Dictionary<int, string> voiceWithIndexCopy,
            Dictionary<int, string> initialWithIndexCopy,
            List<int> notRecognizedIndices,
            Dictionary<string, string> wrongRecognition)
        {
            foreach (var group in groups)
            {
                //короче тут надо описать матрицу сходимостей слов, а затем с большей сходимостью сгруппировать. Остальные добавить в NotRecognizedkekey

                for (var i = 0; i < group.Value.Count; i++)
                {
                    float similarity = 0;
                    int initPairIndex = 0;

                    foreach (var initialRemainIndex in group.Key)
                    {
                        float currentSimilarity = StringSimilarity(voiceWithIndexCopy[group.Value[i]],
                            initialWithIndexCopy[initialRemainIndex]);
                        if (similarity < currentSimilarity)
                        {
                            similarity = currentSimilarity;
                            initPairIndex = initialRemainIndex;
                        }
                    }

                    if (similarity > 0)
                    {
                        if (wrongRecognition.ContainsKey(initialWithIndexCopy[initPairIndex]))
                        {
                            wrongRecognition[initialWithIndexCopy[initPairIndex]] += voiceWithIndexCopy[group.Value[i]];
                        }
                        else
                        {
                            wrongRecognition.Add(initialWithIndexCopy[initPairIndex],
                                voiceWithIndexCopy[group.Value[i]]);
                        }

                        group.Value.Remove(group.Value[i]);
                        i--;
                    }
                    else
                    {
                        //хз куда их посылать
                    }
                }

                for (var i = 0; i < group.Key.Count; i++)
                {
                    if (!wrongRecognition.ContainsKey(initialWithIndexCopy[group.Key[i]]))
                    {
                        notRecognizedIndices.Add(group.Key[i]);
                        group.Key.RemoveAt(i);
                    }
                }
            }
        }

        private  float StringSimilarity(string a, string b)
        {
            return 0.8f;
        }

        private  void CalculatePairs(Dictionary<int, int> pairs,
            Dictionary<int, string> voiceWithIndex,
            Dictionary<int, string> initialWithIndex)
        {
            for (int i = 0; i <= voiceWithIndex.Last().Key; i++)
            {
                for (int j = initialWithIndex.First().Key; j <= initialWithIndex.Last().Key; j++)
                {
                    if (voiceWithIndex.ContainsKey(i) && initialWithIndex.ContainsKey(j))
                    {
                        if (voiceWithIndex[i] == initialWithIndex[j])
                        {
                            pairs.Add(j, i);
                            voiceWithIndex.Remove(i);
                            initialWithIndex.Remove(j);

                            break;
                        }
                    }
                }
            }
        }

        private  void MakeLowerCase(List<string> initial)
        {
            for (int i = 0; i < initial.Count; i++)
            {
                initial[i] = initial[i].ToLower();
            }
        }

        public Dictionary<string, int> RemoveFirstAppearedDuplicates(List<string> voiceRec)
        {
            Dictionary<string, int> duplicatesFromVoiceRec = new Dictionary<string, int>();
            List<string> alreadyChecked = new List<string>();

            for (int i = 0; i < voiceRec.Count; i++)
            {
                if (!alreadyChecked.Contains(voiceRec[i]))
                {
                    int appearancesCount = 1;

                    for (int j = i + 1; j < voiceRec.Count; j++)
                    {
                        if (voiceRec[i] == voiceRec[j])
                        {
                            voiceRec.RemoveAt(j);
                            appearancesCount++;

                            alreadyChecked.Add(voiceRec[i]);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!alreadyChecked.Contains(voiceRec[i]))
                    {
                        alreadyChecked.Add(voiceRec[i]);
                    }

                    if (appearancesCount > 1)
                    {
                        duplicatesFromVoiceRec.Add(voiceRec[i], appearancesCount);
                    }
                }
            }

            return duplicatesFromVoiceRec;
        }

        public float CalculateSimilarity(float initialCount, float voiceRecCount, float similarPairsCount)
        {
            if (voiceRecCount <= 0 || initialCount <= 0 || similarPairsCount < 0)
            {
                return -1;
            }
            if (initialCount > voiceRecCount)
            {
                if (initialCount < similarPairsCount)
                {
                    return -1;
                }
                return similarPairsCount / initialCount;
            }
            else
            {
                if (voiceRecCount < similarPairsCount)
                {
                    return -1;
                }
                return similarPairsCount / voiceRecCount;
            }

        }
    }
}