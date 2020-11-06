using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveSearcher.BackEnd
{
    public static class ArffCompiler
    {
        private static List<string> AtributeNames = new List<string>();
        private static List<string> DataValue = new List<string>();
        private static int Lock = 0;
        private static readonly Object LockToken = new Object();
        private static int ProcessedFiles = 0;
        private static int FileCount = 0;
        public static void ExecuteSearch(string[] SearchEntries, string path, CancellationToken cancellationToken, Delegate DelegatePause)
        {
            try
            {
                SearchEntries = FileOperations.getExistingSearchEntries(SearchEntries, path);

                ProcessedFiles = 0;
                FileCount = 0;
                AtributeNames.Clear();
                DataValue.Clear();
                Lock = 0;
                LockToken.Equals(null);

                int count = FileOperations.FileCount(path);
                FileCount = count;
                int lenght = 4;
                int divider = count / lenght;
                Console.WriteLine("Lenght of files: " + count.ToString());
                Console.WriteLine("Number of threads: " + lenght);
                Console.WriteLine("Divider: " + divider);
                int Suma = 0;
                int UpTo;
                int[] array = new int[lenght + 1];
                // if (lenght > 1) 
                //{ 
                for (int i = 0; i < lenght + 1; i++)
                {
                    array[i] = Suma;
                    Suma = Suma + divider;
                }
                for (int i = 0; i < lenght; i++)
                {

                    if (i == lenght - 1)
                    {
                        int variable1 = array[i];
                        int variable2 = count;
                        Thread th = new Thread(delegate ()
                        {
                            SearchZipFile(path, SearchEntries, variable1, variable2, cancellationToken, DelegatePause);
                        });
                        th.Start();
                    }
                    else
                    {
                        int variable1 = array[i];
                        int variable2 = array[i + 1];
                        Thread th = new Thread(delegate ()
                        {
                            SearchZipFile(path, SearchEntries, variable1, variable2, cancellationToken, DelegatePause);
                        });
                        th.Start();
                    }


                }
                // }



                Thread watcher = new Thread(delegate ()
                {
                    Write(lenght);

                });
                watcher.Start();

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

        }


        public static void PrintList()
        {
            for (int i = 0; i < AtributeNames.Count; i++)
            {
                Console.WriteLine(AtributeNames[i]);
                Console.WriteLine(DataValue[i]);
            }
        }


        public static void SearchZipFile(string path, string[] pattern, int start, int stop, CancellationToken cancellationToken, Delegate DelegatePause)
        {

           
            string[] filePaths = Directory.GetFiles(path);

            for (int j = start; j < stop; j++)
            {
                try
                {


                    bool pause = Convert.ToBoolean(DelegatePause.DynamicInvoke());
                    while (pause == true)
                    {
                        pause = Convert.ToBoolean(DelegatePause.DynamicInvoke());
                    }
                    if (cancellationToken.IsCancellationRequested == false)
                    {
                        if (filePaths[j].EndsWith(".arff"))
                        {
                            List<string> tempList = new List<string>();
                            using (StreamReader reader = new StreamReader(filePaths[j]))
                            {

                                string line;
                                int AtributeCount = 0;
                                int ValueCount = 0;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    tempList.Add(line);

                                }

                                string DataLine = tempList[tempList.Count - 1];
                                string[] Data = DataLine.Split(',');
                                AtributeCount = tempList.Count - 6;
                                ValueCount = Data.Length;
                                if (AtributeCount == ValueCount)
                                {

                                    string atribute = "";
                                    string tempValue = "";
                                    for (int i = 0; i < pattern.Length; i++)
                                    {

                                        var matchingvalues = tempList
                                       .Where(stringToCheck => stringToCheck.Contains(pattern[i]));

                                        if (matchingvalues != null)
                                        {
                                            int indexOfValue = tempList.FindIndex(a => a.Contains(pattern[i]));

                                            atribute = tempList[indexOfValue];
                                            string data = Data[indexOfValue - 2];
                                            if (data != "")
                                            {
                                                tempValue = tempValue + data + ",";
                                            }

                                            lock (LockToken)
                                            {



                                                if (AtributeNames.Contains(atribute))
                                                {

                                                }
                                                else
                                                {
                                                    AtributeNames.Add(atribute);
                                                }
                                            }

                                        }

                                    }
                                    lock (LockToken)
                                    {
                                        ProcessedFiles++;
                                        DataValue.Add(tempValue);


                                    }
                                }
                            }

                        }
                    }

                }
                catch(Exception exc)
                {
                    ProcessedFiles++;
                    continue;
                    
                }
            }
            Lock++;

          


        }
        public static void Write(int ThreadCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool RoundnRound = true;
            while (RoundnRound == true)
            {

                if (ThreadCount == Lock)
                {
                    sw.Stop();
                    
                    RoundnRound = false;
                    Console.WriteLine("Stopped and lasted"+ sw.Elapsed);
                }

            }
            FileOperations.CreateArff(AtributeNames, DataValue);
        }
        public static int getFilesProcessed()
        {
            return ProcessedFiles;
        }
        public static int getFileCount()
        {
            return FileCount;
        }

    }
}
