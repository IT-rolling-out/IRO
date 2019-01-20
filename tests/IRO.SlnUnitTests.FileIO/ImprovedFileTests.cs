using System;
using System.IO;
using IRO.FileIO.ImprovedFileOperations;
using NUnit.Framework;

namespace IRO.SlnUnitTests.FileIO
{
    public class ImprovedFileTests
    {
        const string File1 = "1.txt";
        const string Dir1 = "dir1";
        const string Dir1_File1 = "dir1/1.txt";
        const string Dir1_File2 = "dir1/2.txt";
        const string Dir1_Dir2 = "dir1/dir2";
        const string Dir1_Dir2_File1 = "dir1/dir2/1.txt";

        const string CopyDir1 = "copy_dir1";
        const string CopyDir1_File1 = "copy_dir1/1.txt";
        const string CopyDir1_File2 = "copy_dir1/2.txt";
        const string CopyDir1_Dir2 = "copy_dir1/dir2";
        const string CopyDir1_Dir2_File1 = "copy_dir1/dir2/1.txt";

        [SetUp]
        public void Setup()
        {
            DeleteDir(Dir1);
            DeleteDir(CopyDir1);

            Directory.CreateDirectory(CopyDir1);
            Directory.CreateDirectory(Dir1);
            Directory.CreateDirectory(Dir1_Dir2);

            if (!File.Exists(File1))
            {
                 File.WriteAllText(File1, "text");
            }
            File.WriteAllText(Dir1_File1, "text");
            File.WriteAllText(Dir1_File2, "text");
            File.WriteAllText(Dir1_Dir2_File1, "text");
        }

        [Test]
        public void DeleteTest()
        {
            ImprovedFile.Delete(File1);
            if (File.Exists(File1))
            {
                Assert.Fail("File wasn`t deleted.");
            }


            ImprovedFile.Delete(Dir1_File2);
            if (File.Exists(Dir1_File2))
            {
                Assert.Fail("File wasn`t deleted.");
            }

            ImprovedFile.Delete(Dir1);
            if (Directory.Exists(Dir1))
            {
                Assert.Fail("Dir wasn`t deleted.");
            }
        }

        [Test]
        public void DeleteMustFailTest()
        {
            var blockingStream = File.Open(Dir1_File1, FileMode.Open);
            try
            {
                ImprovedFile.Delete(Dir1);
                Assert.Fail("Delete method doesn`t throw error.");
            }
            catch
            {
                if (File.Exists(Dir1_File1))
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail("File deleted, but can`nt.");
                }
            }
            finally
            {
                blockingStream.Close();
            }
        }

        [Test]
        public void TryDeleteTest()
        {
            var blockingStream = File.Open(Dir1_File1, FileMode.Open);
            try
            {
                ImprovedFile.TryDelete(Dir1);
                if (File.Exists(Dir1_File1) && !File.Exists(Dir1_File2) && !File.Exists(Dir1_Dir2_File1))
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail();
                }
            }
            finally
            {
                blockingStream.Close();
            }
        }

        void DeleteDir(string dirName)
        {
            try
            {
                if (Directory.Exists(dirName))
                {
                    Directory.Delete(dirName, true);
                }

                if (Directory.Exists(dirName))
                {
                    throw new Exception();
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Can`t remove dir '{dirName}'.", ex);
            }
        }

        [Test]
        public void TryCopyTest()
        {
            var blockingStream = File.Open(Dir1_File1, FileMode.Open);
            try
            {
                ImprovedFile.TryCopy(Dir1, CopyDir1);
                if (!File.Exists(CopyDir1_File1) && File.Exists(CopyDir1_File2) && File.Exists(CopyDir1_Dir2_File1))
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail();
                }
            }
            finally
            {
                blockingStream.Close();
            }
        }

        [Test]
        public void CopyTest()
        {
            var blockingStream = File.Open(Dir1_File1, FileMode.Open);
            try
            {
                ImprovedFile.Copy(Dir1, CopyDir1);
                Assert.Fail("Doesn`t throw exception.");
            }
            catch
            {
                if (!File.Exists(CopyDir1_File1))
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail("Something went wrong.");
                }
                
            }
            finally
            {
                blockingStream.Close();
            }
        }
    }
}