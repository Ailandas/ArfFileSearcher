using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveSearcher.BackEnd
{
    public static class FileOperations
    {
        private static string getDestinationPath()
        {
            var enviroment = System.Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(enviroment).Parent.FullName;

            return projectDirectory;
        }
        public static void CreateArff(List<string> Atributes, List<string> Values)
        {
            string ProjectDirectory = getDestinationPath() + @"\BackEnd\";
            string ArfDirectory = ProjectDirectory + "Result.arff";
            if (File.Exists(ArfDirectory))
            {
                File.Delete(ArfDirectory);
                string[] atributes = Atributes.ToArray();
                string[] values = new string[Values.Count];
             

                using (FileStream stream1 = File.Open(ArfDirectory, FileMode.OpenOrCreate))
                {
                    using (StreamWriter strWriter = new StreamWriter(stream1))
                    {
                        foreach(string atribute in Atributes)
                        {
                            strWriter.WriteLine(atribute);
                        }

                        strWriter.WriteLine("\n");
                        strWriter.WriteLine("@Data");
                        strWriter.WriteLine("\n");
                        ///values
                        ///

                        for (int i=0; i < Values.Count; i++)
                        {
                            string[] splitted = Values[i].Split(',');
                            string content = "";
                            int jj = 0;
                            int splitteris = splitted.Length - 1;
                            for(int j=0; j < splitted.Length-1; j++)
                            {

                                if (jj == splitteris - 1)
                                {
                                    content = content + splitted[j];
                                }
                                else
                                {
                                    content = content + splitted[j] + ",";
                                }
                                jj++;
                            }
                            strWriter.WriteLine(content);
                        }
                    }

                }
            }
            
            
        }
        public static int FileCount(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            int count=0;
            foreach (var file in di.EnumerateFiles())
            {
                count++;
            }
            return count;
        }
            
            public static bool CheckIfEgzisting(string path,string fileName)
        {

            DirectoryInfo di = new DirectoryInfo(path);
            foreach (var file in di.EnumerateFiles()) {


                string textFileContent = File.ReadAllText(file.FullName);
             
                    if (textFileContent.Contains(fileName))
                    {
                    return true;
                    
                    }
                    else
                    {

                    return false;
                    }
                 
            }
                
                
            
            return false;
        }
        public static string[] getExistingSearchEntries(string[] SearchEntries,string path)
        {
            bool[] DoSearchEntriesEgzist = new bool[SearchEntries.Length];
            for (int i = 0; i < SearchEntries.Length; i++)
            {
                DoSearchEntriesEgzist[i] = CheckIfEgzisting(path, SearchEntries[i]);
            }
            //count true ones;
            int trueOnes = 0;
            foreach (bool rezult in DoSearchEntriesEgzist)
            {
                if (rezult == true)
                {
                    trueOnes++;
                }
            }
            string[] NewSearchEntries = new string[trueOnes];
            for (int i = 0; i < SearchEntries.Length; i++)
            {
                if (DoSearchEntriesEgzist[i] == true)
                {
                    NewSearchEntries[i] = SearchEntries[i];
                }
            }
         
            return NewSearchEntries;
        }
    }
}
