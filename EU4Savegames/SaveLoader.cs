using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEParser;
using Ionic.Zip;

namespace EU4Savegames
{
    public static class SaveLoader
    {
        private static readonly Encoding encoding = Game.EU4.Encoding;

        public static async Task Decode(string inputPath, string outputPath, bool enforceCompression = false)
        {
            string output = "";

            MemoryStream stream;
            ZipEntry rnw = null;

            if (IsCompressedSavegame(inputPath))
            {
                // Uncompressing savegame
                stream = (MemoryStream)await unpackToStream(inputPath);

                using (var zip = ZipFile.Read(inputPath))
                {
                    rnw = zip["rnw.zip"];
                }
            }
            else
            {
                using (var fs = System.IO.File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stream = new MemoryStream(4096);
                    fs.CopyTo(stream);
                }
            }

            if (stream == null) throw new Exception("Error unpacking savegame file or the file is not a supported binary savegame file.");

            if (!IsBinarySavegame(stream))
            {
                throw new Exception("The specified file is not a supported binary savegame file.");
            }

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            var file = new BinaryFile(stream, Game.EU4, encoding);
            file.Decoder.EnforceDateDatatype = true;
            await file.ParseAsync();

            file.CleanUp("UNKNOWN_*", true);

            output = file.Export();
            output = output.Replace("\r\n", "\n"); // currently, for all the games Unix Lf line endings are used

            // WRITING TO DISK
            List<int> removedPos = new List<int>();
            List<ushort> removedLen = new List<ushort>();

            using (StreamWriter tw = new StreamWriter(outputPath, false, encoding, 4096))
            {
                char[] outputArray = output.ToCharArray();
                tw.Write("eu4txt");

                int pos = 6;

                for (int i = 0; i < removedPos.Count; i++)
                {
                    if (removedPos[i] - pos < 1)
                    {
                    }
                    else
                    {
                        tw.Write(outputArray, pos, removedPos[i] - pos);
                    }
                    pos = removedPos[i] + removedLen[i] + 1;
                }
                if (pos < output.Length)
                    tw.Write(outputArray, pos, output.Length - pos);
            }

            //Random New World or zipped output
            //if (rnw != null || enforceCompression)
            //{
            //    //using (StreamWriter tw = new StreamWriter(Path.Combine(Path.GetDirectoryName(outputPath), "meta"), false, ANSI, 4096))
            //    //{
            //    //    string metaOutput = output.Substring(0, output.IndexOf("\n", output.IndexOf("not_observer")));
            //    //    metaOutput = metaOutput.Replace("EU4bin", "EU4txt");
            //    //    metaOutput = metaOutput.Remove(metaOutput.IndexOf("file_name="),
            //    //        metaOutput.IndexOf("multi_player=", metaOutput.IndexOf("file_name=")) - metaOutput.IndexOf("file_name="));
            //    //    metaOutput += "\n";
            //    //    metaOutput += checksum;
            //    //    tw.Write(metaOutput);
            //    //}
            //    if (rnw != null)
            //    {
            //        if (File.Exists(Path.Combine(Path.GetDirectoryName(outputPath), "rnw.zip")))
            //        {
            //            if (MessageBox.Show("There is an existing 'rnw.zip' file in the output location. Do you want to overwrite it and continue the decoding process?", "Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            //                return;
            //        }

            //        File.Delete(Path.Combine(Path.GetDirectoryName(outputPath)));
            //        var file = File.OpenWrite(Path.Combine(Path.GetDirectoryName(outputPath)));
            //        rnw.Open().CopyTo(file);
            //        file.Close();
            //    }

            //    string zipTempPath = Path.Combine(Path.GetDirectoryName(outputPath), Path.GetFileNameWithoutExtension(outputPath) + ".zip");
            //    using (var zip = new ZipArchive(File.OpenRead(zipTempPath)))
            //    {
            //        zip.AddFile(outputPath, "");
            //        //zip.AddFile(Path.Combine(Path.GetDirectoryName(outputPath), "meta"), "");
            //        if (rnw != null) zip.AddFile(Path.Combine(Path.GetDirectoryName(outputPath), "rnw.zip"), "");
            //        zip.Save();
            //    }

            //    File.Delete(outputPath);
            //    //File.Delete(Path.Combine(Path.GetDirectoryName(outputPath), "meta"));
            //    File.Delete(Path.Combine(Path.GetDirectoryName(outputPath), "rnw.zip"));

            //    // Try to rename the zip file
            //    int failsafe = 10;
            //    while (true)
            //    {
            //        try
            //        {
            //            File.Move(zipTempPath, outputPath);
            //            break; // success!
            //        }
            //        catch
            //        {
            //            if (--failsafe == 0)
            //            {
            //                MessageBox.Show("The decoding was successful but the resulting compressed file: " + zipTempPath + " is locked and cannot be renamed. You can try to rename it manually to " + outputPath + ".", "File locked", MessageBoxButton.OK, MessageBoxImage.Error);
            //                return;
            //            }
            //            else Thread.Sleep(100);
            //        }
            //    }
            //}
        }

        public static bool IsBinarySavegame(byte[] raw)
        {
            string prefix = encoding.GetString(raw, 0, 6);

            return prefix.ToLowerInvariant() == "eu4bin";
        }

        public static bool IsBinarySavegame(Stream stream)
        {
            byte[] raw = new byte[7];
            var pos = stream.Position;

            stream.Position = 0;
            stream.Read(raw, 0, 7);
            stream.Position = pos;

            return IsBinarySavegame(raw);
        }

        public static bool IsCompressedSavegame(string path)
        {
            using (BinaryReader br = new BinaryReader(System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                byte[] code = br.ReadBytes(2);
                return (code[0] == 80 && code[1] == 75);
            }
        }

        private static async Task<Stream> unpackToStream(string path)
        {
            // Unzip the save file
            using (var zip = ZipFile.Read(path))
            {
                var e = zip[Path.GetFileName(path)] ?? zip["game.eu4"];
                MemoryStream ms = new MemoryStream();
                await Task.Run(() =>
                {
                    int failsafe = 3;
                    while (true)
                    {
                        try
                        {
                            e.OpenReader().CopyTo(ms);
                            break; // success!
                        }
                        catch
                        {
                            if (--failsafe == 0) throw new Exception("Cannot unpack the save file. Check if the file is not in use.");
                            else Thread.Sleep(100);
                        }
                    }
                });
                return ms;
            }
        }
    }
}