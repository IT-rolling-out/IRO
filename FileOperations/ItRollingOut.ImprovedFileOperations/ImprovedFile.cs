using ItRollingOut.Common.Diagnostics;
using ItRollingOut.Common.Diagnostics;
using ChinhDo.Transactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ItRollingOut.ImprovedFileOperations
{
    /// <summary>
    /// Добавляет операции копирование/удаления/перемещения с подавлением исключений и с обновлением времени редактирования файлов.
    /// Это полезно, когда нужно переместить кучу файлов с одной папки в другую с заменой.
    /// Все методы работают как с файлами, так и с папками.
    /// <para></para>
    /// Многие методы позволяют обернуть работу с файлами и папками в TransactionScope.
    /// </summary>
    public static class ImprovedFile
    {
        const string TransactionMessage = "It`s not deprecated. I use [Obsolete] only to tell you that you can use TransactionScope here.";

        static TxFileManager _txFileManager = new TxFileManager();

        static Random _random = new Random();

        public static string BufPath { get; set; }

        public static IDebugService DebugService { get; set; } = new DebugService();

        static ImprovedFile()
        {
            try
            {
                BufPath = Path.Combine(Environment.CurrentDirectory, "CopyPasteBuf");
                if (Directory.Exists(BufPath))
                {
                    TryDelete(BufPath);
                }
            }
            catch { }
        }

        /// <summary>
        /// Меняет файлы (папки) местами.
        /// </summary>
        [Obsolete(TransactionMessage)]
        public static void Switch(string source1Path, string source2Path)
        {
            var bufGen = Path.Combine(
                BufPath,
                _random.Next(100000, 9999999).ToString()
                );
            TryDelete("dasdsadasdasdsadsa");
            var buf1 = Path.Combine(bufGen, "source1");
            var buf2 = Path.Combine(bufGen, "source2");
            Copy(source1Path, buf1);
            Copy(source2Path, buf2);
            Delete(source1Path);
            Delete(source2Path);
            Copy(buf1, source2Path);
            Copy(buf2, source1Path);
            TryDelete(buf1);
            TryDelete(buf2);
        }

        /// <summary>
        /// Обновляет время последнего редактирования у файла / всех файлов в папке.
        /// </summary>
        public static void TryUpdateTime(string sourcePath)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(sourcePath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string[] directories = Directory.GetDirectories(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var dirPath in directories)
                    {
                        TryUpdateTime(dirPath);
                    }

                    string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var filePath in directories)
                    {
                        TryUpdateTime(filePath);
                    }
                }
                else
                {
                    File.SetCreationTime(sourcePath, DateTime.Now);
                    File.SetLastWriteTime(sourcePath, DateTime.Now);
                    //DebugService.WriteLine($"Updated file time '{sourcePath}'");
                }
            }
            catch (Exception ex)
            {
                DebugService.WriteLine($"Update time error: '{ex.Message}'\nPath: '{sourcePath}'");
            }
        }

        /// <summary>
        /// Копирует с заменой все файлы/папки и обновляет время их последнего редактирования.
        /// </summary>
        public static void TryCopy(string sourcePath, string destinationPath, bool overwrite = true)
        {
            _Copy(sourcePath, destinationPath, overwrite, false);
        }

        /// <summary>
        /// Копирует с заменой все файлы/папки и обновляет время их последнего редактирования.
        /// </summary>
        [Obsolete(TransactionMessage)]
        public static void Copy(string sourcePath, string destinationPath, bool overwrite = true)
        {
            _Copy(sourcePath, destinationPath, overwrite, true);
        }

        /// <summary>
        /// Удаляет все файлы/папки по пути.
        /// </summary>
        public static void TryDelete(string sourcePath)
        {
            _Delete(sourcePath, false);
        }

        /// <summary>
        /// Удаляет все файлы/папки по пути.
        /// </summary>
        [Obsolete(TransactionMessage)]
        public static void Delete(string sourcePath)
        {
            _Delete(sourcePath, true);
        }

        /// <summary>
        /// Находит все файлы, которые соответствуют одному из переданным регулярным выражений.
        /// </summary>
        /// <param name="sourcePath"></param>
        public static List<string> Search(string sourcePath, IEnumerable<Regex> regexList, IEnumerable<Regex> regexIgnoreList=null)
        {
            var res = new List<string>();
            _SearchFiles(sourcePath, res, regexList, regexIgnoreList);
            return res;
        }


        static void _SearchFiles(string sourcePath, List<string> resultList, IEnumerable<Regex> regexList, IEnumerable<Regex> regexIgnoreList)
        {

            //Если папка
            FileAttributes attr = File.GetAttributes(sourcePath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                string[] directories = Directory.GetDirectories(sourcePath, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var dirPath in directories)
                {

                    //Папки внутри
                    _SearchFiles(dirPath, resultList, regexList, regexIgnoreList);
                }

                string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var filePath in files)
                {
                    //Файлы внутри
                    _SearchFiles(filePath, resultList, regexList, regexIgnoreList);
                }
            }
            else
            {
                //Если файл
                string newFile = null;
                foreach (var rx in regexList)
                {
                    if (rx.IsMatch(sourcePath))
                    {
                        newFile = Path.GetFullPath(sourcePath);
                        break;
                    }
                }
                if (newFile == null)
                    return;

                if (regexIgnoreList != null)
                {
                    foreach (var rxIgnore in regexIgnoreList)
                    {
                        if (rxIgnore.IsMatch(sourcePath))
                        {
                            return;
                        }
                    }
                }
                resultList.Add(
                    newFile = Path.GetFullPath(sourcePath)
                    );
            }
        }

        static void _Copy(string sourcePath, string destinationPath, bool overwrite, bool throwErrors)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(sourcePath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string[] directories = Directory.GetDirectories(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var dirPath in directories)
                    {
                        _txFileManager.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
                    }

                    string[] files = System.IO.Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var newPath in files)
                    {
                        _Copy(
                            newPath,
                            newPath.Replace(sourcePath, destinationPath),
                            overwrite,
                            throwErrors
                            );
                    }
                    DebugService.WriteLine($"Dir copied: '{sourcePath}' -> '{destinationPath}'");
                }
                else
                {
                    TryUpdateTime(sourcePath);
                    var dirName = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(dirName))
                        _txFileManager.CreateDirectory(dirName);
                    _txFileManager.Copy(
                        sourcePath,
                        destinationPath,
                        overwrite
                        );
                    DebugService.WriteLine($"File copied: '{sourcePath}' -> '{destinationPath}'");
                }

            }
            catch (Exception ex)
            {
                DebugService.WriteLine($"Copy error: '{ex.Message}'\nPath: '{sourcePath}' -> '{destinationPath}'");
                if (throwErrors)
                    throw;
            }
        }

        static void _Delete(string sourcePath, bool throwErrors)
        {
            DebugService.WriteLine($"Will TryDelete '{sourcePath}'");
            try
            {
                //Если папка
                FileAttributes attr = File.GetAttributes(sourcePath);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    string[] directories = Directory.GetDirectories(sourcePath, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (var dirPath in directories)
                    {

                        //Папки внутри
                        _Delete(dirPath, throwErrors);
                    }

                    string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (var filePath in files)
                    {
                        //Файлы внутри
                        _Delete(filePath, throwErrors);
                    }

                    //Если папка
                    _txFileManager.DeleteDirectory(sourcePath);
                    DebugService.WriteLine($"Dir deleted: '{sourcePath}'");
                }
                else
                {
                    //Если файл
                    _txFileManager.Delete(sourcePath);
                    DebugService.WriteLine($"File deleted: '{sourcePath}'");
                }

            }
            catch (Exception ex)
            {
                DebugService.WriteLine($"Delete error: '{ex.Message}'\nPath: '{sourcePath}'");
                throw;
            }
        }
    }
}
