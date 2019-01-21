using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace IRO.FileIO.ImprovedFileOperations
{
    /// <summary>
    /// Добавляет операции копирование/удаления/перемещения с подавлением исключений и с обновлением времени редактирования файлов.
    /// Это полезно, когда нужно переместить кучу файлов с одной папки в другую с заменой.
    /// Все методы работают как с файлами, так и с папками.
    /// <para></para>
    /// Многие методы позволяют обернуть работу с файлами и папками в TransactionScope.
    /// </summary>
    public class ImprovedFile
    {
        readonly Action<string> _logger;

        public ImprovedFile(Action<string> logger=null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Обновляет время последнего редактирования у файла / всех файлов в папке.
        /// </summary>
        public void TryUpdateTime(string sourcePath)
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
                    //Log($"Updated file time '{sourcePath}'");
                }
            }
            catch (Exception ex)
            {
                Log($"Update time error: '{ex.Message}'\nPath: '{sourcePath}'");
            }
        }

        /// <summary>
        /// Копирует с заменой все файлы/папки и обновляет время их последнего редактирования.
        /// </summary>
        public void TryCopy(string sourcePath, string destinationPath, bool overwrite = true)
        {
            _Copy(sourcePath, destinationPath, overwrite, false);
        }

        /// <summary>
        /// Копирует с заменой все файлы/папки и обновляет время их последнего редактирования.
        /// </summary>
        public void Copy(string sourcePath, string destinationPath, bool overwrite = true)
        {
            _Copy(sourcePath, destinationPath, overwrite, true);
        }

        /// <summary>
        /// Удаляет все файлы/папки по пути.
        /// </summary>
        public void TryDelete(string sourcePath)
        {
            _Delete(sourcePath, false);
        }

        /// <summary>
        /// Удаляет все файлы/папки по пути.
        /// </summary>
        public void Delete(string sourcePath)
        {
            _Delete(sourcePath, true);
        }

        /// <summary>
        /// Находит все файлы, которые соответствуют одному из переданным регулярным выражений.
        /// </summary>
        /// <param name="sourcePath"></param>
        public List<string> Search(string sourcePath, IEnumerable<Regex> regexList, IEnumerable<Regex> regexIgnoreList=null)
        {
            var res = new List<string>();
            _SearchFiles(sourcePath, res, regexList, regexIgnoreList);
            return res;
        }


        void _SearchFiles(string sourcePath, List<string> resultList, IEnumerable<Regex> regexList, IEnumerable<Regex> regexIgnoreList)
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

        void _Copy(string sourcePath, string destinationPath, bool overwrite, bool throwErrors)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(sourcePath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string[] directories = Directory.GetDirectories(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var dirPath in directories)
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
                    }

                    string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (var newPath in files)
                    {
                        _Copy(
                            newPath,
                            newPath.Replace(sourcePath, destinationPath),
                            overwrite,
                            throwErrors
                            );
                    }
                    Log($"Dir copied: '{sourcePath}' -> '{destinationPath}'");
                }
                else
                {
                    TryUpdateTime(sourcePath);
                    var dirName = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);
                    File.Copy(
                        sourcePath,
                        destinationPath,
                        overwrite
                        );
                    Log($"File copied: '{sourcePath}' -> '{destinationPath}'");
                }

            }
            catch (Exception ex)
            {
                Log($"Copy error: '{ex.Message}'\nPath: '{sourcePath}' -> '{destinationPath}'");
                if (throwErrors)
                    throw;
            }
        }

        void _Delete(string sourcePath, bool throwErrors)
        {
            Log($"Will TryDelete '{sourcePath}'");
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
                    Directory.Delete(sourcePath);
                    Log($"Dir deleted: '{sourcePath}'");
                }
                else
                {
                    //Если файл
                    File.Delete(sourcePath);
                    Log($"File deleted: '{sourcePath}'");
                }

            }
            catch (Exception ex)
            {
                Log($"Delete error: '{ex.Message}'\nPath: '{sourcePath}'");
                if (throwErrors)
                    throw;
            }
        }

        void Log(string msg)
        {
            _logger?.Invoke(msg);
        }
    }
}
