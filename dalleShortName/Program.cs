using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace yyDalleShortName
{
    internal partial class Program
    {
        // A simple program to simplify file names by DALL-E.

        // Example:
        // From: DALL·E 2024-01-05 15.44.53 - A whimsical and surreal wide-format illustration of snowy Moscow, featuring bears drinking vodka. The scene is set in a snowy landscape with iconic Mo.png
        // To: DALL-E 2024-01-05 15.44.53.png

        // It also generates the JPEG version of the image.

        private static readonly string LogFilePath = Path.ChangeExtension (Assembly.GetExecutingAssembly ().Location, ".log");

        private static void WriteLineToLog (string str)
        {
            try
            {
                File.AppendAllText (LogFilePath, (File.Exists (LogFilePath) ? Environment.NewLine : string.Empty) + str + Environment.NewLine, Encoding.UTF8);
            }

            catch
            {
                // Do nothing.
            }
        }

        [GeneratedRegex (@"^(?<FirstPart>DALL·E [0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}\.[0-9]{2}\.[0-9]{2}).+?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
        private static partial Regex FirstPartRegex ();

        static void Main (string [] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine ("Drag & drop the files to rename to the app's executable file.");
                    return;
                }

                foreach (string xFilePath in args)
                {
                    if (Path.IsPathFullyQualified (xFilePath) == false || File.Exists (xFilePath) == false)
                    {
                        Console.WriteLine ("Invalid file path: " + xFilePath);
                        continue;
                    }

                    string xFileNameWithoutExtension = Path.GetFileNameWithoutExtension (xFilePath);

                    Match xMatch = FirstPartRegex ().Match (xFileNameWithoutExtension);

                    if (xMatch.Success == false)
                    {
                        Console.WriteLine ("Invalid file name: " + Path.GetFileName (xFilePath));
                        continue;
                    }

                    string xFirstPart = xMatch.Groups ["FirstPart"].Value.Replace ('·', '-'),
                        xNewFileName = xFirstPart + Path.GetExtension (xFilePath),
                        xNewFilePath = Path.Join (Path.GetDirectoryName (xFilePath), xNewFileName);

                    try
                    {
                        File.Move (xFilePath, xNewFilePath);
                        Console.WriteLine ("Renamed to: " + xNewFileName);

                        // The original file names are the only info that might be needed again.
                        WriteLineToLog ($"Renamed from: {Path.GetFileName (xFilePath)}{Environment.NewLine}To: {xNewFileName}");
                    }

                    catch
                    {
                        Console.WriteLine ("Failed to rename: " + Path.GetFileName (xFilePath));
                        continue;
                    }

                    try
                    {
                        using Image xImage = Image.FromFile (xNewFilePath);
                        xImage.Save (Path.ChangeExtension (xNewFilePath, ".jpg"), ImageFormat.Jpeg); // Default settings.
                        Console.WriteLine ("Generated JPEG version: " + Path.ChangeExtension (xNewFileName, ".jpg"));
                    }

                    catch
                    {
                        Console.WriteLine ("Failed to generate JPEG version: " + Path.ChangeExtension (xNewFileName, ".jpg"));
                        continue;
                    }
                }
            }

            catch (Exception xException)
            {
                // Trimming the following line-break if there is one.
                Console.WriteLine (xException.ToString ().TrimEnd ());
            }

            finally
            {
                Console.Write ("Press any key to close this window: ");
                Console.ReadKey (true);
                Console.WriteLine ();
            }
        }
    }
}
