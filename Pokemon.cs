using System;
using System.Collections.Generic;

namespace AzurePOC;

public partial class Pokemon
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string[] Types { get; set; } = null!;
}
