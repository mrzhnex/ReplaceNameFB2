using System;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace ReplaceNameFB2
{
    class Program
    {
        static string version = "4.0.1";
        static void Main(string[] args)
        {
            string Output = "";
            Console.OutputEncoding = Encoding.GetEncoding("Windows-1251");
            Console.WriteLine("Логирование началось: ");
            Output = Output + "Логирование началось: " + "\n";
            Console.WriteLine("Версия программы: '" + version + "'");
            Output = Output + "Версия программы: '" + version + "'" + "\n";
            Global.GenreTranslate = SetGenreFromFile("genres.list");
            Console.WriteLine("Жанров установлено: " + Global.GenreTranslate.Count);
            Output = Output + "Жанров установлено: " + Global.GenreTranslate.Count + "\n";
            XmlSerializer formatter = new XmlSerializer(typeof(Settings));
            Settings setting = new Settings(true, true);
            if (!File.Exists("settings.xml"))
            {
                using (FileStream fs = new FileStream(Global.SettingsFile, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, setting);
                    Console.WriteLine("Файл настроек создан" + "\n");
                    Output = Output + "Файл настроек создан" + "\n";
                }
            }
            try
            {
                using (FileStream fs = new FileStream(Global.SettingsFile, FileMode.OpenOrCreate))
                {
                    setting = (Settings)formatter.Deserialize(fs);
                    Console.WriteLine("Файл настроек загружен: '" + Global.SettingsFile + "'");
                    Output = Output + "Файл настроек загружен: '" + Global.SettingsFile + "'" + "\n";
                    Console.WriteLine("Вывод информации о файлах: " + setting.Output);
                    Output = Output + "Вывод информации о файлах: " + setting.Output + "\n";
                    Console.WriteLine("Вывод ошибок: " + setting.ErrorOutput + "\n");
                    Output = Output + "Вывод ошибок: " + setting.ErrorOutput + "\n";
                }
            }
            catch (InvalidOperationException)
            {
                File.Delete("settings.xml");
                using (FileStream fs = new FileStream("settings.xml", FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, setting);
                    Console.WriteLine("Файл настроек восстановлен" + "\n");
                    Output = Output + "Файл настроек восстановлен" + "\n";
                }
                using (FileStream fs = new FileStream("settings.xml", FileMode.OpenOrCreate))
                {
                    setting = (Settings)formatter.Deserialize(fs);
                    Console.WriteLine("Файл настроек загружен: '" + Global.SettingsFile + "'");
                    Output = Output + "Файл настроек загружен: '" + Global.SettingsFile + "'" + "\n";
                    Console.WriteLine("Вывод информации о файлах: " + setting.Output);
                    Output = Output + "Вывод информации о файлах: " + setting.Output + "\n";
                    Console.WriteLine("Вывод ошибок: " + setting.ErrorOutput + "\n");
                    Output = Output + "Вывод ошибок: " + setting.ErrorOutput + "\n";
                }
            }

            Console.Write("Введите путь от исполняемого файла программы до папки с .fb2 файлами, требующими изменения: ");
            string folder = Console.ReadLine();
            Output = Output + "Введите путь от исполняемого файла программы до папки с .fb2 файлами, требующими изменения: " + folder + "\n";
            Console.WriteLine("\n");
            if (!Directory.Exists(folder))
            {
                Console.WriteLine("Путь '" + folder + "' не существует. Нажмите любую клавишу, чтобы выйти из программы...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            string[] files = Directory.GetFiles(folder);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(Global.OldExpansion))
                {
                    Encoding enc;
                    try
                    {
                        enc = RightEncoding[GetEncoding(files[i])];
                        if (setting.Output)
                            Console.WriteLine("Кодировка файла '" + files[i] + "' : '" + GetEncoding(files[i]) + "'");
                        Output = Output + "Кодировка файла '" + files[i] + "' : '" + GetEncoding(files[i]) + "'" + "\n";
                    }
                    catch (KeyNotFoundException)
                    {
                        if (setting.ErrorOutput)
                            Console.WriteLine("Ошибка кодировки (отсутствует в списке): '" + GetEncoding(files[i]) + "'");
                        Output = Output + "Ошибка кодировки (отсутствует в списке): '" + GetEncoding(files[i]) + "'" + "\n";
                        continue;
                    }

                    bool genreSet = false;
                    if (setting.Output)
                        Console.WriteLine("Чтение данных с файла: " + files[i]);
                    Output = Output + "Чтение данных с файла: " + files[i] + "\n";

                    string directory;


                    if (Global.Debug)       
                    {
                        Console.WriteLine("directory: '" + directory + "'");
                    }
                    if (!Directory.Exists(@directory))
                    {
                        Console.WriteLine("directory2: '" + directory + "'");
                        Directory.CreateDirectory(@directory);      
                    }

                    try
                    {
                        File.Move(Path.GetFullPath(files[i]), Path.GetFullPath(Path.Combine(directory, GetNewName(last, first, middle, book, year))));
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (setting.ErrorOutput)
                            Console.WriteLine("Ошибка переименовывания файла (ошибка директории) '" + files[i] + "'" + "\n");
                        continue;
                    }
                    if (setting.Output)
                        Console.WriteLine("Файл успешно изменен: '" + Path.Combine(directory, GetNewName(last, first, middle, book, year)) + "'\n");
                    Output = Output + "Файл успешно изменен: '" + Path.Combine(directory, GetNewName(last, first, middle, book, year)) + "'\n" + "\n";
                }
            }

            Console.WriteLine("Готово. Консоль может быть закрыта.");
            Output = Output + "Готово. Консоль может быть закрыта." + "\n";
            if (CreateOutput(Output))
            {
                if (setting.Output)
                    Console.WriteLine("Файл логирования создан");
            }
            else
            {
                if (setting.Output)
                    Console.WriteLine("Произошла ошибка при создании файла логирования");
            }
            Console.ReadKey();
        }

        public static string ClearFromWrongSymbols(string source)
        {
            foreach (KeyValuePair<string, string> kvp in Global.WrongAndGoodSymbols)
            {
                if (source.Contains(kvp.Key))
                {
                    source = source.Replace(kvp.Key, kvp.Value);
                    if (Global.Debug)
                        Console.WriteLine("clear: " + kvp.Key);
                }
            }
            foreach (char c in new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))
            {
                source = source.Replace(c.ToString(), "");
            }
            if (source.Contains(" "))
            {
                if (source.IndexOf(" ") == source.Length - 1)
                {
                    source.Remove(source.Length - 1);
                }
            }
            return source;
        }

        public static string GetNewName(string last, string first, string middle, string book, string year)
        {
            return last + " " + first + " " + middle + Global.symbol + book + " " + Global.left_bracket + year + Global.right_bracket + Global.OldExpansion;
        }

        public static string GetBetween(string all, string first, string last)
        {
            all = all.Remove(0, all.IndexOf(first) + first.Length);
            all = all.Remove(all.IndexOf(last));
            return all.Replace(first, "").Replace(last, "").Replace(" ", string.Empty);
        }

        public static string GetBetweenWithoutRemovedEmpty(string all, string first, string last)
        {
            all = all.Remove(0, all.IndexOf(first) + first.Length);
            all = all.Remove(all.IndexOf(last));
            string temp = all.Replace(first, "").Replace(last, "");
            while (temp.IndexOf(" ") == 0)
            {
                temp = temp.Substring(1);
            }
            if (temp.LastIndexOf(" ") == temp.Length)
            {
                temp = temp.Substring(temp.Length - 1);
            }
            return temp;
        }

        public static string[] GetFewGenre(string genre)
        {
            return genre.Split(new char[] { '_' });
        }

        public static bool CreateOutput(string text)
        {
            if (!Directory.Exists(Global.OutputFolder))
            {
                Directory.CreateDirectory(Global.OutputFolder);
            }
            string[] files = Directory.GetFiles(Global.OutputFolder);
            if (files.Length > 0)
            {
                if (int.TryParse(files[files.Length - 1].Remove(files[files.Length - 1].Length - 4).Remove(0, Global.OutputFolder.Length + 1), out int index))
                {
                    index++;
                    StreamWriter sw = new StreamWriter(Path.Combine(@Global.OutputFolder + @"/" + @index + @".txt"));
                    sw.WriteLine(text);
                    sw.Close();
                    return true;
                }
                else
                {
                    Console.WriteLine(files[files.Length - 1].Remove(files[files.Length - 1].Length - 4).Remove(0, Global.OutputFolder.Length + 1));
                    return false;
                }
            }
            else
            {
                StreamWriter sw = new StreamWriter(Path.Combine(@Global.OutputFolder + @"/" + @"1.txt"));
                sw.WriteLine(text);
                sw.Close();
                return true;
            }
        }

        public static string GetEncoding(string filename)
        {
            string oldEncodingName = "";
            var bytes = File.ReadAllBytes(filename);

            string text = null;
            bool isASCII = true;

            foreach (byte b in bytes)
            {
                if (b < 128) continue;
                isASCII = false;
                break;
            }

            if (isASCII)
            {
                oldEncodingName = "ASCII";
                text = Encoding.ASCII.GetString(bytes);
            }
            else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                oldEncodingName = "UTF-8 с BOM";
                if (bytes.Length > 3) text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                else text = "";
            }
            else if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0 && bytes[3] == 0)
            {
                oldEncodingName = "UTF-32 LE";
                if (bytes.Length > 4) text = Encoding.UTF32.GetString(bytes, 4, bytes.Length - 4);
                else text = "";
            }
            else if (bytes.Length >= 4 && bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            {
                oldEncodingName = "UTF-32 BE";
                if (bytes.Length > 4) text = new UTF32Encoding(true, false).GetString(bytes, 4, bytes.Length - 4);
                else text = "";
            }
            else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                oldEncodingName = "UTF-16 LE";
                if (bytes.Length > 2) text = Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
                else text = "";
            }
            else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                oldEncodingName = "UTF-16 BE";
                if (bytes.Length > 2) text = Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
                else text = "";
            }
            else foreach (var enc in inputEncodings)
                {
                    try { text = enc.GetString(bytes); }
                    catch (Exception) { continue; }
                    oldEncodingName = enc.EncodingName;
                    break;
                }

            if (text == null)
            {
                oldEncodingName = "Unknown";
            }

            return oldEncodingName;
        }

        public static Encoding[] inputEncodings = new Encoding[]
        {
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
            Encoding.GetEncoding(1251, new EncoderExceptionFallback(), new DecoderExceptionFallback()),
        };

        static string[] encodingNames = new string[]
        {
            "UTF-8 без BOM",
            "UTF-8 с BOM",
            "UTF-16 LE",
            "UTF-16 BE",
            "UTF-32 LE",
            "UTF-32 BE",
            "CP1251",
            "KOI8-R",
            "KOI8-U"
        };
        public static Dictionary<string, string> SetGenreFromFile(string file)
        {
            Dictionary<string, string> tempdir = new Dictionary<string, string>();
            if (!File.Exists(file))
            {
                Console.WriteLine("Файл '" + file + "' не существует. Нажмите любую клавишу, чтобы выйти из программы...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                try
                {
                    tempdir.Add(line.Split('=')[0], line.Split('=')[1]);
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR: key: " + line.Split('=')[0] + " value: " + line.Split('=')[1]);
                    Console.ReadKey();
                }
            }
            return tempdir;
        }

        public static Dictionary<string, Encoding> RightEncoding = new Dictionary<string, Encoding>()
        {
            {"Unknown", Encoding.UTF8 },
            {"Unicode (UTF-8)", Encoding.UTF8 },
            {"UTF-8 без BOM", Encoding.UTF8},
            {"UTF-8 с BOM", Encoding.UTF8},
            {"UTF-16 LE", Encoding.UTF8},
            {"UTF-16 BE", Encoding.UTF8},
            {"UTF-32 LE", Encoding.UTF32},
            {"UTF-32 BE", Encoding.UTF32},
            {"Кириллица (Windows)", Encoding.GetEncoding("Windows-1251") },
            {"CP1251", Encoding.GetEncoding("Windows-1251")},
            {"KOI8-R", Encoding.GetEncoding("koi8r")},
            {"KOI8-U", Encoding.GetEncoding("koi8-u")},
            {"ASCII", Encoding.ASCII }
        };
    }

    public static class Global
    {
        public static readonly int MaxLenghtFile = 259;
        public static readonly string OutputFolder = "Output";
        public static readonly string SettingsFile = "settings.xml";
        public static readonly bool Debug = false;
        public static readonly string symbol = "=";
        public static readonly string left_bracket = "(";
        public static readonly string right_bracket = ")";
        public static readonly string OldExpansion = ".fb2";

        public static readonly string genre1 = "<genre>";
        public static readonly string genre2 = "</genre>";

        public static readonly string last_name_1 = "<last-name>";
        public static readonly string last_name_2 = "</last-name>";

        public static readonly string first_name_1 = "<first-name>";
        public static readonly string first_name_2 = "</first-name>";

        public static readonly string middle_name_1 = "<middle-name>";
        public static readonly string middle_name_2 = "</middle-name>";

        public static readonly string book_1 = "<book-title>";
        public static readonly string book_2 = "</book-title>";

        public static readonly string year_1 = "<year>";
        public static readonly string year_2 = "</year>";

        public static readonly Dictionary<string, string> WrongAndGoodSymbols = new Dictionary<string, string>()
        {
            { ".", "," },
            { "|", "-" },
            { "*", "-" },
            { ":", "," },
            { "?", "," },
            { "!", "," },
            { "%", "проценты" },
            { "/", "-" },
            { @"\", "-" },
            { "\n", "-" }

        };

        public static Dictionary<string, string> GenreTranslate = new Dictionary<string, string>()
        {
           
        };
    }

    [Serializable]
    public class Settings
    {
        public bool ErrorOutput { get; set; }
        public bool Output { get; set; }
        public Settings() { }

        public Settings(bool output, bool errorOuput)
        {
            this.Output = output;
            this.ErrorOutput = errorOuput;
        }
    }
}
