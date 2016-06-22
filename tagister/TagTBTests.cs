using System;
using System.IO;
using NUnit.Framework;
using System.Linq;

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
    public void AddFiles() {
      database.AddFile("foo.mp4");
      database.AddFile("bar.mp4");
      database.AddFile("foobar.mp4");
      database.ListFiles();
      Assert.That(database.ListFiles().Count == 3);
    }

    [Test]
    public void AddTags() {
      database.AddTag("tagA");
      database.AddTag("tabB");
      database.AddTag("tagC");
      database.AddTag("tagD");
      Assert.That(database.Tags.Count == 4);
      foreach(var v in database.TagUsage()) {
        Assert.That(v.Value == 0);
      }
    }

  }
}