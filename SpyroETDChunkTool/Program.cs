using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SpyroETDChunkTool
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("SpyroETDChunkTool v0.1 by igorseabra4");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("- Drag CNK files from Spyro: Enter the Dragonfly into the executable to unpack them (GameCube files only)");
            Console.WriteLine("- Otherwise, run the program and you'll be able to choose what to do:");
            Console.WriteLine("- Unpacking extracts all files to a new folder in the same folder of the source CNK file.");
            Console.WriteLine("- Packing rebuilds the archive from all files in a folder, the original archive is not required for this");
            Console.WriteLine("- Make sure correct console is set before unpacking or packing a file from the corresponding version of the game");
            Console.WriteLine();

            if (args.Length > 0)
            {
                Console.WriteLine("Current console mode: " + (Extensions.isPS2 ? "Playstation 2" : "GameCube"));

                foreach (string s in args)
                {
                    Extract(s, out string outputFolder);
                    Console.WriteLine("Done extracting " + s + " to " + outputFolder);
                }
            }
            else
            {
                ShowNoArgsMenu();
            }
        }

        private static void ShowNoArgsMenu()
        {
            ConsoleKey option = ConsoleKey.D4;

            while (option != ConsoleKey.D0)
            {
                Console.WriteLine();
                Console.WriteLine("Current console mode: " + (Extensions.isPS2 ? "Playstation 2" : "GameCube"));
                Console.WriteLine("What do you want to do?");
                Console.WriteLine("0 = exit");
                Console.WriteLine("1 = unpack CNK file");
                Console.WriteLine("2 = pack CNK file from folder");
                Console.WriteLine("3 = switch console mode to " + (Extensions.isPS2 ? "GameCube" : "Playstation 2"));

                option = Console.ReadKey().Key;
                Console.WriteLine();

                switch (option)
                {
                    case ConsoleKey.D1:
                        OpenFileDialog openFile = new OpenFileDialog()
                        {
                            Filter = "CNK Files|*.cnk",
                            Multiselect = true
                        };
                        if (openFile.ShowDialog(new Form() { TopMost = true }) == DialogResult.OK)
                        {
                            foreach (string s in openFile.FileNames)
                            {
                                Extract(s, out string outputFolder);
                                Console.WriteLine("Done extracting " + s + " to " + outputFolder);
                            }
                        }
                        else
                            Console.WriteLine("Operation cancelled.");
                        break;
                    case ConsoleKey.D2:
                        FolderBrowserDialog openFolder = new FolderBrowserDialog();
                        SaveFileDialog saveFile = new SaveFileDialog()
                        {
                            Filter = "CNK Files|*.cnk"
                        };

                        if (openFolder.ShowDialog(new Form() { TopMost = true }) == DialogResult.OK)
                        {
                            if (saveFile.ShowDialog(new Form() { TopMost = true }) == DialogResult.OK)
                            {
                                Create(openFolder.SelectedPath, saveFile.FileName);
                                Console.WriteLine("Done creating " + saveFile.FileName + " from " + openFolder.SelectedPath);
                            }
                            else
                                Console.WriteLine("Operation cancelled.");
                        }
                        else
                            Console.WriteLine("Operation cancelled.");
                        break;
                    case ConsoleKey.D3:
                        Extensions.isPS2 = !Extensions.isPS2;
                        break;
                }
            }
        }

        private static void Extract(string s, out string outputFolder)
        {
            BinaryReader binaryReader = new BinaryReader(new FileStream(s, FileMode.Open));
            binaryReader.BaseStream.Position = 0xC;

            int fileCount = binaryReader.ReadInt32().Switch();

            List<FileEntry> fileEntries = new List<FileEntry>();

            for (int i = 0; i < fileCount; i++)
            {
                fileEntries.Add(new FileEntry
                {
                    Hash = binaryReader.ReadUInt32().Switch(),
                    Offset = binaryReader.ReadUInt32().Switch(),
                    Size = binaryReader.ReadUInt32().Switch(),
                    Unknown = binaryReader.ReadUInt32().Switch()
                });
            }

            outputFolder = Path.GetDirectoryName(s) + "\\" + Path.GetFileNameWithoutExtension(s) + "_out";
            Directory.CreateDirectory(outputFolder);

            foreach (FileEntry file in fileEntries)
            {
                binaryReader.BaseStream.Position = file.Offset;
                file.Data = binaryReader.ReadBytes((int)file.Size);
                File.WriteAllBytes(Path.Combine(outputFolder, file.Hash.ToString("X8")), file.Data);
            }
        }

        private static void Create(string folderPath, string destinationCNK)
        {
            string[] files = Directory.GetFiles(folderPath);

            List<FileEntry> fileEntries = new List<FileEntry>();
            List<byte> data = new List<byte>();

            foreach (string s in files)
            {
                byte[] Data = File.ReadAllBytes(s);
                fileEntries.Add(new FileEntry
                {
                    Hash = Convert.ToUInt32(Path.GetFileName(s), 16),
                    Data = Data,
                    Size = (uint)Data.Length,
                    Unknown = (uint)(Extensions.isPS2 ? 2 : 0x00020000)
                });
            }

            fileEntries = fileEntries.OrderBy(f => f.Hash).ToList();

            int headerLenght = (fileEntries.Count + 2) * 16;
            while (headerLenght % 0x20 != 0 || headerLenght < 0x2000)
                headerLenght++;

            foreach (FileEntry file in fileEntries)
            {
                file.Offset = (uint)(data.Count + headerLenght);
                data.AddRange(file.Data);
                while (data.Count % 0x20 != 0)
                    data.Add(0);
            }

            BinaryWriter binaryWriter = new BinaryWriter(new FileStream(destinationCNK, FileMode.Create));
            binaryWriter.Write(1);
            binaryWriter.Write(1);
            binaryWriter.Write(0);
            binaryWriter.Write(fileEntries.Count.Switch());

            foreach (FileEntry file in fileEntries)
            {
                binaryWriter.Write(file.Hash.Switch());
                binaryWriter.Write(file.Offset.Switch());
                binaryWriter.Write(file.Size.Switch());
                binaryWriter.Write(file.Unknown.Switch());
            }

            binaryWriter.BaseStream.Position = headerLenght;
            binaryWriter.Write(data.ToArray());

            binaryWriter.Close();
        }
    }
}
