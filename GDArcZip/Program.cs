using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Windows.Forms;

using Utils;

namespace GDArcZip
{
    internal class Program
    {

        #region common stuff

        private static readonly string CWD = Application.StartupPath;

        private static void Exit(string msg, int errCode = 0)
        {
            Console.WriteLine(msg);
            System.Threading.Thread.Sleep(5000);
            Environment.Exit(errCode);
        }

        #endregion

        private readonly static string CFG = Path.Combine(CWD, "GDArcZip.cfg");
        private static Dictionary<string, string> cfgDict;

        static void Main(string[] args)
        {
            cfgDict = LoadCFGDict(CFG);
            SaveCFGDict(CFG, cfgDict);
            if (args.Length == 0)
                Exit($"Drag and drop text_xx.arc or text_xx.zip onto the exe");
            foreach(var file in args)
                ProcessFile(file);
            Exit($"Done");
        }

        private static void ProcessFile(string inFile)
        {
            Console.WriteLine($"processing file: {inFile}");
            if (!File.Exists(inFile))
                Exit($"File '{inFile}' not found");

            var archTool = $"{cfgDict["gdpath"]}/ArchiveTool.exe";
            var inFilePath = Path.GetDirectoryName(inFile);
            var inFileName = Path.GetFileNameWithoutExtension(inFile);
            var inFileExt = Path.GetExtension(inFile).ToLower();
            string tmpPath = Funcs.GetTempFileName();
            Directory.CreateDirectory(tmpPath);
            string tmpPath2 = Funcs.GetTempFileName();
            Directory.CreateDirectory(tmpPath2);
            switch (inFileExt)
            {
                case ".arc":
                    {
                        var inFileTemp = Path.Combine(tmpPath2, $"{inFileName}.arc");
                        File.Copy(inFile, inFileTemp);
                        RunProcess(archTool, $"\"{inFileTemp}\" -extract \"{tmpPath}\"");
                        var outFile = Path.Combine(inFilePath, $"{inFileName}.zip");
                        if (File.Exists(outFile))
                            File.Delete(outFile);
                        ZipFile.CreateFromDirectory(Path.Combine(tmpPath, inFileName.ToLower()), outFile);
                    }
                    break;
                case ".zip":
                    {
                        var outFile = Path.Combine(inFilePath, $"{inFileName}.arc");
                        var outFileTemp = Path.Combine(tmpPath2, $"{inFileName}.arc");
                        ZipFile.ExtractToDirectory(inFile, tmpPath);
                        foreach (var f in Directory.GetFiles(tmpPath, "*.*", SearchOption.AllDirectories))
                            RunProcess(archTool, $"\"{outFileTemp}\" -replace \"{f.Substring(tmpPath.Length + 1)}\" \"{tmpPath}\" 9");
                        if (File.Exists(outFile))
                            File.Delete(outFile);
                        File.Move(outFileTemp, outFile);
                    }
                    break;
                default:
                    Console.WriteLine($"Unsupported file extension {inFileExt}");
                    break;
            }
            Directory.Delete(tmpPath, true);
            Directory.Delete(tmpPath2, true);
        }

        private static Dictionary<string, string> LoadCFGDict(string cfgFile)
        {
            if (!File.Exists(cfgFile))
                File.WriteAllText(cfgFile, "");
            var cfgDict = Funcs.ReadDictionaryFromFile(cfgFile, '=');
            var gdPath = "";
            if (cfgDict.ContainsKey("gdpath") && !Directory.Exists(cfgDict["gdpath"]))
                cfgDict.Remove("gdpath");
            while (!cfgDict.ContainsKey("gdpath"))
            {
                gdPath = Prompt("Enter GD Path:", gdPath);
                if (File.Exists(Path.Combine(gdPath, "ArchiveTool.exe")))
                    cfgDict.Add("gdpath", gdPath);
            }
            return cfgDict;
        }

        private static void SaveCFGDict(string cfgFile, Dictionary<string, string> cfgDict)
        {
            var lines = new List<string>();
            foreach (var kvp in cfgDict)
                lines.Add($"{kvp.Key}={kvp.Value}");
            File.WriteAllLines(cfgFile, lines);
        }

        private static void RunProcess(string fileName, string arguments)
        {
            try
            {
                using (Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                })
                {
                    p.Start();
                    while (!p.StandardOutput.EndOfStream)
                        Console.WriteLine(p.StandardOutput.ReadLine());

                    p.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static string Prompt(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button accept = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            accept.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(accept);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = accept;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
