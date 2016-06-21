using System;
using System.IO;
using NUnit.Framework;

namespace tagster {

  [TestFixture]
  public class TagTBTests {

    private TagDB database;

    private String testDBFile;

    [SetUp]
    protected void SetUp() {
      testDBFile = System.IO.Path.GetTempFileName();
      database = new TagDB(testDBFile);
      Console.WriteLine(testDBFile);
    }

    [TearDown]
    protected void TearDown() {
      database = null;
      File.Delete(testDBFile);
    }

    [Test]
    public void AddFile() {
      database.AddFile("foo.mp4");
      database.ListFiles();
      Assert.That(database.ListFiles().Count == 2);
    }

  }
}