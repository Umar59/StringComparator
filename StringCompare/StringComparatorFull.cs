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
        public ComparatorResults Compare(List<string> initial, List<string> voiceRec)
        {
            int initialCount = initial.Count;
            int voiceRecCount = voiceRec.Count;

            MakeLowerCase(initial);
            MakeLowerCase(voiceRec);

            var recognizedMultipleTimes = RemoveFirstAppearedDuplicates(voiceRec);

            var initialWithIndex = CreateIndexedList(initial);
            var voiceWithIndex = CreateIndexedList(voiceRec);

            Dictionary<int, string> initialWithIndexCopy = new Dictionary<int, string>(initialWithIndex);
            Dictionary<int, string> voiceWithIndexCopy = new Dictionary<int, string>(voiceWithIndex);

            Dictionary<int, int> pairsIndices = CalculatePairs(voiceWithIndex, initialWithIndex);

            Dictionary<string, string> pairsValues = new Dictionary<string, string>();

            foreach (var index in pairsIndices)
            {
                pairsValues.Add(initialWithIndexCopy[index.Key], voiceWithIndexCopy[index.Value]);
            }

            List<int> notRecognizedIndices = new List<int>();
            Dictionary<string, string> wrongRecognition = new Dictionary<string, string>();

            CalculateNotRecognized(wrongRecognition, notRecognizedIndices, voiceWithIndexCopy,
                initialWithIndexCopy, pairsIndices);

            List<string> notRecognizedWords = IndicesToValues(notRecognizedIndices, initialWithIndexCopy);


            var similarity = CalculateSimilarity(initialCount, voiceRecCount, pairsIndices.Count);

            return new ComparatorResults(pairsValues, wrongRecognition, recognizedMultipleTimes, notRecognizedWords,
                similarity);
        }

        public List<T> IndicesToValues<T>(IEnumerable<int> indices, Dictionary<int, T> indexedValues)
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

        public Dictionary<int, T> CreateIndexedList<T>(List<T> convertInit)
        {
            Dictionary<int, T> converted = new Dictionary<int, T>();
            
            for (var i = 0; i < convertInit.Count; i++)
            {
                converted.Add(i, convertInit[i]);
            }

            return converted;
        }

        public void CalculateNotRecognized
        (
            Dictionary<string, string> wrongRecognition,
            List<int> notRecognizedIndices,
            Dictionary<int, string> voiceWithIndexCopy,
            Dictionary<int, string> initialWithIndexCopy,
            Dictionary<int, int> pairs
        )
        {
            //тут мы обязательно смотрим из исходных слов в выходные

            Dictionary<List<int>, List<int>> groups = new Dictionary<List<int>, List<int>>();

            List<int> initGroupMember = new List<int>();
            List<int> voiceGroupMember = new List<int>();
            
            foreach (var keyValuePair in pairs)
            {
                int keyIndex = keyValuePair.Key + 1, valueIndex = keyValuePair.Value + 1;

                initGroupMember.Clear();
                voiceGroupMember.Clear();

                while (!pairs.ContainsValue(valueIndex) && voiceWithIndexCopy.ContainsKey(valueIndex))
                {
                    voiceGroupMember.Add(valueIndex);

                    valueIndex++;
                }   

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
                    groups.Add(new List<int>(initGroupMember), new List<int>(voiceGroupMember));
                }
                else if (initGroupMember.Count > 0)
                {
                    foreach (var s in initGroupMember)
                    {
                        notRecognizedIndices.Add(s);
                    }
                }
            }
            
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
            CalculateWrongRecognized(groups, voiceWithIndexCopy, initialWithIndexCopy, notRecognizedIndices, wrongRecognition);
        }

        private void CalculateWrongRecognized
        (
            Dictionary<List<int>, List<int>> groups,
            Dictionary<int, string> voiceWithIndexCopy,
            Dictionary<int, string> initialWithIndexCopy,
            List<int> notRecognizedIndices,
            Dictionary<string, string> wrongRecognition)
        {
            
        }

        private float StringSimilarity(string a, string b)
        {
            return 0.8f;
        }

        public Dictionary<int, int> CalculatePairs(
            Dictionary<int, string> voiceWithIndex,
            Dictionary<int, string> initialWithIndex)
        {
            Dictionary<int, int> pairsIndices = new Dictionary<int, int>();
            
            for (int i = 0; i <= voiceWithIndex.Last().Key; i++)
            {
                for (int j = initialWithIndex.First().Key; j <= initialWithIndex.Last().Key; j++)
                {
                    if (voiceWithIndex.ContainsKey(i) && initialWithIndex.ContainsKey(j))
                    {
                        if (voiceWithIndex[i] == initialWithIndex[j])
                        {
                            pairsIndices.Add(j, i);
                            voiceWithIndex.Remove(i);
                            initialWithIndex.Remove(j);

                            break;
                        }
                    }
                }
            }

            return pairsIndices;
        }

        public void MakeLowerCase(List<string> initial)
        {
            for (int i = 0; i < initial.Count; i++)
            {
                initial[i] = initial[i].ToLower();
            }
        }

        public Dictionary<string, int> RemoveFirstAppearedDuplicates(List<string> voiceRec)
        {
            Dictionary<string, int> firstAppearedDuplicates = new Dictionary<string, int>();
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
                            j--;
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
                        firstAppearedDuplicates.Add(voiceRec[i], appearancesCount);
                    }
                }
            }

            return firstAppearedDuplicates;
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