using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Silky
{
    static class Core
    {
        public class Operation
        {
            public String From;
            public String To;
            public char PartType;

            public Operation(String From, String To, char PartType)
            {
                this.From = From;
                this.To = To;
                this.PartType = PartType;
            }
            public override string ToString()
            {
                string PartTypeString = "";

                switch(PartType)
                {
                    case 'R': PartTypeString = "Resistors"; break;
                    case 'C': PartTypeString = "Capacitors"; break;
                    case 'L': PartTypeString = "Inductors"; break;
                    case 'D': PartTypeString = "Diodes"; break;
                    case 'K': PartTypeString = "Relays"; break;
                    case 'Q': PartTypeString = "Transistors"; break;
                    case 'J': PartTypeString = "Jumpers and Connectors"; break;
                    case 'U': PartTypeString = "ICs"; break;
                    case (char)0: PartTypeString = "entire Layer"; break;
                    default: PartTypeString = "whatever " + PartType + " means"; break;
                }

                return "Change " + From + " to " + To + " for " + PartTypeString;
            }

            public string Execute(string filePath)
            {
                if (!File.Exists(filePath)) return "";

                string find = "";
                string replace = "";

                switch(From)
                {
                    case "Part Value": find = "fp_text reference"; break;
                    case "Part Reference": find = "fp_text value"; break;
                    default: find = From; break;
                }

                replace = To;

                string tempFilePath = Path.GetTempFileName();

                using (var sr = new StreamReader(filePath))
                using (var sw = new StreamWriter(tempFilePath))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        sw.WriteLine(line.Replace(find, replace));
                    }
                }
                
                // delete the original file
                //File.Delete(filePath);

                // rename the temporary file to the original file name
                //File.Move(tempFilePath, filePath);
                
                return tempFilePath;
            }
        }

        public static List<Operation> operations = new List<Operation>();

        public static List<String> OperationNames { get { return operations.Select(x => x.ToString()).ToList(); } }

        public static void AddUniqueOperation(String from, String to, char partType)
        {
            foreach (Operation operation in operations)
            {
                if (operation.From == from && operation.To == to && operation.PartType == partType) return;
            }

            operations.Add(new Operation(from, to, partType));
        }

        public static void RemoveOperation(String DisplayName)
        {
            int index = operations.FindIndex(x => x.ToString() == DisplayName);
            if (index < 0) return;
            operations.RemoveAt(index);
        }

        public static List<String> LayerNames()
        {
            List<String> LayerNames = new List<string>();

            foreach (String filePath in PCBFiles)
            {
                String[] lines = File.ReadAllLines(filePath);

                int ParentheseCount = 0;
                int LineCount;
                for(LineCount = 0; LineCount < lines.Count(); LineCount++)
                {
                    if (lines[LineCount].Contains("(layers")) { ParentheseCount++; break; }
                }

                while (ParentheseCount > 0)
                {
                    LineCount++;
                    if (lines[LineCount].Contains("(")) { ParentheseCount++; }
                    if (lines[LineCount].Contains(")")) { ParentheseCount--; }
                    if (lines[LineCount].Contains("F.Fab") && !LayerNames.Contains("F.Fab")) { LayerNames.Add("F.Fab"); }
                    if (lines[LineCount].Contains("B.Fab") && !LayerNames.Contains("B.Fab")) { LayerNames.Add("B.Fab"); }
                    if (lines[LineCount].Contains("F.CrtYd") && !LayerNames.Contains("F.CrtYd")) { LayerNames.Add("F.CrtYd"); }
                    if (lines[LineCount].Contains("B.CrtYd") && !LayerNames.Contains("B.CrtYd")) { LayerNames.Add("B.CrtYd"); }
                    if (lines[LineCount].Contains("F.SilkS") && !LayerNames.Contains("F.SilkS") ) { LayerNames.Add("F.SilkS"); }
                    if (lines[LineCount].Contains("B.SilkS") && !LayerNames.Contains("B.SilkS")) { LayerNames.Add("B.SilkS"); }
                    if (lines[LineCount].Contains("F.Mask") && !LayerNames.Contains("F.Mask")) { LayerNames.Add("F.Mask"); }
                    if (lines[LineCount].Contains("B.Mask") && !LayerNames.Contains("B.Mask")) { LayerNames.Add("B.Mask"); }
                    if (lines[LineCount].Contains("F.Cu") && !LayerNames.Contains("F.Cu")) { LayerNames.Add("F.Cu"); }
                    if (lines[LineCount].Contains("In1.Cu") && !LayerNames.Contains("In1.Cu")) { LayerNames.Add("In1.Cu"); }
                    if (lines[LineCount].Contains("In2.Cu") && !LayerNames.Contains("In2.Cu")) { LayerNames.Add("In2.Cu"); }
                    if (lines[LineCount].Contains("B.Cu") && !LayerNames.Contains("B.Cu")) { LayerNames.Add("B.Cu"); }
                    if (lines[LineCount].Contains("Edge.Cuts") && !LayerNames.Contains("Edge.Cuts")) { LayerNames.Add("Edge.Cuts"); }
                    if (lines[LineCount].Contains("In3.Cu") && !LayerNames.Contains("In3.Cu")) { LayerNames.Add("In3.Cu"); }
                    if (lines[LineCount].Contains("In4.Cu") && !LayerNames.Contains("In4.Cu")) { LayerNames.Add("In4.Cu"); }
                    if (lines[LineCount].Contains("In5.Cu") && !LayerNames.Contains("In5.Cu")) { LayerNames.Add("In5.Cu"); }
                    if (lines[LineCount].Contains("In6.Cu") && !LayerNames.Contains("In6.Cu")) { LayerNames.Add("In6.Cu"); }
                    if (lines[LineCount].Contains("In7.Cu") && !LayerNames.Contains("In7.Cu")) { LayerNames.Add("In7.Cu"); }
                    if (lines[LineCount].Contains("In8.Cu") && !LayerNames.Contains("In8.Cu")) { LayerNames.Add("In8.Cu"); }
                    if (lines[LineCount].Contains("In9.Cu") && !LayerNames.Contains("In9.Cu")) { LayerNames.Add("In9.Cu"); }
                    if (lines[LineCount].Contains("In10.Cu") && !LayerNames.Contains("In10.Cu")) { LayerNames.Add("In10.Cu"); }
                    if (lines[LineCount].Contains("In11.Cu") && !LayerNames.Contains("In11.Cu")) { LayerNames.Add("In11.Cu"); }
                }

            }
            return LayerNames;
        }

        public static List<char> PartAcronyms()
        {
            List<char> PartAcronyms = new List<char>();
            foreach (String filePath in PCBFiles)
            {
                String[] lines = File.ReadAllLines(filePath);

                foreach (String line in lines)
                {
                    if (line.Contains("(fp_text reference \""))
                    {
                        char PartAcronym = line.Split("(fp_text reference \"")[1][0];
                        if (!PartAcronyms.Contains(PartAcronym)) { PartAcronyms.Add(PartAcronym); }
                    }
                }
            }
            return PartAcronyms;
        }

        private static List<String> PCBFiles = new List<string>();

        public static List<String> PCBNames
        {
            get
            {
                List<String> PCBNames = new List<string>();
                foreach (String filePath in PCBFiles)
                {
                    PCBNames.Add(System.IO.Path.GetFileName(filePath));
                }
                return PCBNames;
            }
        }

        public static void AddFile(String filePath)
        {
            PCBFiles.Add(filePath);
            PCBNames.Add(System.IO.Path.GetFileName(filePath));
        }

        public static void Remove(String filePath)
        {
            PCBFiles.Remove(filePath);
        }
    }
}
