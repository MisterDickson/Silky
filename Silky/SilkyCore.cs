﻿using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Silky
{
    class SilkyCore
    {
        public ListView Source;
        public static int KiCadVersion = 0; // 0 = not initilised, used to determine if there has been files loaded of different versions
        public static string PartDesignator
        {
            // Version 8 changed the way a part is described and what comes right before its declaration
            get
            {
                return KiCadVersion < 8 ? "(fp_text reference \"" : "(property \"Reference\" \"";
            }
        }

        public static string ValueDesignator
        {
            // Version 8 changed the way a part is described and what comes right before its declaration
            get
            {
                return KiCadVersion < 8 ? "fp_text value" : "property \"Value\"";
            }
        }

        public SilkyCore(ListView source)
        {
            Source = source;
        }
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

                switch (PartType)
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

            public string Execute(string filePath) // Overrides the file on filePath
            {
                if (!File.Exists(filePath)) return "";

                string tempFilePath = Path.GetTempFileName();

                using (var sr = new StreamReader(filePath))
                using (var sw = new StreamWriter(tempFilePath))
                {
                    string line;
                    bool deepEnough = false; // The first couple of lines should not be touched

                    while ((line = sr.ReadLine()!) != null)
                    {
                        string writeLine = line;
                        string layer = "layer \"";

                        if (line.Contains("(footprint")) deepEnough = true;

                        if (From != "Part Value" && From != "Part Reference" && deepEnough)
                        {
                            writeLine = line.Replace(From, To);
                        }
                        else if (From == "Part Value")
                        {
                            if (line.Contains(PartDesignator + PartType))
                            {
                                do
                                {
                                    if (line.Contains(ValueDesignator))
                                    {
                                        if (KiCadVersion > 7)
                                        { // kicad 8 doesn't have the layer in the same line
                                            sw.WriteLine(line);
                                            sw.WriteLine(sr.ReadLine());
                                            line = sr.ReadLine();
                                        }
                                        int layerIndex = line.IndexOf(layer) + layer.Length;
                                        int layerEndIndex = line.IndexOf("\"", layerIndex);
                                        string layerName = line.Substring(layerIndex, layerEndIndex - layerIndex);
                                        writeLine = line.Replace(layerName, To);
                                        break;
                                    }
                                    sw.WriteLine(line);
                                } while ((line = sr.ReadLine()!) != null);
                            }
                        }
                        else if (From == "Part Reference")
                        {
                            if (line.Contains(PartDesignator + PartType))
                            {
                                if (KiCadVersion > 7)
                                { // kicad 8 doesn't have the layer in the same line
                                    sw.WriteLine(line);
                                    sw.WriteLine(sr.ReadLine());
                                    line = sr.ReadLine();
                                }
                                int layerIndex = line.IndexOf(layer) + layer.Length;
                                int layerEndIndex = line.IndexOf("\"", layerIndex);
                                string layerName = line.Substring(layerIndex, layerEndIndex - layerIndex);
                                writeLine = line.Replace(layerName, To);
                            }
                        }
                        sw.WriteLine(writeLine);
                    }
                }

                // delete the original file
                File.Delete(filePath);

                // rename the temporary file to the original file name
                try
                { File.Move(tempFilePath, filePath); }
                catch (Exception ex)
                { Intermediate.SavedFileLogListView.Items.Add(Intermediate.PrepareListViewItem("Something went terribly wrong", "", ex)); }

                return filePath;
            }
        }

        public List<Operation> operations = new List<Operation>();

        public void ExecuteOperationsOnFiles(List<string> filePaths)
        {
            foreach (String filePath in filePaths)
            {
                foreach (Operation operation in operations)
                {
                    operation.Execute(filePath);
                }
            }
        }

        public List<String> OperationNames { get { return operations.Select(x => x.ToString()).ToList(); } }

        public bool AddUniqueOperation(String from, String to, char partType)
        {
            foreach (Operation operation in operations)
            {
                if (operation.From == from && operation.To == to && operation.PartType == partType) return false;
            }

            operations.Add(new Operation(from, to, partType));
            return true;
        }

        public void RemoveOperation(String DisplayName)
        {
            int index = operations.FindIndex(x => x.ToString() == DisplayName);
            if (index < 0) return;
            operations.RemoveAt(index);
        }

        public List<String> LayerNames
        {
            get
            {
                List<String> LayerNames = new List<string>();

                foreach (ListViewItem PCBListViewEntry in Source.Items)
                {
                    String[] lines = File.ReadAllLines(PCBListViewEntry.DataContext.ToString());

                    int ParentheseCount = 0;
                    int LineCount;
                    for (LineCount = 0; LineCount < lines.Count(); LineCount++)
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
                        if (lines[LineCount].Contains("F.SilkS") && !LayerNames.Contains("F.SilkS")) { LayerNames.Add("F.SilkS"); }
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
        }

        public List<char> PartAcronyms
        {
            get
            {
                List<char> PartAcronyms = new List<char>();
                foreach (ListViewItem PCBListViewEntry in Source.Items)
                {
                    String[] lines = File.ReadAllLines(PCBListViewEntry.DataContext.ToString());

                    foreach (String line in lines)
                    {
                        if (line.Contains(PartDesignator))
                        {
                            char PartAcronym = line.Split(PartDesignator)[1][0];
                            if (!PartAcronyms.Contains(PartAcronym)) { PartAcronyms.Add(PartAcronym); }
                        }
                    }
                }
                return PartAcronyms;
            }
        }
    }
}