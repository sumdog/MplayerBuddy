using System.Collections.Generic;

namespace tagster {

  public class Tag {
    public string Name { get; set; }
    public bool Set { get; set; }
  }

  public class FileTags {
    public TFile File { get; set; }
    public List<Tag> Tags { get; set; }
  }

  public class TFile {
    public long Id { get; set; }
    public string File { get; set; }
  }

}
