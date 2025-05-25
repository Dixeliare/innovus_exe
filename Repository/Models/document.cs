using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class document
{
    public int document_id { get; set; }

    public int? lesson { get; set; }

    public string? lesson_name { get; set; }

    public string link { get; set; } = null!;

    public int instrument_id { get; set; }

    public virtual instrument instrument { get; set; } = null!;

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
